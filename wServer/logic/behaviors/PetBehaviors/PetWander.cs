using Mono.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using common.resources;
using wServer.realm;
using wServer.realm.entities;
using wServer.realm.terrain;
using wServer.realm.worlds;
using wServer.realm.worlds.logic;

namespace wServer.logic.behaviors.PetBehaviors
{
    internal class PetWander : CycleBehavior
    {
        //State storage: wander state
        private readonly float speed;
        private Cooldown coolDown;
        private Vector2 spawnPoint;

        public PetWander(double speed, Cooldown coolDown)
        {
            this.speed = (float)speed;
            this.coolDown = coolDown;
        }

        protected override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {
            spawnPoint = new Vector2(host.X, host.Y);
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            if ((host as Pet)?.PlayerOwner != null) return;

            var map = host.Owner.Map;
            var x = (int) host.X;
            var y = (int) host.Y;
            WmapTile tile = null;

            if (map.Contains(x, y))
            {
                tile = map[x, y];
            }
            
            if (tile == null || (tile.Region == TileRegion.None && host.Owner is PetYard))
            {
                host.Move(spawnPoint.X, spawnPoint.Y);
                return;
            }
            
            if (host.GetNearestEntity(1, null) != null) return;
            WanderStorage storage;
            if (state == null) storage = new WanderStorage();
            else storage = (WanderStorage)state;

            Status = CycleStatus.NotStarted;

            if (host.HasConditionEffect(ConditionEffects.Paralyzed)) return;

            Status = CycleStatus.InProgress;
            if (storage.RemainingDistance <= 0)
            {
                storage.Direction = new Vector2(Random.Next(-2, 2), Random.Next(-2, 2));
                storage.Direction.Normalize();
                storage.RemainingDistance = coolDown.Next(Random) / 1000f;
                Status = CycleStatus.Completed;
            }

            float dist = host.GetSpeed(speed) * (time.ElaspedMsDelta / 1000f);
            host.ValidateAndMove(host.X + storage.Direction.X * dist, host.Y + storage.Direction.Y * dist);

            storage.RemainingDistance -= dist;

            state = storage;
        }

        private class WanderStorage
        {
            public Vector2 Direction;
            public float RemainingDistance;
        }
    }
}
