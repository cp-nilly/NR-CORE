using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using common;

namespace wServer.realm.entities
{
    public class Container : StaticObject, IContainer
    {
        private const int BagSize = 8;

        public Container(RealmManager manager, ushort objType, int? life, bool dying, RInventory dbLink = null)
            : base(manager, objType, life, false, dying, false)
        {
            Initialize(dbLink);
        }

        public Container(RealmManager manager, ushort id)
            : base(manager, id, null, false, false, false)
        {
            Initialize(null);
        }

        private void Initialize(RInventory dbLink)
        {
            Inventory = new Inventory(this);
            BagOwners = new int[0];
            DbLink = dbLink;

            var node = Manager.Resources.GameData.ObjectTypeToElement[ObjectType];
            SlotTypes = Utils.ResizeArray(node.Element("SlotTypes").Value.CommaToArray<int>(), BagSize);
            XElement eq = node.Element("Equipment");
            if (eq != null)
            {
                var inv = eq.Value.CommaToArray<ushort>().Select(_ => _ == 0xffff ? null : Manager.Resources.GameData.Items[_]).ToArray();
                Array.Resize(ref inv, BagSize);
                Inventory.SetItems(inv);
            }
        }

        public RInventory DbLink { get; private set; }
        public int[] SlotTypes { get; private set; }
        public Inventory Inventory { get; private set; }
        public int[] BagOwners { get; set; }

        protected override void ImportStats(StatsType stats, object val)
        {
            if (Inventory == null) return;

            switch (stats)
            {
                case StatsType.Inventory0: Inventory[0] = (int)val == -1 ? null : Manager.Resources.GameData.Items[(ushort)(int)val]; break;
                case StatsType.Inventory1: Inventory[1] = (int)val == -1 ? null : Manager.Resources.GameData.Items[(ushort)(int)val]; break;
                case StatsType.Inventory2: Inventory[2] = (int)val == -1 ? null : Manager.Resources.GameData.Items[(ushort)(int)val]; break;
                case StatsType.Inventory3: Inventory[3] = (int)val == -1 ? null : Manager.Resources.GameData.Items[(ushort)(int)val]; break;
                case StatsType.Inventory4: Inventory[4] = (int)val == -1 ? null : Manager.Resources.GameData.Items[(ushort)(int)val]; break;
                case StatsType.Inventory5: Inventory[5] = (int)val == -1 ? null : Manager.Resources.GameData.Items[(ushort)(int)val]; break;
                case StatsType.Inventory6: Inventory[6] = (int)val == -1 ? null : Manager.Resources.GameData.Items[(ushort)(int)val]; break;
                case StatsType.Inventory7: Inventory[7] = (int)val == -1 ? null : Manager.Resources.GameData.Items[(ushort)(int)val]; break;
                case StatsType.Inventory8: Inventory[8] = (int)val == -1 ? null : Manager.Resources.GameData.Items[(ushort)(int)val]; break;
                case StatsType.Inventory9: Inventory[9] = (int)val == -1 ? null : Manager.Resources.GameData.Items[(ushort)(int)val]; break;
                case StatsType.Inventory10: Inventory[10] = (int)val == -1 ? null : Manager.Resources.GameData.Items[(ushort)(int)val]; break;
                case StatsType.Inventory11: Inventory[11] = (int)val == -1 ? null : Manager.Resources.GameData.Items[(ushort)(int)val]; break;
                case StatsType.OwnerAccountId: break;// BagOwner = (int)val == -1 ? (int?)null : (int)val; break;
            }
            base.ImportStats(stats, val);
        }
        protected override void ExportStats(IDictionary<StatsType, object> stats)
        {
            if (Inventory == null) return;

            stats[StatsType.Inventory0] = Inventory[0]?.ObjectType ?? -1;
            stats[StatsType.Inventory1] = Inventory[1]?.ObjectType ?? -1;
            stats[StatsType.Inventory2] = Inventory[2]?.ObjectType ?? -1;
            stats[StatsType.Inventory3] = Inventory[3]?.ObjectType ?? -1;
            stats[StatsType.Inventory4] = Inventory[4]?.ObjectType ?? -1;
            stats[StatsType.Inventory5] = Inventory[5]?.ObjectType ?? -1;
            stats[StatsType.Inventory6] = Inventory[6]?.ObjectType ?? -1;
            stats[StatsType.Inventory7] = Inventory[7]?.ObjectType ?? -1;
            stats[StatsType.OwnerAccountId] = (BagOwners.Length == 1 ? BagOwners[0] : -1).ToString();
            base.ExportStats(stats);
        }

        public override void Tick(RealmTime time)
        {
            if (Inventory == null)
                return;

            if (ObjectType == 0x504)    //Vault chest
                return;

            bool hasItem = false;
            foreach (var i in Inventory)
                if (i != null)
                {
                    hasItem = true;
                    break;
                }

            if (!hasItem)
                Owner.LeaveWorld(this);

            base.Tick(time);
        }

        public override bool HitByProjectile(Projectile projectile, RealmTime time)
        {
            return false;
        }
    }
}
