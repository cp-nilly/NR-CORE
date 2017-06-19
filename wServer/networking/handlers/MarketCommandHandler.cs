using wServer.networking.packets;
using System;
using System.Linq;
using System.Threading.Tasks;
using common;
using wServer.networking.packets.incoming;
using wServer.networking.packets.outgoing;
using MarketResult = wServer.networking.packets.outgoing.MarketResult;

namespace wServer.networking.handlers
{
    class MarketCommandHandler : PacketHandlerBase<MarketCommand>
    {
        public override PacketId ID => PacketId.MARKET_COMMAND;

        protected override async void HandlePacket(Client client, MarketCommand packet)
        {
            try
            {
                switch (packet.CommandId)
                {
                    case MarketCommand.REQUEST_MY_ITEMS:
                        MyItems(client);
                        break;
                    case MarketCommand.ADD_OFFER:
                        AddOffers(client, packet.NewOffers);
                        break;
                    case MarketCommand.REMOVE_OFFER:
                        await RemoveOffer(client, packet.OfferId);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void MyItems(Client client)
        {
            var items = client.Player.GetMarketItems();

            client.SendPacket(new MarketResult
            {
                CommandId = MarketResult.MARKET_REQUEST_RESULT,
                Items = items
            });
        }

        private void AddOffers(Client client, MarketOffer[] newOffers)
        {
            if (newOffers.Length > 20 || newOffers.Select(o => o.Slot.SlotId).Distinct().Count() < newOffers.Length)
            {
                Log.Info($"Market Error {client.Player.Name}: Invalid offer.");
                client.SendPacket(MarketResult.Error("Invalid offer."));
                return;
            }

            var result = client.Player.AddToMarket(newOffers);
            if (result != realm.entities.MarketResult.Success)
            {
                client.SendPacket(MarketResult.Error(result.GetDescription()));
                return;
            }

            client.SendPacket(MarketResult.Success($"Your item{((newOffers.Length > 1) ? "s have" : " has")} been placed on market."));
        }

        private async Task RemoveOffer(Client client, uint offerId)
        {
            var result = await client.Player.RemoveItemFromMarketAsync(offerId);
            if (result != realm.entities.MarketResult.Success)
            {
                client.SendPacket(MarketResult.Error(result.GetDescription()));
                return;
            }

            client.SendPacket(MarketResult.Success("Your item has been removed and placed in your gift chest."));
            client.SendPacket(new GlobalNotification
            {
                Text = "giftChestOccupied"
            });
        }
    }
}
