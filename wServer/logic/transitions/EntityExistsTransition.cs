using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using common;
using wServer.realm.entities;

namespace wServer.logic.transitions
{
    class EntityExistsTransition : Transition
    {
        //State storage: none

        readonly double _dist;
        readonly ushort _target;

        public EntityExistsTransition(string target, double dist, string targetState)
            : base(targetState)
        {
            _dist = dist;
            _target = Behavior.GetObjType(target);
        }

        protected override bool TickCore(Entity host, RealmTime time, ref object state)
        {
            return host.GetNearestEntity(_dist, _target) != null;
        }
    }
}