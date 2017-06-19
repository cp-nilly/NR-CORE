using common;

namespace wServer.realm.entities
{
    class GiftChest : OneWayContainer
    {
        public GiftChest(RealmManager manager, ushort objType, int? life, bool dying, RInventory dbLink = null) 
            : base(manager, objType, life, dying, dbLink)
        {
        }

        public GiftChest(RealmManager manager, ushort id) 
            : base(manager, id)
        {
        }
    }
}
