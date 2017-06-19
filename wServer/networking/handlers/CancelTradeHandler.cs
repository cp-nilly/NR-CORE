using wServer.networking.packets;
using wServer.networking.packets.incoming;

namespace wServer.networking.handlers
{
    class CancelTradeHandler : PacketHandlerBase<CancelTrade>
    {
        public override PacketId ID =>  PacketId.CANCELTRADE;

        protected override void HandlePacket(Client client, CancelTrade packet)
        {
            //client.Manager.Logic.AddPendingAction(t => Handle(client, packet));
            Handle(client, packet);
        }

        private void Handle(Client client, CancelTrade packet)
        {
            var player = client.Player;
            if (player == null || IsTest(client))
                return;

            player.CancelTrade();
        }
    }
}
