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
    class Graveyard : ISetPiece
    {
        public int Size { get { return 34; } }

        static readonly string Floor = "Grass";
        static readonly string WallA = "Grey Wall";
        static readonly string WallB = "Destructible Grey Wall";
        static readonly string Cross = "Cross";

        static readonly Loot chest = new Loot(
                new TierLoot(4, ItemType.Weapon, 0.3),
                new TierLoot(5, ItemType.Weapon, 0.2),
                new TierLoot(6, ItemType.Weapon, 0.1),

                new TierLoot(3, ItemType.Armor, 0.3),
                new TierLoot(4, ItemType.Armor, 0.2),
                new TierLoot(5, ItemType.Armor, 0.1),

                new TierLoot(1, ItemType.Ability, 0.3),
                new TierLoot(2, ItemType.Ability, 0.2),
                new TierLoot(3, ItemType.Ability, 0.2),

                new TierLoot(1, ItemType.Ring, 0.25),
                new TierLoot(2, ItemType.Ring, 0.15),

                new TierLoot(1, ItemType.Potion, 0.5)
            );

        Random rand = new Random();
        public void RenderSetPiece(World world, IntPoint pos)
        {
            int[,] t = new int[23, 35];

            for (int x = 0; x < 23; x++)    //Floor
                for (int y = 0; y < 35; y++)
                    t[x, y] = rand.Next() % 3 == 0 ? 0 : 1;

            for (int y = 0; y < 35; y++)    //Perimeters
                t[0, y] = t[22, y] = 2;
            for (int x = 0; x < 23; x++)
                t[x, 0] = t[x, 34] = 2;

            List<IntPoint> pts = new List<IntPoint>();
            for (int y = 0; y < 11; y++)    //Crosses
                for (int x = 0; x < 7; x++)
                {
                    if (rand.Next() % 3 > 0)
                        t[2 + 3 * x, 2 + 3 * y] = 4;
                    else
                        pts.Add(new IntPoint(2 + 3 * x, 2 + 3 * y));
                }

            for (int x = 0; x < 23; x++)    //Corruption
                for (int y = 0; y < 35; y++)
                {
                    if (t[x, y] == 1 || t[x, y] == 0 || t[x, y] == 4) continue;
                    double p = rand.NextDouble();
                    if (p < 0.1)
                        t[x, y] = 1;
                    else if (p < 0.4)
                        t[x, y]++;
                }


            //Boss & Chest
            var pt = pts[rand.Next(0, pts.Count)];
            t[pt.X, pt.Y] = 5;
            t[pt.X+1, pt.Y] = 6;

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
                        tile.TileId = dat.IdToTileType[Floor];
                        tile.ObjType = 0;
                        tile.UpdateCount++;
                    }
                    else if (t[x, y] == 2)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Floor];
                        tile.ObjType = dat.IdToObjectType[WallA];
                        tile.ObjDesc = dat.ObjectDescs[tile.ObjType];
                        if (tile.ObjId == 0) tile.ObjId = world.GetNextEntityId();
                        tile.UpdateCount++;
                    }
                    else if (t[x, y] == 3)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Floor];
                        tile.UpdateCount++;
                        Entity wall = Entity.Resolve(world.Manager, dat.IdToObjectType[WallB]);
                        wall.Move(x + pos.X + 0.5f, y + pos.Y + 0.5f);
                        world.EnterWorld(wall);
                    }
                    else if (t[x, y] == 4)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Floor];
                        tile.ObjType = dat.IdToObjectType[Cross];
                        tile.ObjDesc = dat.ObjectDescs[tile.ObjType];
                        if (tile.ObjId == 0) tile.ObjId = world.GetNextEntityId();
                        tile.UpdateCount++;
                    }
                    else if (t[x, y] == 5)
                    {
                        Container container = new Container(world.Manager, 0x0501, null, false);
                        Item[] items = chest.GetLoots(world.Manager, 3, 8).ToArray();
                        for (int i = 0; i < items.Length; i++)
                            container.Inventory[i] = items[i];
                        container.Move(pos.X + x + 0.5f, pos.Y + y + 0.5f);
                        world.EnterWorld(container);
                    }
                    else if (t[x, y] == 6)
                    {
                        Entity mage = Entity.Resolve(world.Manager, "Deathmage");
                        mage.Move(pos.X + x, pos.Y + y);
                        world.EnterWorld(mage);
                    }
                }
        }
    }
}
