using wServer.realm;
using wServer.realm.entities;
using wServer.networking.packets;
using wServer.networking.packets.incoming;

namespace wServer.networking.handlers
{
    class GroundDamageHandler : PacketHandlerBase<GroundDamage>
    {
        public override PacketId ID => PacketId.GROUNDDAMAGE;

        protected override void HandlePacket(Client client, GroundDamage packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, t, packet.Position, packet.Time));
        }

        void Handle(Player player, RealmTime time, Position pos, int timeHit)
        {
            if (player?.Owner == null)
                return;

            player.ForceGroundHit(time, pos, timeHit);
        }
    }
}
