using System;
using System.Collections.Generic;
using System.Linq;
using common.resources;
using wServer.logic.transitions;
using wServer.realm.entities;
using wServer.logic;
using log4net;
using wServer.realm.entities.vendors;
using wServer.realm.worlds;

namespace wServer.realm
{
    public class Entity : IProjectileOwner, ICollidable<Entity>
    {
        private const int EffectCount = 51;

        protected static readonly ILog Log = LogManager.GetLogger(typeof(Entity));
        
        public RealmManager Manager { get; }
        public World Owner { get; private set; }
        public int Id { get; internal set; }
        public ushort ObjectType { get; protected set; }
        public Player AttackTarget { get; set; }
        public int LootValue { get; set; } = 1;
        public event EventHandler FocusLost;
        public Player Controller;
        public CollisionNode<Entity> CollisionNode { get; set; }
        public CollisionMap<Entity> Parent { get; set; }
        public event EventHandler<StatChangedEventArgs> StatChanged;

        private readonly Position[] _posHistory;
        private byte _posIdx;
        private readonly int[] _effects;
        private bool _tickingEffects;

        private readonly ObjectDesc _desc;
        public ObjectDesc ObjectDesc => _desc;

        public bool Spawned;
        public bool GivesNoXp;
        private int _originalSize;

        private readonly SV<string> _name;
        private readonly SV<int> _size;
        private readonly SV<int> _altTextureIndex;
        private readonly SV<float> _x;
        private readonly SV<float> _y;
        private readonly SV<int> _conditionEffects1;
        private readonly SV<int> _conditionEffects2;
        private ConditionEffects _conditionEffects;

        public string Name
        {
            get { return _name.GetValue(); }
            set { _name.SetValue(value); }
        }
        public int Size
        {
            get { return _size.GetValue(); }
            set { _size.SetValue(value); }
        }
        public int AltTextureIndex
        {
            get { return _altTextureIndex.GetValue(); }
            set { _altTextureIndex.SetValue(value); }
        }
        public ConditionEffects ConditionEffects
        {
            get { return _conditionEffects; }
            set
            {
                _conditionEffects = value;
                _conditionEffects1?.SetValue((int)value);
                _conditionEffects2?.SetValue((int)((ulong)value >> 31));
            }
        }

        public float RealX => _x.GetValue();
        public float RealY => _y.GetValue();
        public float X
        {
            get
            {
                var player = this as Player;
                return player?.SpectateTarget?.RealX ?? _x.GetValue();
            }
            private set { _x.SetValue(value); }
        }
        public float Y
        {
            get
            {
                var player = this as Player;
                return player?.SpectateTarget?.RealY ?? _y.GetValue();
            }
            private set { _y.SetValue(value); }
        }
        
        Entity IProjectileOwner.Self => this;
        private readonly Projectile[] _projectiles;
        Projectile[] IProjectileOwner.Projectiles => _projectiles;
        protected byte projectileId;
        
        public bool TickStateManually { get; set; }
        public State CurrentState { get; private set; }
        private bool _stateEntry;
        private State _stateEntryCommonRoot;
        private Dictionary<object, object> _states;
        public IDictionary<object, object> StateStorage
        {
            get
            {
                if (_states == null) _states = new Dictionary<object, object>();
                return _states;
            }
        }
        
        protected Entity(RealmManager manager, ushort objType)
        {
            _name = new SV<string>(this, StatsType.Name, "");
            _size = new SV<int>(this, StatsType.Size, 100);
            _originalSize = 100;
            _altTextureIndex = new SV<int>(this, StatsType.AltTextureIndex, -1);
            _x = new SV<float>(this, StatsType.None, 0);
            _y = new SV<float>(this, StatsType.None, 0);
            _conditionEffects1 = new SV<int>(this, StatsType.Effects, 0);
            _conditionEffects2 = new SV<int>(this, StatsType.Effects2, 0);
            
            ObjectType = objType;
            Manager = manager;
            manager.Behaviors.ResolveBehavior(this);
            manager.Resources.GameData.ObjectDescs.TryGetValue(ObjectType, out _desc);
            if (_desc == null)
                return;

            if (_desc.Player)
            {
                _posHistory = new Position[256];
                _projectiles = new Projectile[256];
                _effects = new int[EffectCount];
                return;
            }

            if (_desc.Enemy && !_desc.Static)
            {
                _projectiles = new Projectile[256];
                _effects = new int[EffectCount];
                return;
            }

            if (_desc.Character || _desc.IsPet)
            {
                _effects = new int[EffectCount];
                return;
            }
        }

