using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using wServer.networking.packets.outgoing;
using wServer.realm.terrain;

namespace wServer.realm.entities
{
    public class UpdatedSet : HashSet<Entity>
    {
        private readonly Player _player;
        private readonly object _changeLock = new object();

        public UpdatedSet(Player player)
        {
            _player = player;
        }

        public new bool Add(Entity e)
        {
            using (TimedLock.Lock(_changeLock))
            {
                var added = base.Add(e);
                if (added)
                    e.StatChanged += _player.HandleStatChanges;

                return added;
            }
        }

        public new bool Remove(Entity e)
        {
            using (TimedLock.Lock(_changeLock))
            {
                e.StatChanged -= _player.HandleStatChanges;
                return base.Remove(e);
            }
        }

        public new void RemoveWhere(Predicate<Entity> match)
        {
            using (TimedLock.Lock(_changeLock))
            {
                foreach (var e in this.Where(match.Invoke))
                    e.StatChanged -= _player.HandleStatChanges;

                base.RemoveWhere(match);
            }
        }

        public void Dispose()
        {
            RemoveWhere(e => true);
        }
    }

    public partial class Player
    {
        public HashSet<Entity> clientEntities => _clientEntities;

        public readonly ConcurrentQueue<Entity> ClientKilledEntity = new ConcurrentQueue<Entity>();

        public const int Radius = 20;
        public const int RadiusSqr = Radius * Radius;
        private const int StaticBoundingBox = Radius * 2;
        private const int AppoxAreaOfSight = (int)(Math.PI * Radius * Radius + 1);

        private readonly HashSet<IntPoint> _clientStatic = new HashSet<IntPoint>();
        private readonly UpdatedSet _clientEntities;
        private ObjectStats[] _updateStatuses;
        private Update.TileData[] _tiles;
        private ObjectDef[] _newObjects;
        private int[] _removedObjects;

        private readonly object _statUpdateLock = new object();
        private readonly Dictionary<Entity, Dictionary<StatsType, object>> _statUpdates = 
            new Dictionary<Entity, Dictionary<StatsType, object>>(); 

        public Sight Sight { get; private set; }

        public int TickId;

        public void HandleStatChanges(object entity, StatChangedEventArgs statChange)
        {
            var e = entity as Entity;
            if (e == null || e != this && statChange.UpdateSelfOnly)
                return;

            using (TimedLock.Lock(_statUpdateLock))
            {
                if (e == this && statChange.Stat == StatsType.None)
                    return;
                
                if (!_statUpdates.ContainsKey(e))
                    _statUpdates[e] = new Dictionary<StatsType, object>();

                if (statChange.Stat != StatsType.None)
                    _statUpdates[e][statChange.Stat] = statChange.Value;

                //Log.Info($"{entity} {statChange.Stat} {statChange.Value}");
            }
        }

        private void SendNewTick(RealmTime time)
        {
            using (TimedLock.Lock(_statUpdateLock))
            {
                _updateStatuses = _statUpdates.Select(_ => new ObjectStats()
                {
                    Id = _.Key.Id,
                    Position = new Position() { X = _.Key.RealX, Y = _.Key.RealY },
                    Stats = _.Value.ToArray()
                }).ToArray();
                _statUpdates.Clear();
            }
            
            _client.SendPacket(new NewTick
            {
                TickId = ++TickId,
                TickTime = time.ElaspedMsDelta,
                Statuses = _updateStatuses
            });
            AwaitMove(TickId);
        }

        private void SendUpdate(RealmTime time)
        {
            // init sight circle
            var sCircle = Sight.GetSightCircle(Owner.Blocking);

            // get list of tiles for update
            var tilesUpdate = new List<Update.TileData>(AppoxAreaOfSight);
            foreach (var point in sCircle)
            {
                var x = point.X;
                var y = point.Y;
                var tile = Owner.Map[x, y];

                if (tile.TileId == 255 ||
                    tiles[x, y] >= tile.UpdateCount)
                    continue;

                tilesUpdate.Add(new Update.TileData()
                {
                    X = (short)x,
                    Y = (short)y,
                    Tile = (Tile)tile.TileId
                });
                tiles[x, y] = tile.UpdateCount;
            }
            FameCounter.TileSent(tilesUpdate.Count);
            
            // get list of new static objects to add
            var staticsUpdate = GetNewStatics(sCircle).ToArray();

            // get dropped entities list
            var entitiesRemove = new HashSet<int>(GetRemovedEntities(sCircle));

            // removed stale entities
            _clientEntities.RemoveWhere(e => entitiesRemove.Contains(e.Id));

            // get list of added entities
            var entitiesAdd = GetNewEntities(sCircle).ToArray();

            // get dropped statics list
            var staticsRemove = new HashSet<IntPoint>(GetRemovedStatics(sCircle));
            _clientStatic.ExceptWith(staticsRemove);

            if (tilesUpdate.Count > 0 || entitiesRemove.Count > 0 || staticsRemove.Count > 0 ||
                entitiesAdd.Length > 0 || staticsUpdate.Length > 0)
            {
                entitiesRemove.UnionWith(
                    staticsRemove.Select(s => Owner.Map[s.X, s.Y].ObjId));

                _tiles = tilesUpdate.ToArray();
                _newObjects = entitiesAdd.Select(_ => _.ToDefinition()).Concat(staticsUpdate).ToArray();
                _removedObjects = entitiesRemove.ToArray();
                _client.SendPacket(new Update
                {
                    Tiles = _tiles,
                    NewObjs = _newObjects,
                    Drops = _removedObjects
                });
                AwaitUpdateAck(time.TotalElapsedMs);
            }
        }

