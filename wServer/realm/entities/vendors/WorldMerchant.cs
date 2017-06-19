using System.Collections.Generic;
using common;

namespace wServer.realm.entities.vendors
{
    class WorldMerchant : Merchant
    {
        public List<ISellableItem> ItemList { get; set; }
        public ISellableItem ShopItem { get; set; }

        public WorldMerchant(RealmManager manager, ushort objType) 
            : base(manager, objType)
        {
        }

        public override void Tick(RealmTime time)
        {
            if (ShopItem == null && TimeLeft != 0 && Count != 0)
                return;

            base.Tick(time);
        }

        public override void Reload()
        {
            if (Reloading)
                return;
            Reloading = true;

            int i;
            if (ItemList == null || (i = ItemList.IndexOf(ShopItem)) == -1)
            {
                Owner.LeaveWorld(this);
                return;
            }

            if (ShopItem.Count == 0)
            {
                ItemList.Remove(ShopItem);
                if (ItemList.Count <= 0)
                {
                    Owner.LeaveWorld(this);
                    return;
                }
            }

            i++;
            if (ItemList.Count <= i)
                i = 0;

            var nextItem = ItemList[i];
            ShopItem = nextItem;
            Item = nextItem.ItemId;
            Price = nextItem.Price;
            Count = nextItem.Count;
            //TimeLeft = Rand.Next(30000, 60000);

            Reloading = false;
        }
    }
}
