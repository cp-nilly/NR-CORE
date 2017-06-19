using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using terrain;
using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class SkullShrine : ISetPiece
    {
        public int Size { get { return 33; } }

        static readonly string Grass = "Blue Grass";
        static readonly string Tile = "Castle Stone Floor Tile";
        static readonly string TileDark = "Castle Stone Floor Tile Dark";
        static readonly string Stone = "Cracked Purple Stone";
        static readonly string PillarA = "Blue Pillar";
        static readonly string PillarB = "Broken Blue Pillar";

        Random rand = new Random();
        public void RenderSetPiece(World world, IntPoint pos)
        {
            int[,] t = new int[33, 33];

            for (int x = 0; x < 33; x++)                    //Grassing
                for (int y = 0; y < 33; y++)
                {
                    if (Math.Abs(x - Size / 2) / (Size / 2.0) + rand.NextDouble() * 0.3 < 0.95 &&
                        Math.Abs(y - Size / 2) / (Size / 2.0) + rand.NextDouble() * 0.3 < 0.95)
                        t[x, y] = 1;
                }

            for (int x = 12; x < 21; x++)                   //Outer
                for (int y = 4; y < 29; y++)
                    t[x, y] = 2;
            t = SetPieces.rotateCW(t);
            for (int x = 12; x < 21; x++)
                for (int y = 4; y < 29; y++)
                    t[x, y] = 2;

            for (int x = 13; x < 20; x++)                   //Inner
                for (int y = 5; y < 28; y++)
                    t[x, y] = 4;
            t = SetPieces.rotateCW(t);
            for (int x = 13; x < 20; x++)
                for (int y = 5; y < 28; y++)
                    t[x, y] = 4;

            for (int i = 0; i < 4; i++)                     //Ext
            {
                for (int x = 13; x < 20; x++)
                    for (int y = 5; y < 7; y++)
                        t[x, y] = 3;
                t = SetPieces.rotateCW(t);
            }

            for (int i = 0; i < 4; i++)                     //Pillars
            {
                t[13, 7] = rand.Next() % 3 == 0 ? 6 : 5;
                t[19, 7] = rand.Next() % 3 == 0 ? 6 : 5;
                t[13, 10] = rand.Next() % 3 == 0 ? 6 : 5;
                t[19, 10] = rand.Next() % 3 == 0 ? 6 : 5;
                t = SetPieces.rotateCW(t);
            }

            Noise noise = new Noise(Environment.TickCount); //Perlin noise
            for (int x = 0; x < 33; x++)
                for (int y = 0; y < 33; y++)
                    if (noise.GetNoise(x / 33f * 8, y / 33f * 8, .5f) < 0.2)
                        t[x, y] = 0;

            var dat = world.Manager.Resources.GameData;
            for (int x = 0; x < 33; x++)                    //Rendering
                for (int y = 0; y < 33; y++)
                {
                    if (t[x, y] == 1)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Grass];
                        tile.ObjType = 0;
                        tile.UpdateCount++;
                    }
                    else if (t[x, y] == 2)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[TileDark];
                        tile.ObjType = 0;
                        tile.UpdateCount++;
                    }
                    else if (t[x, y] == 3)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Tile];
                        tile.ObjType = 0;
                        tile.UpdateCount++;
                    }
                    else if (t[x, y] == 4)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Stone];
                        tile.ObjType = 0;
                        tile.UpdateCount++;
                    }
                    else if (t[x, y] == 5)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Stone];
                        tile.ObjType = dat.IdToObjectType[PillarA];
                        tile.ObjDesc = dat.ObjectDescs[tile.ObjType];
                        if (tile.ObjId == 0) tile.ObjId = world.GetNextEntityId();
                        tile.UpdateCount++;
                    }
                    else if (t[x, y] == 6)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Stone];
                        tile.ObjType = dat.IdToObjectType[PillarB];
                        tile.ObjDesc = dat.ObjectDescs[tile.ObjType];
                        if (tile.ObjId == 0) tile.ObjId = world.GetNextEntityId();
                        tile.UpdateCount++;
                    }
                }

            Entity skull = Entity.Resolve(world.Manager, "Skull Shrine");          //Skulls!
            skull.Move(pos.X + Size / 2f, pos.Y + Size / 2f);
            world.EnterWorld(skull);
        }
    }
}
