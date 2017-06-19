using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using common.resources;
using wServer.realm;

namespace wServer.logic.behaviors
{
    class BackAndForth : CycleBehavior
    {
        //State storage: remaining distance

        float speed;
        int distance;
        public BackAndForth(double speed, int distance = 5)
        {
            this.speed = (float)speed;
            this.distance = distance;
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            float dist;
            if (state == null) dist = distance;
            else dist = (float)state;

            Status = CycleStatus.NotStarted;

            if (host.HasConditionEffect(ConditionEffects.Paralyzed)) 
                return;

            float moveDist = host.GetSpeed(speed) * (time.ElaspedMsDelta / 1000f);
            if (dist > 0)
            {
                Status = CycleStatus.InProgress;
                host.ValidateAndMove(host.X + moveDist, host.Y);
                dist -= moveDist;
                if (dist <= 0)
                {
                    dist = -distance;
                    Status = CycleStatus.Completed;
                }
            }
            else
            {
                Status = CycleStatus.InProgress;
                host.ValidateAndMove(host.X - moveDist, host.Y);
                dist += moveDist;
                if (dist >= 0)
                {
                    dist = distance;
                    Status = CycleStatus.Completed;
                }
            }

            state = dist;
        }
    }
}
