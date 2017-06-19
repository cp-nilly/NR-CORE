using System;
using wServer.realm;
using common.resources;
using Mono.Game;

namespace wServer.logic.behaviors
{
    class MoveTo : CycleBehavior
    {
        private readonly float _speed;
        private readonly float _x;
        private readonly float _y;

        public MoveTo(float speed, float x, float y)
        {
            _speed = speed;
            _x = x;
            _y = y;
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            Status = CycleStatus.NotStarted;
            if (host.HasConditionEffect(ConditionEffects.Paralyzed))
                return;

            Status = CycleStatus.InProgress;
            var path = new Vector2(_x - host.X, _y - host.Y);
            var dist = host.GetSpeed(_speed) * time.ElaspedMsDelta / 1000f;
            if (path.Length() <= dist)
            {
                Status = CycleStatus.Completed;
                host.ValidateAndMove(_x, _y);
            }
            else
            {
                path.Normalize();
                host.ValidateAndMove(host.X + path.X * dist, host.Y + path.Y * dist);
            }
        }
    }
}