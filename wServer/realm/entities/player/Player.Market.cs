using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using common;
using common.resources;
using wServer.realm.worlds.logic;

namespace wServer.realm.entities
{
    public enum MarketResult
    {
        [Description("Success!")]
        Success,
        [Description("Can't market other players items.")]
        InvalidPlayerId,
        [Description("Can only market items in Marketplace.")]
        NotMarketPlace,
        [Description("Marketplace Disabled.")]
        MarketplaceDisabled,
        [Description("Can't market items while trading.")]
        CurrentlyTrading,
        [Description("Your asking price should be greater than or equal to 0.")]
        PriceLessThanZero,
        [Description("Invalid slot.")]
        InvalidSlot,
        [Description("Inventory transfer failure.")]
        InventoryTransferFailure,
        [Description("Failed to add item to Market. Please try again later.")]
        AddMarketFailure,
        [Description("Can't market soulbound items.")]
        IsSoulbound,
        [Description("Can't market nothing...")]
        NullItem,
        [Description("Not enough fame. There is a 5 fame fee to remove that item.")]
        InsufficientFunds,
        [Description("Market item does not exist. Perhaps someone bought it already or you removed it already?")]
        InvalidShopItemId,
        [Description("Failed to remove item from Market. Please try again later.")]
        RemoveMarketFailure,
    }

    public partial class Player
    {
        public PlayerShopItem[] GetMarketItems()
        {
            var items = Client.Manager.Market.GetItems(this);

            for (var i = items.Length - 1; i >= 0; i--)
                if (items[i].IsLastMarketedItem(Client.Account.LastMarketId))
                    break;

            return items;
        }

        public MarketResult AddToMarket(MarketOffer[] offers)
        {
            var result = MarketResult.Success;
            offers.Any(offer => (result = AddToMarket(offer)) != MarketResult.Success);
            return result;
        }

        public MarketResult AddToMarket(MarketOffer offer)
        {
            if (Id != offer.Slot.ObjectId)
                return LogError(MarketResult.InvalidPlayerId);
            
            return AddToMarket(offer.Slot.SlotId, offer.Price);
        }

        public MarketResult AddToMarket(int slot, int price)
        {
            var acc = Client.Account;

            if (!(Owner is Marketplace))
                return LogError(MarketResult.NotMarketPlace);
            if (acc.Admin && acc.Rank < 100 || !Manager.Config.serverSettings.enableMarket)
                return LogError(MarketResult.MarketplaceDisabled);
            if (tradeTarget != null)
                return LogError(MarketResult.CurrentlyTrading);
            if (slot >= Inventory.Length)
                return LogError(MarketResult.InvalidSlot);
            if (price < 0)
                return LogError(MarketResult.PriceLessThanZero);

            var it = Inventory.CreateTransaction();
            var item = it[slot];
            it[slot] = null;

            if (item == null)
                return LogError(MarketResult.NullItem);
            if (item.Soulbound)
                return LogError(MarketResult.IsSoulbound);
            
            // create shopItem
            var shopItem = Manager.Database.CreatePlayerShopItemAsync(
                item.ObjectType, price, DateTime.UtcNow.ToUnixTimestamp(), AccountId).Result;
            
            var tran = Manager.Database.Conn.CreateTransaction();
            Manager.Market.Add(shopItem, tran);
            if (!tran.Execute())
                return LogError(MarketResult.AddMarketFailure);

            if (!Inventory.Execute(it))
            {
                Manager.Market.Remove(shopItem);
                return LogError(MarketResult.InventoryTransferFailure);
            }

            Client.Account.LastMarketId = shopItem.Id;

            Log.Info($"{Name} added a {item.DisplayName} to the market for {price} fame");
            return MarketResult.Success;
        }

        public async Task<MarketResult> RemoveItemFromMarketAsync(uint shopItemId)
        {
            var acc = Client.Account;
            var market = Client.Manager.Market;

            if (acc.Admin && acc.Rank < 100 || !Manager.Config.serverSettings.enableMarket)
                return LogError(MarketResult.MarketplaceDisabled);
            if (acc.LastMarketId != shopItemId && acc.Fame < 5)
                return LogError(MarketResult.InsufficientFunds);

            var shopItem = market.GetShopItem(shopItemId);
            if (shopItem == null || shopItem.AccountId != AccountId)
                return LogError(MarketResult.InvalidShopItemId);

            var db = Manager.Database;
            var trans = db.Conn.CreateTransaction();

            Task t1 = Task.FromResult(0);
            if (acc.LastMarketId != shopItemId)
                t1 = db.UpdateCurrency(acc, -5, CurrencyType.Fame, trans);
            market.Remove(shopItem, trans);
            db.AddGift(acc, shopItem.ItemId, trans);
            var t2 = trans.ExecuteAsync();
            await Task.WhenAll(t1, t2);
            
            var success = !t2.IsCanceled && t2.Result;
            if (!success)
                return LogError(MarketResult.RemoveMarketFailure);

            if (acc.Fame < CurrentFame)
                CurrentFame = acc.Fame;

            Log.Info($"{Name} removed {Manager.Resources.GameData.Items[shopItem.ItemId].DisplayName} ({shopItem.Price}) from market.");
            return MarketResult.Success;
        }

        private MarketResult LogError(MarketResult result)
        {
            Log.Info($"Market Error {Name}: {result.GetDescription()}");
            return result;
        }
    }
}
