using System.Linq;
using common.resources;
using wServer.networking.packets.outgoing;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors
{
    class KillPlayer : Behavior
    {
        private Cooldown _coolDown;
        private readonly string _killMessage;
        private readonly bool _rekt;
        private readonly bool _killAll;

        public KillPlayer(string killMessage, Cooldown coolDown = new Cooldown(), bool rekt = true, bool killAll = false)
        {
            _coolDown = coolDown.Normalize();
            _killMessage = killMessage;
            _rekt = rekt;
            _killAll = killAll;
        }

        protected override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {
            state = _coolDown.Next(Random);
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            if (host.AttackTarget == null || host.AttackTarget.Owner == null)
                return;
                
            var cool = (int)state;

            if (cool <= 0)
            {
                // death strike
                if (_killAll)
                    foreach (var plr in host.Owner.Players.Values
                        .Where(p => !p.HasConditionEffect(ConditionEffects.Hidden)))
                    {
                        Kill(host, plr);
                    }
                else
                    Kill(host, host.AttackTarget);

                // send kill message
                if (_killMessage != null)
                {
                    var packet = new Text()
                    {
                        Name = "#" + (host.ObjectDesc.DisplayId ?? host.ObjectDesc.ObjectId),
                        ObjectId = host.Id,
                        NumStars = -1,
                        BubbleTime = 3,
                        Recipient = "",
                        Txt = _killMessage,
                        CleanText = ""
                    };
                    foreach (var i in host.Owner.PlayersCollision.HitTest(host.X, host.Y, 15).Where(e => e is Player))
                    {
                        if (i is Player && host.Dist(i) < 15)
                            (i as Player).Client.SendPacket(packet);
                    }
                }

                cool = _coolDown.Next(Random);
            }
            else
                cool -= time.ElaspedMsDelta;

            state = cool;
        }

        private void Kill(Entity host, Player player)
        {
            host.Owner.BroadcastPacketNearby(new ShowEffect()
            {
                EffectType = EffectType.Trail,
                TargetObjectId = host.Id,
                Pos1 = new Position { X = player.X, Y = player.Y },
                Color = new ARGB(0xffffffff)
            }, host, null, PacketPriority.Low);

            // kill player
            player.Death(host.ObjectDesc.DisplayId, rekt: _rekt);
        }
    }
}