        protected virtual void ImportStats(StatsType stats, object val)
        {
            if (stats == StatsType.Name) Name = (string)val;
            else if (stats == StatsType.Size) Size = (int)val;
            else if (stats == StatsType.AltTextureIndex) AltTextureIndex = (int)val;
            else if (stats == StatsType.Effects) ConditionEffects = (ConditionEffects)(ulong)val;
        }

        protected virtual void ExportStats(IDictionary<StatsType, object> stats)
        {
            stats[StatsType.Name] = Name;
            stats[StatsType.Size] = Size;
            stats[StatsType.AltTextureIndex] = AltTextureIndex;
            stats[StatsType.Effects] = _conditionEffects1.GetValue();
            stats[StatsType.Effects2] = _conditionEffects2.GetValue();
        }

        public void FromDefinition(ObjectDef def)
        {
            ObjectType = def.ObjectType;
            ImportStats(def.Stats);
        }

        public void ImportStats(ObjectStats stat)
        {
            Id = stat.Id;
            (this is Enemy ? Owner.EnemiesCollision : Owner.PlayersCollision)
                .Move(this, stat.Position.X, stat.Position.Y);
            X = stat.Position.X;
            Y = stat.Position.Y;
            foreach (var i in stat.Stats)
                ImportStats(i.Key, i.Value);
        }

        public ObjectStats ExportStats()
        {
            var stats = new Dictionary<StatsType, object>();
            ExportStats(stats);
            
            return new ObjectStats()
            {
                Id = Id,
                Position = new Position() { X = RealX, Y = RealY },
                Stats = stats.ToArray()
            };
        }

        public ObjectDef ToDefinition()
        {
            return new ObjectDef()
            {
                ObjectType = ObjectType,
                Stats = ExportStats()
            };
        }

        public virtual void Init(World owner)
        {
            Owner = owner;
        }

        public virtual void Tick(RealmTime time)
        {
            if (this is Projectile || Owner == null) return;
            if (CurrentState != null && Owner != null)
            {
                if (!HasConditionEffect(ConditionEffects.Stasis) &&
                    !TickStateManually &&
                    (this.AnyPlayerNearby() || ConditionEffects != 0))
                    TickState(time);
            }
            if (_posHistory != null)
                _posHistory[++_posIdx] = new Position() { X = X, Y = Y };
            if (_effects != null)
                ProcessConditionEffects(time);
        }

        public void SwitchTo(State state)
        {
            var origState = CurrentState;

            CurrentState = state;
            GoDeeeeeeeep();

            _stateEntryCommonRoot = State.CommonParent(origState, CurrentState);
            _stateEntry = true;
        }

        void GoDeeeeeeeep()
        {
            //always the first deepest sub-state
            if (CurrentState == null) return;
            while (CurrentState.States.Count > 0)
                CurrentState = CurrentState = CurrentState.States[0];
        }

        public void TickState(RealmTime time)
        {
            if (_stateEntry)
            {
                //State entry
                var s = CurrentState;
                while (s != null && s != _stateEntryCommonRoot)
                {
                    foreach (var i in s.Behaviors)
                        i.OnStateEntry(this, time);
                    s = s.Parent;
                }
                _stateEntryCommonRoot = null;
                _stateEntry = false;
            }

            var origState = CurrentState;
            var state = CurrentState;
            bool transited = false;
            while (state != null)
            {
                if (!transited)
                    foreach (var i in state.Transitions)
                        if (i.Tick(this, time))
                        {
                            transited = true;
                            break;
                        }

                foreach (var i in state.Behaviors)
                {
                    if (Owner == null) break;
                    i.Tick(this, time);
                }
                if (Owner == null) break;

                state = state.Parent;
            }
            if (transited)
            {
                //State exit
                var s = origState;
                while (s != null && s != _stateEntryCommonRoot)
                {
                    foreach (var i in s.Behaviors)
                        i.OnStateExit(this, time);
                    s = s.Parent;
                }
            }
        }

        class FPoint
        {
            public float X;
            public float Y;
        }

        public void ValidateAndMove(float x, float y)
        {
            if (Owner == null)
                return;

            var pos = new FPoint();
            ResolveNewLocation(x, y, pos);
            Move(pos.X, pos.Y);
        }

