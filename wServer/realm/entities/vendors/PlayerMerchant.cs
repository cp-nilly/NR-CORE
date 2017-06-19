using System.Linq;
using System.Threading.Tasks;
using common;
using wServer.networking;
using System;
using wServer.networking.packets.outgoing;

namespace wServer.realm.entities.vendors
{
    public class PlayerMerchant : Merchant
    {
        public PlayerShopItem PlayerShopItem { get; set; }
        
        public PlayerMerchant(RealmManager manager, ushort objType)
            : base(manager, objType)
        {
            RankReq = 2;
        }

        public override void Reload()
        {
            if (Reloading)
                return;
            Reloading = true;
            Manager.Market.Reload(this);
            Reloading = false;
        }

        public override void Buy(Player player)
        {
            if (player.Client.Account.Admin && player.Client.Account.Rank < 100)
            {
                SendFailed(player, BuyResult.Admin);
                return;
            }

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
            // acquire price, id and seller here so that the wrong price is not sent to seller after update
            var sellerId = PlayerShopItem.AccountId;
            var price = PlayerShopItem.Price;
            var type = PlayerShopItem.ItemId;
            var trans = db.Conn.CreateTransaction();
            var t1 = db.UpdateCurrency(player.Client.Account, -Price, Currency, trans);
            db.AddToTreasury(Tax, trans);
            var invTrans = TransactionItem(player, trans);
            Manager.Market.Remove(PlayerShopItem, trans);
            var seller = Manager.Clients.FirstOrDefault(
                c => c.Key.Account?.AccountId == sellerId).Key;
            Task t2 = Task.FromResult(0);
            if (seller?.Account != null)
                t2 = Manager.Database.UpdateCurrency(seller.Account, Price - Tax, Currency, trans);
            else
                Manager.Database.UpdateCurrency(sellerId, Price - Tax, Currency, trans);
            var t3 = trans.ExecuteAsync();
            await Task.WhenAll(t1, t2, t3);

            var success = !t3.IsCanceled && t3.Result;
            TransactionItemComplete(player, invTrans, success);
            if (success)
            {
                if (seller?.Player != null && seller.Account != null)
                    seller.Player.CurrentFame = seller.Account.Fame;

                var itemDesc = Manager.Resources.GameData.Items[type];
                Manager.Chat.SendInfo(sellerId, $"Your {itemDesc.DisplayName} has sold for {price} fame.");
                Reload();
                BeingPurchased = false;
                AwaitingReload = false;
                return;
            }
            BeingPurchased = false;
        }


        protected override void SendNotifications(Player player, bool gift)
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

            Log.InfoFormat("[{0}]User {1} has bought {2} for {3} {4} from {5}.",
                DateTime.Now, 
                player.Name, 
                Manager.Resources.GameData.Items[Item].DisplayName, 
                Price, 
                Currency.ToString(), 
                Manager.Database.ResolveIgn(PlayerShopItem.AccountId));
        }
    }
}
