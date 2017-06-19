using wServer.networking.packets;
using wServer.networking.packets.incoming;

namespace wServer.networking.handlers
{
    class RequestTradeHandler : PacketHandlerBase<RequestTrade>
    {
        public override PacketId ID => PacketId.REQUESTTRADE;

        protected override void HandlePacket(Client client, RequestTrade packet)
        {
            //client.Manager.Logic.AddPendingAction(t => Handle(client, packet));
            Handle(client, packet);
        }

        private void Handle(Client client, RequestTrade packet)
        {
            if (client.Player == null || IsTest(client))
                return;

            client.Player.RequestTrade(packet.Name);
        }
    }
}