        private void ResolveNewLocation(float x, float y, FPoint pos)
        {
            if (HasConditionEffect(ConditionEffects.Paralyzed) ||
                HasConditionEffect(ConditionEffects.Petrify))
            {
                pos.X = X;
                pos.Y = Y;
                return;
            }

            var dx = x - X;
            var dy = y - Y;

            const float colSkipBoundary = .4f;
            if (dx < colSkipBoundary &&
                dx > -colSkipBoundary &&
                dy < colSkipBoundary &&
                dy > -colSkipBoundary)
            {
                CalcNewLocation(x, y, pos);
                return;
            }

            var ds = colSkipBoundary / Math.Max(Math.Abs(dx), Math.Abs(dy));
            var tds = 0f;

            pos.X = X;
            pos.Y = Y;

            var done = false;
            while (!done)
            {
                if (tds + ds >= 1)
                {
                    ds = 1 - tds;
                    done = true;
                }

                CalcNewLocation(pos.X + dx * ds, pos.Y + dy * ds, pos);
                tds = tds + ds;
            }
        }

        private void CalcNewLocation(float x, float y, FPoint pos)
        {
            float fx = 0;
            float fy = 0;

            var isFarX = (X % .5f == 0 && x != X) || (int) (X / .5f) != (int) (x / .5f);
            var isFarY = (Y % .5f == 0 && y != Y) || (int) (Y / .5f) != (int) (y / .5f);
            
            if ((!isFarX && !isFarY) || RegionUnblocked(x, y))
            {
                pos.X = x;
                pos.Y = y;
                return;
            }

            if (isFarX)
            {
                fx = (x > X) ? (int) (x * 2) / 2f : (int) (X * 2) / 2f;
                if ((int) fx > (int) X)
                    fx = fx - 0.01f;
            }

            if (isFarY)
            {
                fy = (y > Y) ? (int) (y * 2) / 2f : (int) (Y * 2) / 2f;
                if ((int) fy > (int) Y)
                    fy = fy - 0.01f;
            }
			
            if (!isFarX)
            {
                pos.X = x;
                pos.Y = fy;
                return;
            }
			
            if (!isFarY)
            {
                pos.X = fx;
                pos.Y = y;
                return;
            }

            var ax = (x > X) ? x - fx : fx - x;
            var ay = (y > Y) ? y - fy : fy - y;
            if (ax > ay)
            {
                if (RegionUnblocked(x, fy))
                {
                    pos.X = x;
                    pos.Y = fy;
                    return;
                }

                if (RegionUnblocked(fx, y))
                {
                    pos.X = fx;
                    pos.Y = y;
                    return;
                }
            }
            else
            {
                if (RegionUnblocked(fx, y))
                {
                    pos.X = fx;
                    pos.Y = y;
                    return;
                }

                if (RegionUnblocked(x, fy))
                {
                    pos.X = x;
                    pos.Y = fy;
                    return;
                }
            }
			
            pos.X = fx;
            pos.Y = fy;
        }

        private bool RegionUnblocked(float x, float y)
        {
            if (TileOccupied(x, y))
                return false;
			
            var xFrac = x - (int) x;
            var yFrac = y - (int) y;

            if (xFrac < 0.5)
            {
                if (TileFullOccupied(x - 1, y))
                    return false;
                
                if (yFrac < 0.5)
                {
                    if (TileFullOccupied(x, y - 1) || TileFullOccupied(x - 1, y - 1))
                        return false;
                }
                else
                {
                    if (yFrac > 0.5)
                        if (TileFullOccupied(x, y + 1) || TileFullOccupied(x - 1, y + 1))
                            return false;
                }

                return true;
            }

            if (xFrac > 0.5)
            {
                if (TileFullOccupied(x + 1, y))
                    return false;
                    
                if (yFrac < 0.5)
                {
                    if (TileFullOccupied(x, y - 1) || TileFullOccupied(x + 1, y - 1))
                        return false;
                }
                else
                {
                    if (yFrac > 0.5)
                        if (TileFullOccupied(x, y + 1) || TileFullOccupied(x + 1, y + 1))
                            return false;
                }

                return true;
            }

            if (yFrac < 0.5)
            {
                if (TileFullOccupied(x, y - 1))
                    return false;

                return true;
            }

            if (yFrac > 0.5)
                if (TileFullOccupied(x, y + 1))
                    return false;

            return true;
        }
        
