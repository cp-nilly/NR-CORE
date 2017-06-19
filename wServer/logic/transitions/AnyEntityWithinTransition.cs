using wServer.realm;

namespace wServer.logic.transitions
{
    class AnyEntityWithinTransition : Transition
    {
        //State storage: none

        private readonly int _dist;

        public AnyEntityWithinTransition(int dist, string targetState)
            : base(targetState)
        {
            _dist = dist;
        }

        protected override bool TickCore(Entity host, RealmTime time, ref object state)
        {
            return host.AnyEnemyNearby(_dist) || host.AnyPlayerNearby(_dist);
        }
    }
}
