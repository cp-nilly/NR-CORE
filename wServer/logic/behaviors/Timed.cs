using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;

namespace wServer.logic.behaviors
{
    //replacement for simple timed transition in sequence
    class Timed : CycleBehavior
    {
        //State storage: time

        Behavior[] behaviors;
        int period;
        public Timed(int period, params Behavior[] behaviors)
        {
            this.behaviors = behaviors;
            this.period = period;
        }

        protected override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {
            foreach(var behavior in behaviors)
                behavior.OnStateEntry(host, time);
            state = period;
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            int period = (int)state;
            
                foreach (Behavior behavior in behaviors)
                {   behavior.Tick(host, time);
                Status = CycleStatus.InProgress;

                period -= time.ElaspedMsDelta;
                if (period <= 0)
                {
                    period = this.period;
                    Status = CycleStatus.Completed;
                    //......- -
                    if (behavior is Prioritize)
                        host.StateStorage[behavior] = -1;
                }
            }
            state = period;
        }
    }
}