        private IEnumerable<int> GetRemovedEntities(HashSet<IntPoint> visibleTiles)
        {
            foreach (var e in ClientKilledEntity)
                yield return e.Id;

            foreach (var i in _clientEntities)
            {
                if (i.Owner == null)
                    yield return i.Id;

                if (i != this &&  !i.CanBeSeenBy(this))
                    yield return i.Id;

                var so = i as StaticObject;
                if (so != null && so.Static)
                {
                    if (Math.Abs(StaticBoundingBox - ((int)X - i.X)) > 0 &&
                        Math.Abs(StaticBoundingBox - ((int)Y - i.Y)) > 0)
                        continue;
                }

                if (i is Player ||
                    i == questEntity || i == SpectateTarget || /*(i is StaticObject && (i as StaticObject).Static) ||*/
                    visibleTiles.Contains(new IntPoint((int)i.X, (int)i.Y)))
                    continue;

                yield return i.Id;
            }
        }

        private IEnumerable<Entity> GetNewEntities(HashSet<IntPoint> visibleTiles)
        {
            Entity entity;
            while (ClientKilledEntity.TryDequeue(out entity))
                _clientEntities.Remove(entity);

            foreach (var i in Owner.Players)
                if ((i.Value == this || (i.Value.Client.Account != null && i.Value.Client.Player.CanBeSeenBy(this))) && _clientEntities.Add(i.Value))
                    yield return i.Value;

            foreach (var i in Owner.PlayersCollision.HitTest(X, Y, Radius))
                if ((i is Decoy || i is Pet) && _clientEntities.Add(i))
                    yield return i;

            var p = new IntPoint(0, 0);
            foreach (var i in Owner.EnemiesCollision.HitTest(X, Y, Radius))
            {
                if (i is Container)
                {
                    int[] owners = (i as Container).BagOwners;
                    if (owners.Length > 0 && Array.IndexOf(owners, AccountId) == -1)
                        continue;
                }

                p.X = (int)i.X;
                p.Y = (int)i.Y;
                if (visibleTiles.Contains(p) && _clientEntities.Add(i))
                    yield return i;
            }

            if (questEntity?.Owner != null && _clientEntities.Add(questEntity))
                yield return questEntity;

            if (SpectateTarget?.Owner != null && _clientEntities.Add(SpectateTarget))
                yield return SpectateTarget;
        }

        private IEnumerable<IntPoint> GetRemovedStatics(HashSet<IntPoint> visibleTiles)
        {
            foreach (var i in _clientStatic)
            {
                var tile = Owner.Map[i.X, i.Y];

                if (/*visibleTiles.Contains(i)*/
                    StaticBoundingBox - ((int)X - i.X) > 0 &&
                    StaticBoundingBox - ((int)Y - i.Y) > 0 &&
                    tile.ObjType != 0 &&
                    tile.ObjId != 0)
                    continue;

                yield return i;
            }
        }

        private readonly List<ObjectDef> _newStatics = new List<ObjectDef>(AppoxAreaOfSight);
        private IEnumerable<ObjectDef> GetNewStatics(HashSet<IntPoint> visibleTiles)
        {
            _newStatics.Clear();

            foreach (var i in visibleTiles)
            {
                var x = i.X;
                var y = i.Y;
                var tile = Owner.Map[x, y];

                if (tile.ObjId != 0 && tile.ObjType != 0 && _clientStatic.Add(i))
                    _newStatics.Add(tile.ToDef(x, y));
            }

            return _newStatics;
        }
    }
}