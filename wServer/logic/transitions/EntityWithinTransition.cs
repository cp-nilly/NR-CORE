using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.transitions
{
    class EntityWithinTransition : Transition
    {
        //State storage: none

        private readonly double _dist;
        private readonly string _entity;

        public EntityWithinTransition(double dist, string entity, string targetState)
            : base(targetState)
        {
            _dist = dist;
            _entity = entity;
        }

        protected override bool TickCore(Entity host, RealmTime time, ref object state)
        {
            return host.GetNearestEntityByName(_dist, _entity) != null;
        }
    }
}