        public bool TileOccupied(float x, float y)
        {
            var x_ = (int) x;
            var y_ = (int) y;

            var map = Owner.Map;

            if (!map.Contains(x_, y_))
                return true;

            var tile = map[x_, y_];

            var tileDesc = Manager.Resources.GameData.Tiles[tile.TileId];
            if (tileDesc?.NoWalk == true)
                return true;
            
            if (tile.ObjType != 0)
            {
                var objDesc = Manager.Resources.GameData.ObjectDescs[tile.ObjType];
                if (objDesc?.EnemyOccupySquare == true)
                    return true;
            }

            return false;
        }

        public bool TileFullOccupied(float x, float y)
        {
            var xx = (int) x;
            var yy = (int) y;

            if (!Owner.Map.Contains(xx, yy))
                return true;

            var tile = Owner.Map[xx, yy];

            if (tile.ObjType != 0)
            {
                var objDesc = Manager.Resources.GameData.ObjectDescs[tile.ObjType];
                if (objDesc?.FullOccupy == true)
                    return true;
            }
            return false;
        }

        public virtual void Move(float x, float y)
        {
            if (Controller != null)
                return;

            MoveEntity(x, y);
        }

        public void MoveEntity(float x, float y)
        {
            if (Owner != null && !(this is Projectile) && !(this is Pet) && (!(this is StaticObject) || (this as StaticObject).Hittestable))
                ((this is Enemy || this is StaticObject && !(this is Decoy)) ? Owner.EnemiesCollision : Owner.PlayersCollision)
                    .Move(this, x, y);
            X = x; Y = y;
        }

        public Position? TryGetHistory(long ticks)
        {
            if (_posHistory == null) return null;
            if (ticks > 255) return null;
            return _posHistory[(byte)(_posIdx - (byte)ticks)];
        }

        public static Entity Resolve(RealmManager manager, string name)
        {
            ushort id;
            if (!manager.Resources.GameData.IdToObjectType.TryGetValue(name, out id))
                return null;

            return Resolve(manager, id);
        }

        public static Entity Resolve(RealmManager manager, ushort id)
        {
            var node = manager.Resources.GameData.ObjectTypeToElement[id];
            string type = node.Element("Class").Value;
            switch (type)
            {
                case "Projectile":
                    throw new Exception("Projectile should not instantiated using Entity.Resolve");
                case "Sign":
                    return new Sign(manager, id);
                case "Wall":
                case "DoubleWall":
                    return new Wall(manager, id, node);
                case "ConnectedWall":
                case "CaveWall":
                    return new ConnectedObject(manager, id);
                case "GameObject":
                case "CharacterChanger":
                case "MoneyChanger":
                case "NameChanger":
                    return new StaticObject(manager, id, StaticObject.GetHP(node), true, false, true);
                case "GuildRegister":
                case "GuildChronicle":
                case "GuildBoard":
                    return new StaticObject(manager, id, null, false, false, false);
                case "Container":
                    return new Container(manager, id);
                case "Player":
                    throw new Exception("Player should not instantiated using Entity.Resolve");
                case "Character":   //Other characters means enemy
                    return new Enemy(manager, id);
                case "ArenaPortal":
                case "Portal":
                    return new Portal(manager, id, null);
                case "GuildHallPortal":
                    return new GuildHallPortal(manager, id, null);
                case "ClosedVaultChest":
                    return new ClosedVaultChest(manager, id);
                case "ClosedVaultChestGold":
                case "ClosedGiftChest":
                case "VaultChest":
                case "Merchant":
                    return new WorldMerchant(manager, id);
                case "GuildMerchant":
                    return new GuildMerchant(manager, id);
                case "ArenaGuard":
                case "MysteryBoxGround":
                case "ReskinVendor":
                case "PetUpgrader":
                case "FortuneTeller":
                case "YardUpgrader":
                case "FortuneGround":
                case "MarketNPC":
                case "QuestRewards":
                    return new StaticObject(manager, id, null, true, false, false);
                case "Pet":
                    return new Pet(manager, id);
                default:
                    Log.WarnFormat("Not supported type: {0}", type);
                    return new Entity(manager, id);
            }
        }

