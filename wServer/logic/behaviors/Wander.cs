using common.resources;
using wServer.realm;
using Mono.Game;

namespace wServer.logic.behaviors
{
    class Wander : CycleBehavior
    {
        //State storage: direction & remain time
        class WanderStorage
        {
            public Vector2 Direction;
            public float RemainingDistance;
        }


        float speed;
        public Wander(double speed)
        {
            this.speed = (float)speed;
        }

        //static Cooldown period = new Cooldown(500, 200);
        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            WanderStorage storage;
            if (state == null) storage = new WanderStorage();
            else storage = (WanderStorage)state;

            Status = CycleStatus.NotStarted;

            if (host.HasConditionEffect(ConditionEffects.Paralyzed))
                return;

            Status = CycleStatus.InProgress;
            if (storage.RemainingDistance <= 0)
            {
                // old wander
                //storage.Direction = new Vector2(Random.Next(-1, 2), Random.Next(-1, 2));
                //storage.Direction.Normalize();
                //storage.RemainingDistance = period.Next(Random) / 1000f;
                //Status = CycleStatus.Completed;

                // creepylava's newer wander
                storage.Direction = new Vector2(Random.Next() % 2 == 0 ? -1 : 1, Random.Next() % 2 == 0 ? -1 : 1);
                storage.Direction.Normalize();
                storage.RemainingDistance = 600 / 1000f;
                Status = CycleStatus.Completed;
            }
            float dist = host.GetSpeed(speed) * (time.ElaspedMsDelta / 1000f);
            host.ValidateAndMove(host.X + storage.Direction.X * dist, host.Y + storage.Direction.Y * dist);

            storage.RemainingDistance -= dist;

            state = storage;
        }
    }
}
