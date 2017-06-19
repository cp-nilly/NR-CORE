using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors
{
    class Duration : Behavior
    {

        Behavior child;
        int duration;
        public Duration(Behavior child, int duration)
        {
            this.child = child;
            this.duration = duration;
        }

        protected override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {
             child.OnStateEntry(host, time);
             state = 0;
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            int timeElapsed = (int)state;
            if (timeElapsed <= duration)
            {
                child.Tick(host, time);
                timeElapsed += time.ElaspedMsDelta;
            }
            state = timeElapsed;
        }
    }
}
