using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using common.resources;
using wServer.logic.loot;
using wServer.realm.entities;
using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class ArchMage : ISetPiece
    {
        public int Size { get { return 11; } }


        static readonly string Lava = "Lava Blend";
        static readonly string Floor = "Firey Floor";

        static readonly Loot chest = new Loot(
                new TierLoot(9, ItemType.Weapon, 0.3),
                new TierLoot(10, ItemType.Weapon, 0.2),
                new TierLoot(11, ItemType.Weapon, 0.1),

                new TierLoot(10, ItemType.Armor, 0.3),
                new TierLoot(11, ItemType.Armor, 0.2),
                new TierLoot(12, ItemType.Armor, 0.1),

                new TierLoot(3, ItemType.Ability, 0.3),
                new TierLoot(4, ItemType.Ability, 0.2),
                new TierLoot(5, ItemType.Ability, 0.1),

                new TierLoot(3, ItemType.Ring, 0.25),
                new TierLoot(4, ItemType.Ring, 0.15),
                
                new TierLoot(2, ItemType.Potion, 0.5)
            );

        Random rand = new Random();
        public void RenderSetPiece(World world, IntPoint pos)
        {
            int[,] t = new int[11, 11];

            for (int x = 0; x < 11; x++)    //Moats
                for (int y = 0; y < 11; y++)
                    if (x == 0 || x == 10 || y == 0 || y == 10)
                    {
                        t[x, y] = t[x, y] = 1;
                        t[x, y] = t[x, y] = 1;
                    }

            for (int x = 1; x < 10; x++)    //Floor
                for (int y = 1; y < 10; y++)
                    t[x, y] = 2;

            //Boss & Chest
            t[5, 5] = 7;
            t[5, 6] = 8;

            int r = rand.Next(0, 4);
            for (int i = 0; i < r; i++)     //Rotation
                t = SetPieces.rotateCW(t);
            int w = t.GetLength(0), h = t.GetLength(1);

            var dat = world.Manager.Resources.GameData;
            for (int x = 0; x < w; x++)     //Rendering
                for (int y = 0; y < h; y++)
                {
                    if (t[x, y] == 1)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Lava];
                        tile.ObjType = 0;
                        tile.UpdateCount++;
                    }

                    else if (t[x, y] == 2)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Floor];
                        tile.ObjType = 0;
                        tile.UpdateCount++;
                    }

                    else if (t[x, y] == 7)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Floor];
                        tile.ObjType = 0;
                        tile.UpdateCount++;

                        Container container = new Container(world.Manager, 0x0501, null, false);
                        Item[] items = chest.GetLoots(world.Manager, 5, 8).ToArray();
                        for (int i = 0; i < items.Length; i++)
                            container.Inventory[i] = items[i];
                        container.Move(pos.X + x + 0.5f, pos.Y + y + 0.5f);
                        world.EnterWorld(container);
                    }
                    else if (t[x, y] == 8)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Floor];
                        tile.ObjType = 0;
                        tile.UpdateCount++;
                        Entity mage = Entity.Resolve(world.Manager, "Forgotten Archmage of Flame");
                        mage.Move(pos.X + x, pos.Y + y);
                        world.EnterWorld(mage);
                    }
                }
        }
    }
}
