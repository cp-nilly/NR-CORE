using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.transitions
{
    class NoPlayerWithinTransition : Transition
    {
        //State storage: none

        double dist;

        public NoPlayerWithinTransition(double dist, string targetState)
            : base(targetState)
        {
            this.dist = dist;
        }

        protected override bool TickCore(Entity host, RealmTime time, ref object state)
        {
            return host.GetNearestEntity(dist, null) == null;
        }
    }
}
