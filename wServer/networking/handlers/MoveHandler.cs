using wServer.realm.entities;
using wServer.networking.packets;
using wServer.networking.packets.incoming;
using wServer.realm;

namespace wServer.networking.handlers
{
    class MoveHandler : PacketHandlerBase<Move>
    {
        public override PacketId ID => PacketId.MOVE;

        protected override void HandlePacket(Client client, Move packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, t, packet));
        }

        void Handle(Player player, RealmTime time, Move packet)
        {
            if (player?.Owner == null)
                return;

            player.MoveReceived(time, packet);

            var newX = packet.NewPosition.X;
            var newY = packet.NewPosition.Y;
            if (player.SpectateTarget == null && player.Id == packet.ObjectId ||
                player.SpectateTarget?.Id == packet.ObjectId)
            {
                if (newX != -1 && newX != player.X ||
                    newY != -1 && newY != player.Y)
                {
                    player.Move(newX, newY);
                }
            }
        }
    }
}
