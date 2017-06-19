using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class Tower : ISetPiece
    {
        static int[,] quarter;
        static Tower()
        {
            string s =
"............XX\n" +
"........XXXXXX\n" +
"......XXXXXXXX\n" +
".....XXXX=====\n" +
"....XXX=======\n" +
"...XXX========\n" +
"..XXX=========\n" +
"..XX==========\n" +
".XXX==========\n" +
".XX===========\n" +
".XX===========\n" +
".XX===========\n" +
"XXX===========\n" +
"XXX===========";
            string[] a = s.Split('\n');
            quarter = new int[14, 14];
            for (int y = 0; y < 14; y++)
                for (int x = 0; x < 14; x++)
                    quarter[x, y] =
                        a[y][x] == 'X' ? 1 :
                            (a[y][x] == '=' ? 2 : 0);
        }

        public int Size { get { return 27; } }

        static readonly string Floor = "Rock";
        static readonly string Wall = "Grey Wall";

        Random rand = new Random();
        public void RenderSetPiece(World world, IntPoint pos)
        {
            int[,] t = new int[27, 27];

            int[,] q = (int[,])quarter.Clone();

            for (int y = 0; y < 14; y++)        //Top left
                for (int x = 0; x < 14; x++)
                    t[x, y] = q[x, y];

            q = SetPieces.reflectHori(q);           //Top right
            for (int y = 0; y < 14; y++)
                for (int x = 0; x < 14; x++)
                    t[13 + x, y] = q[x, y];

            q = SetPieces.reflectVert(q);           //Bottom right
            for (int y = 0; y < 14; y++)
                for (int x = 0; x < 14; x++)
                    t[13 + x, 13 + y] = q[x, y];

            q = SetPieces.reflectHori(q);           //Bottom left
            for (int y = 0; y < 14; y++)
                for (int x = 0; x < 14; x++)
                    t[x, 13 + y] = q[x, y];

            for (int y = 1; y < 4; y++)             //Opening
                for (int x = 8; x < 19; x++)
                    t[x, y] = 2;
            t[12, 0] = t[13, 0] = t[14, 0] = 2;


            int r = rand.Next(0, 4);                //Rotation
            for (int i = 0; i < r; i++)
                t = SetPieces.rotateCW(t);

            t[13, 13] = 3;

            var dat = world.Manager.Resources.GameData;
            for (int x = 0; x < 27; x++)            //Rendering
                for (int y = 0; y < 27; y++)
                {
                    if (t[x, y] == 1)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Floor];
                        tile.ObjType = dat.IdToObjectType[Wall];
                        tile.ObjDesc = dat.ObjectDescs[tile.ObjType];
                        if (tile.ObjId == 0) tile.ObjId = world.GetNextEntityId();
                        tile.UpdateCount++;
                    }
                    else if (t[x, y] == 2)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Floor];
                        tile.ObjType = 0;
                        tile.UpdateCount++;
                    }

                    else if (t[x, y] == 3)
                    {
                        var tile = world.Map[x + pos.X, y + pos.Y];
                        tile.TileId = dat.IdToTileType[Floor];
                        tile.ObjType = 0;
                        tile.UpdateCount++;
                        Entity ghostKing = Entity.Resolve(world.Manager, 0x0928);
                        ghostKing.Move(pos.X + x, pos.Y + y);
                        world.EnterWorld(ghostKing);
                    }
                }
        }
    }
}
