using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using common.resources;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors
{
    class ConditionEffectRegion : Behavior
    {
        private readonly ConditionEffectIndex[] _effects;
        private readonly int _range;
        private readonly int _duration;

        public ConditionEffectRegion(ConditionEffectIndex[] effects, int range = 2, int duration = -1)
        {
            _range = range;
            _effects = effects;
            _duration = duration;
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            if (host.Owner == null) return;

            if (host.HasConditionEffect(ConditionEffects.Paused)) 
                return;

            var hx = (int) host.X;
            var hy = (int) host.Y;

            var players = host.Owner.Players.Values
                .Where(p => Math.Abs(hx - (int) p.X) < _range && Math.Abs(hy - (int) p.Y) < _range);

            foreach (var player in players)
            {
                foreach (var effect in _effects)
                {
                    player.ApplyConditionEffect(new ConditionEffect()
                    {
                        Effect = effect,
                        DurationMS = _duration
                    });
                }
            }
        }
    }
}
