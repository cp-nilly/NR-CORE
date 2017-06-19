using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.transitions
{
    class EntityHpLessTransition : Transition
    {
        //State storage: none

        private readonly double _dist;
        private readonly string _entity;
        private readonly double _threshold;

        public EntityHpLessTransition(double dist, string entity, double threshold, string targetState)
            : base(targetState)
        {
            _dist = dist;
            _entity = entity;
            _threshold = threshold;
        }

        protected override bool TickCore(Entity host, RealmTime time, ref object state)
        {
            var entity = host.GetNearestEntityByName(_dist, _entity);

            if (entity == null)
                return false;

            return ((double)(entity as Enemy).HP / entity.ObjectDesc.MaxHP) < _threshold;
        }
    }
}
