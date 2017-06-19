using System.Linq;
using common.resources;
using wServer.networking;
using wServer.realm.entities;
using wServer.realm.terrain;

namespace wServer.realm.worlds.logic
{
    class Nexus : World
    {
        public Nexus(ProtoWorld proto, Client client = null) : base(proto)
        {
        }

        protected override void Init()
        {
            base.Init();
            
            var monitor = Manager.Monitor;
            foreach (var i in Manager.Worlds.Values)
            {
                if (i is Realm)
                {
                    monitor.AddPortal(i.Id);
                    continue;
                }

                if (i.Id >= 0)
                    continue;

                if (i is DeathArena)
                {
                    var portal = new Portal(Manager, 0x023D, null)
                    {
                        Name = "Oryx's Arena",
                        WorldInstance = i
                    };

                    var pos = GetRegionPosition(TileRegion.Arena_Edge_Spawn);
                    if (pos == null)
                        continue;

                    monitor.AddPortal(i.Id, portal, pos);
                    continue;
                }

                if (i is Arena)
                {
                    var portal = new Portal(Manager, 0x7002, null)
                    {
                        Name = "Arena (0)",
                        WorldInstance = i
                    };

                    var pos = GetRegionPosition(TileRegion.Arena_Central_Spawn);
                    if (pos == null)
                        continue;

                    monitor.AddPortal(i.Id, portal, pos);
                    continue;
                }

                if (i is ArenaSolo)
                {
                    var portal = new Portal(Manager, 0x144, null)
                    {
                        Name = "Solo Arena",
                        WorldInstance = i
                    };

                    var pos = GetRegionPosition(TileRegion.Store_38);
                    if (pos == null)
                        continue;

                    monitor.AddPortal(i.Id, portal, pos);
                    continue;
                }

                if (i is PetYard)
                {
                    var portal = new Portal(Manager, 0x166, null)
                    {
                        Name = "Pet Yard",
                        WorldInstance = i
                    };

                    var pos = GetRegionPosition(TileRegion.Store_40);
                    if (pos == null)
                        continue;

                    monitor.AddPortal(i.Id, portal, pos);
                    continue;
                }

                if (i.Name.Equals("ClothBazaar"))
                {
                    var portal = new Portal(Manager, 0x167, null)
                    {
                        Name = "Cloth Bazaar (0)",
                        WorldInstance = i
                    };

                    var pos = GetRegionPosition(TileRegion.Store_39);
                    if (pos == null)
                        continue;

                    monitor.AddPortal(i.Id, portal, pos);
                    continue;
                }

                if (i is Marketplace && Manager.Config.serverSettings.enableMarket)
                {
                    var portal = new Portal(Manager, 0x190, null)
                    {
                        Name = "Marketplace (0)",
                        WorldInstance = i
                    };

                    var pos = GetRegionPosition(TileRegion.Store_37);
                    if (pos == null)
                        continue;

                    monitor.AddPortal(i.Id, portal, pos);
                    continue;
                }
            }
        }
    }
}
