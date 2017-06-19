using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using common.resources;
using wServer.realm;
using Mono.Game;
using wServer.realm.entities;

namespace wServer.logic.behaviors
{
    class Charge : CycleBehavior
    {
        //State storage: charge state
        class ChargeState
        {
            public Vector2 Direction;
            public int RemainingTime;
        }

        private readonly float _speed;
        private readonly float _range;
        private Cooldown _coolDown;

        public Charge(double speed = 4, float range = 10, Cooldown coolDown = new Cooldown())
        {
            _speed = (float)speed;
            _range = range;
            _coolDown = coolDown.Normalize(2000);
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            var s = (state == null) ? 
                new ChargeState() : 
                (ChargeState) state;

            Status = CycleStatus.NotStarted;

            if (host.HasConditionEffect(ConditionEffects.Paralyzed)) 
                return;

            if (s.RemainingTime <= 0)
            {
                if (s.Direction == Vector2.Zero)
                {
                    var player = host.GetNearestEntity(_range, null);
                    if (player != null && player.X != host.X && player.Y != host.Y)
                    {
                        s.Direction = new Vector2(player.X - host.X, player.Y - host.Y);
                        var d = s.Direction.Length();
                        s.Direction.Normalize();
                        //s.RemainingTime = _coolDown.Next(Random);
                        //if (d / host.GetSpeed(_speed) < s.RemainingTime)
                            s.RemainingTime = (int)(d / host.GetSpeed(_speed) * 1000);
                        Status = CycleStatus.InProgress;
                    }
                }
                else
                {
                    s.Direction = Vector2.Zero;
                    s.RemainingTime = _coolDown.Next(Random);
                    Status = CycleStatus.Completed;
                }
            }

            if (s.Direction != Vector2.Zero)
            {
                float dist = host.GetSpeed(_speed) * (time.ElaspedMsDelta / 1000f);
                host.ValidateAndMove(host.X + s.Direction.X * dist, host.Y + s.Direction.Y * dist);
                Status = CycleStatus.InProgress;
            }

            s.RemainingTime -= time.ElaspedMsDelta;

            state = s;
        }
    }
}
