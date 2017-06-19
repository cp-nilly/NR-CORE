using wServer.networking.packets;
using log4net;
using wServer.networking.packets.incoming;
using wServer.networking.packets.outgoing;

namespace wServer.networking.handlers
{
    class ChangeTradeHandler : PacketHandlerBase<ChangeTrade>
    {
        private static readonly ILog CheatLog = LogManager.GetLogger("CheatLog");

        public override PacketId ID => PacketId.CHANGETRADE;

        protected override void HandlePacket(Client client, ChangeTrade packet)
        {
            //client.Manager.Logic.AddPendingAction(t => Handle(client, packet));
            Handle(client, packet);
        }

        private void Handle(Client client, ChangeTrade packet)
        {
            var sb = false;
            var player = client.Player;
            if (player == null || IsTest(client))
                return;

            if (player.tradeTarget == null)
                return;

            for (int i = 0; i < packet.Offer.Length; i++)
            {
                if (packet.Offer[i])
                {
                    if (player.Inventory[i].Soulbound)
                    {
                        sb = true;
                        packet.Offer[i] = false;
                    }
                }
            }

            player.tradeAccepted = false;
            player.tradeTarget.tradeAccepted = false;
            player.trade = packet.Offer;

            player.tradeTarget.Client.SendPacket(new TradeChanged()
            {
                Offer = player.trade
            });

            if (sb)
            {
                CheatLog.InfoFormat("User {0} tried to trade a Soulbound item.", player.Name);
                player.SendError("You can't trade Soulbound items.");
            }
        }
    }
}
