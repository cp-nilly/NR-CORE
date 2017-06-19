using System;
using System.Linq;
using common.resources;
using wServer.networking.packets.outgoing;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors.PetBehaviors
{
    public class PetHeal : PetBehavior
    {
        public PetHeal() : base(PAbility.Heal, true)
        {
        }

        protected override void TickCore(Pet host, RealmTime time, ref object state)
        {
            var cool = (int)state;
            if (cool <= 0)
            {
                var maxHp = PlayerOwner.Stats[0];
                var heal = LinearGrowthHp();
                cool = DiminishingReturnsCooldown(1, 10, Ability.Level, 0.5264);
                if (heal == -1) return;
                var newHp = Math.Min(maxHp, PlayerOwner.HP + heal);
                if (newHp != PlayerOwner.HP)
                {
                    if (PlayerOwner.HasConditionEffect(ConditionEffects.Sick))
                    {
                        PlayerOwner.Owner.BroadcastPacket(new ShowEffect
                        {
                            EffectType = EffectType.Trail,
                            TargetObjectId = host.Id,
                            Pos1 = new Position { X = PlayerOwner.X, Y = PlayerOwner.Y },
                            Color = new ARGB(0xffffffff)
                        }, null);
                        PlayerOwner.Owner.BroadcastPacket(new Notification
                        {
                            ObjectId = PlayerOwner.Id,
                            Message = "{\"key\":\"blank\",\"tokens\":{\"data\":\"No Effect\"}}",
                            Color = new ARGB(0xFF0000)
                        }, null);
                        state = cool;
                        return;
                    }
                    var n = newHp - PlayerOwner.HP;
                    PlayerOwner.HP = newHp;
                    PlayerOwner.Owner.BroadcastPacket(new ShowEffect
                    {
                        EffectType = EffectType.Potion,
                        TargetObjectId = PlayerOwner.Id,
                        Color = new ARGB(0xffffffff)
                    }, null);
                    PlayerOwner.Owner.BroadcastPacket(new ShowEffect
                    {
                        EffectType = EffectType.Trail,
                        TargetObjectId = host.Id,
                        Pos1 = new Position { X = PlayerOwner.X, Y = PlayerOwner.Y },
                        Color = new ARGB(0xffffffff)
                    }, null);
                    PlayerOwner.Owner.BroadcastPacket(new Notification
                    {
                        ObjectId = PlayerOwner.Id,
                        Message = "{\"key\":\"blank\",\"tokens\":{\"data\":\"+" + n + "\"}}",
                        Color = new ARGB(0xff00ff00)
                    }, null);
                }
            }
            else
                cool -= time.ElaspedMsDelta;

            state = cool;
        }

        private int LinearGrowthHp()
        {
            return (int)Math.Round(5.83283522976111E-07 * Math.Pow(Ability.Level, 4)
                - 0.0000469692310249639 * Math.Pow(Ability.Level, 3)
                + 0.0076256636031656 * Math.Pow(Ability.Level, 2)
                - 0.0776182463432286 * Ability.Level + 10.0998122309192);
        }
    }
}