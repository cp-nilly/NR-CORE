using System;
using System.Linq;
using common.resources;
using wServer.networking.packets.outgoing;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors.PetBehaviors
{
    public class PetMagicHeal : PetBehavior
    {
        public PetMagicHeal() : base(PAbility.MagicHeal, true)
        {
        }

        protected override void TickCore(Pet host, RealmTime time, ref object state)
        {
            var cool = (int)state;
            if (cool <= 0)
            {
                var maxHp = PlayerOwner.Stats[1];
                var mp = LinearGrowthMagicHeal();
                cool = DiminishingReturnsCooldown(1, 10, Ability.Level, 0.5264);
                if (mp == -1) return;
                var newHp = Math.Min(maxHp, PlayerOwner.MP + mp);
                if (newHp != PlayerOwner.MP)
                {
                    if (PlayerOwner.HasConditionEffect(ConditionEffects.Quiet))
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
                    var n = newHp - PlayerOwner.MP;
                    PlayerOwner.MP = newHp;
                    PlayerOwner.Owner.BroadcastPacket(new ShowEffect
                    {
                        EffectType = EffectType.Potion,
                        TargetObjectId = PlayerOwner.Id,
                        Color = new ARGB(0x6084e0)
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
                        Color = new ARGB(0x6084e0)
                    }, null);
                }
            }
            else
                cool -= time.ElaspedMsDelta;

            state = cool;
        }

        private int LinearGrowthMagicHeal()
        {
            return (int)Math.Round(0.0000003111287813272 * Math.Pow(Ability.Level, 4)
                - 0.0000256213162692859 * Math.Pow(Ability.Level, 3)
                + 0.00444679769799348 * Math.Pow(Ability.Level, 2)
                - 0.0623195688424998 * Ability.Level + 1.14636993462723);
        }
    }
}