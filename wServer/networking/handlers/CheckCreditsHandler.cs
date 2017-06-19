using wServer.networking.packets;
using wServer.networking.packets.incoming;

namespace wServer.networking.handlers
{
    class CheckCreditsHandler : PacketHandlerBase<CheckCredits>
    {
        public override PacketId ID => PacketId.CHECKCREDITS;

        protected override void HandlePacket(Client client, CheckCredits packet)
        {
            //client.Manager.Logic.AddPendingAction(t => Handle(client));
            Handle(client);
        }

        void Handle(Client client)
        {
            var player = client.Player;
            if (player == null || IsTest(client))
                return;

            client.Account.FlushAsync();
            client.Account.Reload();
            player.Credits = player.Client.Account.Credits;
        }
    }
}
