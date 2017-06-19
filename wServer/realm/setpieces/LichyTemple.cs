using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class LichyTemple : ISetPiece
    {
        public int Size { get { return 26; } }

        static readonly string Floor = "Blue Floor";
        static readonly string WallA = "Blue Wall";
        static readonly string WallB = "Destructible Blue Wall";
        static readonly string PillarA = "Blue Pillar";
        static readonly string PillarB = "Broken Blue Pillar";

        Random rand = new Random();
        public void RenderSetPiece(World world, IntPoint pos)
        {
            int[,] t = new int[25, 26];

            for (int x = 2; x < 23; x++)    //Floor
                for (int y = 1; y < 24; y++)
                    t[x, y] = rand.Next() % 10 == 0 ? 0 : 1;

            for (int y = 1; y < 24; y++)    //Perimeters
                t[2, y] = t[22, y] = 2;
            for (int x = 2; x < 23; x++)
                t[x, 23] = 2;
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    t[x + 1, y] = t[x + 21, y] = 2;
            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 5; y++)
                {
                    if ((x == 0 && y == 0) ||
                        (x == 0 && y == 4) ||
                        (x == 4 && y == 0) ||
                        (x == 4 && y == 4)) continue;
                    t[x, y + 21] = t[x + 20, y + 21] = 2;
                }

            for (int y = 0; y < 6; y++)     //Pillars
                t[9, 4 + 3 * y] = t[15, 4 + 3 * y] = 4;

            for (int x = 0; x < 25; x++)    //Corruption
                for (int y = 0; y < 26; y++)
                {
                    if (t[x, y] == 1 || t[x, y] == 0) continue;
                    double p = rand.NextDouble();
                    if (p < 0.1)
                        t[x, y] = 1;
                    else if (p < 0.4)
                        t[x, y]++;
                }

            int r = rand.Next(0, 4);
            for (int i = 0; i < r; i++)     //Rotation
                t = SetPieces.rotateCW(t);
            int w = t.GetLength(0), h = t.GetLength(1);

            var dat = world.Manager.Resources.GameData;
            for (int x = 0; x < w; x++)    //Rendering
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
                        tile.ObjType = dat.IdToObjectType[PillarA];
                        tile.ObjDesc = dat.ObjectDescs[tile.ObjType];
                        if (tile.ObjId == 0) tile.ObjId = world.GetNextEntityId();
                        tile.UpdateCount++;
                    }
                    else if (t[x, y] == 5)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Floor];
                        tile.ObjType = dat.IdToObjectType[PillarB];
                        tile.ObjDesc = dat.ObjectDescs[tile.ObjType];
                        if (tile.ObjId == 0) tile.ObjId = world.GetNextEntityId();
                        tile.UpdateCount++;
                    }
                }

            //Boss
            Entity lich = Entity.Resolve(world.Manager, "Lich");
            lich.Move(pos.X + Size / 2, pos.Y + Size / 2);
            world.EnterWorld(lich);
        }
    }
}
