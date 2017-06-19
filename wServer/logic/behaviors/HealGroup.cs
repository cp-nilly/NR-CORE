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
    class HealGroup : Behavior
    {
        //State storage: cooldown timer

        double range;
        string group;
        Cooldown coolDown;
        int? amount;

        public HealGroup(double range, string group, Cooldown coolDown = new Cooldown(), int? healAmount = null)
        {
            this.range = (float)range;
            this.group = group;
            this.coolDown = coolDown.Normalize();
            this.amount = healAmount;
        }

        protected override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {
            state = 0;
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            int cool = (int)state;

            if (cool <= 0)
            {
                if (host.HasConditionEffect(ConditionEffects.Stunned)) return;

                foreach (var entity in host.GetNearestEntitiesByGroup(range, group).OfType<Enemy>())
                {
                    int newHp = entity.ObjectDesc.MaxHP;
                    if (amount != null)
                    {
                        var newHealth = (int) amount + entity.HP;
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
                cool = coolDown.Next(Random);
            }
            else
                cool -= time.ElaspedMsDelta;

            state = cool;
        }
    }
}
