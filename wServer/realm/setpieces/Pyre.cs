using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using common.resources;
using wServer.realm.entities;
using wServer.logic.loot;
using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class Pyre : ISetPiece
    {
        public int Size
        {
            get { return 30; }
        }

        static readonly string Floor = "Scorch Blend";

        static readonly Loot chest = new Loot(
                new TierLoot(5, ItemType.Weapon, 0.3),
                new TierLoot(6, ItemType.Weapon, 0.2),
                new TierLoot(7, ItemType.Weapon, 0.1),

                new TierLoot(4, ItemType.Armor, 0.3),
                new TierLoot(5, ItemType.Armor, 0.2),
                new TierLoot(6, ItemType.Armor, 0.1),

                new TierLoot(2, ItemType.Ability, 0.3),
                new TierLoot(3, ItemType.Ability, 0.2),

                new TierLoot(1, ItemType.Ring, 0.25),
                new TierLoot(2, ItemType.Ring, 0.15),

                new TierLoot(1, ItemType.Potion, 0.5)
            );

        Random rand = new Random();
        public void RenderSetPiece(World world, IntPoint pos)
        {
            var dat = world.Manager.Resources.GameData;
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                {
                    double dx = x - (Size / 2.0);
                    double dy = y - (Size / 2.0);
                    double r = Math.Sqrt(dx * dx + dy * dy) + rand.NextDouble() * 4 - 2;
                    if (r <= 10)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Floor];
                        tile.ObjType = 0;
                        tile.UpdateCount++;
                    }
                }

            Entity lord = Entity.Resolve(world.Manager, "Phoenix Lord");
            lord.Move(pos.X + 15.5f, pos.Y + 15.5f);
            world.EnterWorld(lord);

            Container container = new Container(world.Manager, 0x0501, null, false);
            Item[] items = chest.GetLoots(world.Manager, 5, 8).ToArray();
            for (int i = 0; i < items.Length; i++)
                container.Inventory[i] = items[i];
            container.Move(pos.X + 15.5f, pos.Y + 15.5f);
            world.EnterWorld(container);
        }
    }
}
