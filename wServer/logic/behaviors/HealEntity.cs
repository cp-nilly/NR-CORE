using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using common.resources;
using wServer.networking.packets.outgoing;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors
{
    class HealEntity : Behavior
    {
        //State storage: cooldown timer

        private readonly double _range;
        private readonly string _name;
        private Cooldown _coolDown;
        private readonly int? _amount;

        public HealEntity(double range, string name = null, int? healAmount = null, Cooldown coolDown = new Cooldown())
        {
            _range = (float)range;
            _name = name;
            _coolDown = coolDown.Normalize();
            _amount = healAmount;
        }

        protected override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {
            state = 0;
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            var cool = (int)state;

            if (cool <= 0)
            {
                if (host.HasConditionEffect(ConditionEffects.Stunned)) return;

                foreach (var entity in host.GetNearestEntitiesByName(_range, _name).OfType<Enemy>())
                {
                    int newHp = entity.ObjectDesc.MaxHP;
                    if (_amount != null)
                    {
                        var newHealth = (int)_amount + entity.HP;
                        if (newHp > newHealth)
                            newHp = newHealth;
                    }
                    if (newHp != entity.HP)
                    {
                        int n = newHp - entity.HP;
                        entity.HP = newHp;
                        entity.Owner.BroadcastPacketNearby(new ShowEffect()
                        {
                            EffectType = EffectType.Potion,
                            TargetObjectId = entity.Id,
                            Color = new ARGB(0xffffffff)
                        }, entity, null, PacketPriority.Low);
                        entity.Owner.BroadcastPacketNearby(new ShowEffect()
                        {
                            EffectType = EffectType.Trail,
                            TargetObjectId = host.Id,
                            Pos1 = new Position() { X = entity.X, Y = entity.Y },
                            Color = new ARGB(0xffffffff)
                        }, host, null, PacketPriority.Low);
                        entity.Owner.BroadcastPacketNearby(new Notification()
                        {
                            ObjectId = entity.Id,
                            Message = "+" + n,
                            Color = new ARGB(0xff00ff00)
                        }, entity, null, PacketPriority.Low);
                    }
                }
                cool = _coolDown.Next(Random);
            }
            else
                cool -= time.ElaspedMsDelta;

            state = cool;
        }
    }
}
