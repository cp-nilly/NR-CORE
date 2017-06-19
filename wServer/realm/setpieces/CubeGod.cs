using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using terrain;
using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class CubeGod : ISetPiece
    {
        public int Size { get { return 5; } }

        public void RenderSetPiece(World world, IntPoint pos)
        {
            Entity cube = Entity.Resolve(world.Manager, "Cube God");
            cube.Move(pos.X + 2.5f, pos.Y + 2.5f);
            world.EnterWorld(cube);
        }
    }
}
