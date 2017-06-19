using System;
using System.Collections.Generic;
using System.ComponentModel;
using common;
using common.resources;
using wServer.networking.packets.outgoing;
using wServer.realm.worlds.logic;

namespace wServer.realm.entities.vendors
{
    public enum BuyResult
    {
        [Description("Purchase successful.")]
        Ok,
        [Description("Cannot purchase items with a guest account.")]
        IsGuest,
        [Description("Insufficient Rank.")]
        InsufficientRank,
        [Description("Insufficient Funds.")]
        InsufficientFunds,
        [Description("Can't buy items on a test map.")]
        IsTestMap,
        [Description("Uninitalized.")]
        Uninitialized,
        [Description("Transaction failed.")]
        TransactionFailed,
        [Description("Item is currently being purchased.")]
        BeingPurchased,
        [Description("Admins can't buy player merched items.")]
        Admin
    }

    public abstract class SellableObject : StaticObject
    {
        protected static Random Rand = new Random();

        private readonly SV<int> _price;
        private readonly SV<CurrencyType> _currency;
        private readonly SV<int> _rankReq;

        public int Price
        {
            get { return _price.GetValue(); }
            set { _price.SetValue(value);}
        }
        public CurrencyType Currency
        {
            get { return _currency.GetValue(); }
            set { _currency.SetValue(value); }
        }
        public int RankReq
        {
            get { return _rankReq.GetValue(); }
            set { _rankReq.SetValue(value); }
        }

        public int Tax { get; set; }

        protected SellableObject(RealmManager manager, ushort objType)
            : base(manager, objType, null, true, false, false)
        {
            _price = new SV<int>(this, StatsType.SellablePrice, 0);
            _currency = new SV<CurrencyType>(this, StatsType.SellablePriceCurrency, 0);
            _rankReq = new SV<int>(this, StatsType.SellableRankRequirement, 0);
        }

        public virtual void Buy(Player player)
        {
            SendFailed(player, BuyResult.Uninitialized);
        }

        protected override void ExportStats(IDictionary<StatsType, object> stats)
        {
            stats[StatsType.SellablePrice] = Price;
            stats[StatsType.SellablePriceCurrency] = (int)Currency;
            stats[StatsType.SellableRankRequirement] = RankReq;
            base.ExportStats(stats);
        }

        protected override void ImportStats(StatsType stats, object val)
        {
            switch (stats)
            {
                case StatsType.SellablePrice:
                    Price = (int) val; break;
                case StatsType.SellablePriceCurrency:
                    Currency = (CurrencyType) val; break;
                case StatsType.SellableRankRequirement:
                    RankReq = (int) val; break;
            }
            base.ImportStats(stats, val);
        }

        protected BuyResult ValidateCustomer(Player player)
        {
            if (Owner is Test)
                return BuyResult.IsTestMap;
            if (player.Stars < RankReq)
                return BuyResult.InsufficientRank;

            var acc = player.Client.Account;
            if (acc.Guest)
            {
                // reload guest prop just in case user registered in game
                acc.Reload("guest");
                if (acc.Guest)
                    return BuyResult.IsGuest;
            }

            if (player.GetCurrency(Currency) < Price)
                return BuyResult.InsufficientFunds;
            
            return BuyResult.Ok;
        }

        protected void SendFailed(Player player, BuyResult result)
        {
            player.Client.SendPacket(new networking.packets.outgoing.BuyResult
            {
                Result = 1,
                ResultString = $"Purchase Error: {result.GetDescription()}"
            });
        }
    }
}
