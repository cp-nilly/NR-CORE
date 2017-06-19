using System;
using wServer.realm;

namespace wServer.logic.transitions
{
    class GroupNotExistTransition : Transition
    {
        //State storage: none

        private readonly double _dist;
        private readonly string _group;

        public GroupNotExistTransition(double dist, string targetState, string group)
            : base(targetState)
        {
            _dist = dist;
            _group = group;
        }

        protected override bool TickCore(Entity host, RealmTime time, ref object state)
        {
            if (String.IsNullOrWhiteSpace(_group))
                return false;

            return host.GetNearestEntityByGroup(_dist, _group) == null;
        }
    }
}
