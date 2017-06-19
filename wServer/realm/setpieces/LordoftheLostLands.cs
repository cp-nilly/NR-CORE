using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using terrain;
using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class LordoftheLostLands : ISetPiece
    {
        public int Size { get { return 5; } }

        public void RenderSetPiece(World world, IntPoint pos)
        {
            Entity Lord = Entity.Resolve(world.Manager, "Lord of the Lost Lands");
            Lord.Move(pos.X + 2.5f, pos.Y + 2.5f);
            world.EnterWorld(Lord);
        }
    }
}