        public Projectile CreateProjectile(ProjectileDesc desc, ushort container, int dmg, long time, Position pos, float angle)
        {
            var ret = new Projectile(Manager, desc) //Assume only one
            {
                ProjectileOwner = this,
                ProjectileId = projectileId++,
                Container = container,
                Damage = dmg,

                CreationTime = time,
                StartPos = pos,
                Angle = angle,

                X = pos.X,
                Y = pos.Y
            };
            if (_projectiles[ret.ProjectileId] != null)
                _projectiles[ret.ProjectileId].Destroy();
            _projectiles[ret.ProjectileId] = ret;
            return ret;
        }

        public virtual bool HitByProjectile(Projectile projectile, RealmTime time)
        {
            if (ObjectDesc == null)
                return true;

            return ObjectDesc.Enemy || ObjectDesc.Player;
        }
        
        void ProcessConditionEffects(RealmTime time)
        {
            if (_effects == null || !_tickingEffects) return;

            ConditionEffects newEffects = 0;
            _tickingEffects = false;
            for (int i = 0; i < _effects.Length; i++)
            {
                if (_effects[i] > 0)
                {
                    _effects[i] -= time.ElaspedMsDelta;
                    if (_effects[i] > 0)
                    {
                        newEffects |= (ConditionEffects)((ulong)1 << i);
                        _tickingEffects = true;
                    }
                    else
                    {
                        _effects[i] = 0;
                    }
                }
                else if (_effects[i] == -1)
                {
                    newEffects |= (ConditionEffects)((ulong)1 << i);
                }
                    
            }

            ConditionEffects = newEffects;
        }

        public bool HasConditionEffect(ConditionEffects eff)
        {
            return (ConditionEffects & eff) != 0;
        }

        public void ApplyConditionEffect(params ConditionEffect[] effs)
        {
            foreach (var i in effs)
            {
                if (!ApplyCondition(i.Effect))
                    continue;

                var eff = (int)i.Effect;

                _effects[eff] = i.DurationMS;
                if (i.DurationMS != 0)
                    ConditionEffects |= (ConditionEffects)((ulong)1 << eff);
            }

            _tickingEffects = true;
        }

        public void ApplyConditionEffect(ConditionEffectIndex effect, int durationMs = -1)
        {
            if (!ApplyCondition(effect))
                return;

            var eff = (int)effect;

            _effects[eff] = durationMs;
            if (durationMs != 0)
                ConditionEffects |= (ConditionEffects)((ulong)1 << eff);

            _tickingEffects = true;
        }

        private bool ApplyCondition(ConditionEffectIndex effect)
        {
            if (effect == ConditionEffectIndex.Stunned &&
                HasConditionEffect(ConditionEffects.StunImmume))
                return false;

            if (effect == ConditionEffectIndex.Stasis &&
                HasConditionEffect(ConditionEffects.StasisImmune))
                return false;

            if (effect == ConditionEffectIndex.Paralyzed &&
                HasConditionEffect(ConditionEffects.ParalyzeImmune))
                return false;

            if (effect == ConditionEffectIndex.ArmorBroken &&
                HasConditionEffect(ConditionEffects.ArmorBreakImmune))
                return false;

            if (effect == ConditionEffectIndex.Curse &&
                HasConditionEffect(ConditionEffects.CurseImmune))
                return false;

            if (effect == ConditionEffectIndex.Petrify &&
                HasConditionEffect(ConditionEffects.PetrifyImmune))
                return false;

            if (effect == ConditionEffectIndex.Dazed &&
                HasConditionEffect(ConditionEffects.DazedImmune))
                return false;

            if (effect == ConditionEffectIndex.Slowed &&
                HasConditionEffect(ConditionEffects.SlowedImmune))
                return false;

            return true;
        }

        public void OnChatTextReceived(Player player, string text)
        {
            var state = CurrentState;
            while (state != null)
            {
                foreach (var t in state.Transitions.OfType<PlayerTextTransition>())
                    t.OnChatReceived(player, text);
                state = state.Parent;
            }
        }

        public void InvokeStatChange(StatsType t, object val, bool updateSelfOnly = false)
        {
            StatChanged?.Invoke(this, new StatChangedEventArgs(t, val, updateSelfOnly));
        }

        public virtual void Dispose()
        {
            Owner = null;
            FocusLost?.Invoke(this, EventArgs.Empty);
        }

        public virtual bool CanBeSeenBy(Player player)
        {
            return !HasConditionEffect(ConditionEffects.Hidden);
        }

        public void SetDefaultSize(int size)
        {
            _originalSize = size;
            Size = size;
        }

        public void RestoreDefaultSize()
        {
            Size = _originalSize;
        }
    }
}