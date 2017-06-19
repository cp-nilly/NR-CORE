using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using wServer.networking.packets.outgoing;

namespace wServer.realm.entities.vendors
{
    public abstract class Merchant : SellableObject
    {
        private readonly SV<ushort> _item;
        private readonly SV<int> _count;
        private readonly SV<int> _timeLeft;

        public ushort Item
        {
            get { return _item.GetValue(); }
            set { _item.SetValue(value); }
        }
        public int Count
        {
            get { return _count.GetValue(); }
            set { _count.SetValue(value); }
        }
        public int TimeLeft {
            get { return _timeLeft.GetValue(); }
            set { _timeLeft.SetValue(value); }
        }

        public int ReloadOffset { get; set; }
        public bool Rotate { get; set; }

        protected volatile bool BeingPurchased;
        protected volatile bool AwaitingReload;
        protected volatile bool Reloading;

        protected Merchant(RealmManager manager, ushort objType)
            : base(manager, objType)
        {
            _item = new SV<ushort>(this, StatsType.MerchantMerchandiseType, 0xa00);
            _count = new SV<int>(this, StatsType.MerchantRemainingCount, -1);
            _timeLeft = new SV<int>(this, StatsType.MerchantRemainingMinute, -1);
            Rotate = true;
        }

        protected override void ExportStats(IDictionary<StatsType, object> stats)
        {
            stats[StatsType.MerchantMerchandiseType] = (int)Item;
            stats[StatsType.MerchantRemainingCount] = Count;
            stats[StatsType.MerchantRemainingMinute] = -1; //(int)(TimeLeft / 60000f);
            base.ExportStats(stats);
        }

        protected override void ImportStats(StatsType stats, object val)
        {
            switch (stats)
            {
                case StatsType.MerchantMerchandiseType:
                    Item = (ushort) val; break;
                case StatsType.MerchantRemainingCount:
                    Count = (int) val; break;
                case StatsType.MerchantRemainingMinute:
                    TimeLeft = (int) val; break;
            }
            base.ImportStats(stats, val);
        }

        /*public override void Tick(RealmTime time)
        {
            base.Tick(time);

            if (TimeLeft == -1)
                return;
            
            TimeLeft = Math.Max(0, TimeLeft - time.ElaspedMsDelta);

            if (this.AnyPlayerNearby(2))
                return;

            if (AwaitingReload || TimeLeft <= 0)
            {
                if (BeingPurchased)
                {
                    AwaitingReload = true;
                    return;
                }
                BeingPurchased = true;

                Reload();
                BeingPurchased = false;
                AwaitingReload = false;
            }
        }*/

        public override void Tick(RealmTime time)
        {
            base.Tick(time);

            var a = time.TotalElapsedMs % 20000;
            if (AwaitingReload ||
                a - time.ElaspedMsDelta <= ReloadOffset && a > ReloadOffset)
            {
                if (!AwaitingReload && !Rotate)
                    return;

                if (this.AnyPlayerNearby(2))
                {
                    AwaitingReload = true;
                    return;
                }

                if (BeingPurchased)
                {
                    AwaitingReload = true;
                    return;
                }
                BeingPurchased = true;

                TimeLeft = -1; // needed for player merchant to function properly with new rotation method
                Reload();
                BeingPurchased = false;
                AwaitingReload = false;
            }
        }

        public virtual void Reload() { }

        public override void Buy(Player player)
        {
            if (BeingPurchased)
            {
                SendFailed(player, BuyResult.BeingPurchased);
                return;
            }
            BeingPurchased = true;
            
            var result = ValidateCustomer(player);
            if (result != BuyResult.Ok)
            {
                SendFailed(player, result);
                BeingPurchased = false;
                return;
            }
            
            PurchaseItem(player);
        }

        private async void PurchaseItem(Player player)
        {
            var db = Manager.Database;
            var trans = db.Conn.CreateTransaction();
            var t1 = db.UpdateCurrency(player.Client.Account, -Price, Currency, trans);
            db.AddToTreasury(Tax, trans);
            var invTrans = TransactionItem(player, trans);
            var t2 = trans.ExecuteAsync();
            await Task.WhenAll(t1, t2);

            var success = !t2.IsCanceled && t2.Result;
            TransactionItemComplete(player, invTrans, success);
            if (success && Count != -1 && --Count <= 0)
            {
                Reload();
                AwaitingReload = false;
            }
            BeingPurchased = false;
        }

        protected InventoryTransaction TransactionItem(Player player, ITransaction tran)
        {
            var invTrans = player.Inventory.CreateTransaction();
            var item = Manager.Resources.GameData.Items[Item];
            var slot = invTrans.GetAvailableInventorySlot(item);
            if (slot == -1)
            {
                player.Manager.Database.AddGift(player.Client.Account, Item, tran);
                return null;
            }

            invTrans[slot] = item;
            return invTrans;
        }

        protected void TransactionItemComplete(Player player, InventoryTransaction invTrans, bool success)
        {
            if (!success)
            {
                SendFailed(player, BuyResult.TransactionFailed);
                return;
            }

            // update player currency values
            var acc = player.Client.Account;
            player.Credits = acc.Credits;
            player.CurrentFame = acc.Fame;
            player.Tokens = acc.Tokens;

            if (invTrans != null)
                Inventory.Execute(invTrans);
            SendNotifications(player, invTrans == null);
        }

        protected virtual void SendNotifications(Player player, bool gift)
        {
            if (gift)
                player.Client.SendPacket(new GlobalNotification
                {
                    Text = "giftChestOccupied"
                });

            player.Client.SendPacket(new networking.packets.outgoing.BuyResult
            {
                Result = 0,
                ResultString = "{\"key\":\"PackagePurchased.message\"}"
            });

            Log.InfoFormat("[{0}]User {1} has bought {2} for {3} {4}.",
                DateTime.Now, player.Name, Manager.Resources.GameData.Items[Item].DisplayName, Price, Currency.ToString());
        }
    }
}