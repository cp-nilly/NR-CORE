using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using common.resources;
using wServer.realm;
using Mono.Game;
using wServer.realm.entities;

namespace wServer.logic.behaviors
{
    class StayBack : CycleBehavior
    {
        //State storage: cooldown timer

        float speed;
        float distance;
        string entity;

        public StayBack(double speed, double distance = 8, string entity = null)
        {
            this.speed = (float)speed;
            this.distance = (float)distance;
            this.entity = entity;
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            int cooldown;
            if (state == null) cooldown = 1000;
            else cooldown = (int)state;

            Status = CycleStatus.NotStarted;

            if (host.HasConditionEffect(ConditionEffects.Paralyzed))
                return;

            Entity e = entity != null ? 
                host.GetNearestEntityByName(distance, entity) : 
                host.GetNearestEntity(distance, null);

            if (e != null)
            {
                Vector2 vect;
                vect = new Vector2(e.X - host.X, e.Y - host.Y);
                vect.Normalize();
                float dist = host.GetSpeed(speed) * (time.ElaspedMsDelta / 1000f);
                host.ValidateAndMove(host.X + (-vect.X) * dist, host.Y + (-vect.Y) * dist);

                if (cooldown <= 0)
                {
                    Status = CycleStatus.Completed;
                    cooldown = 1000;
                }
                else
                {
                    Status = CycleStatus.InProgress;
                    cooldown -= time.ElaspedMsDelta;
                }
            }

            state = cooldown;
        }
    }
}
