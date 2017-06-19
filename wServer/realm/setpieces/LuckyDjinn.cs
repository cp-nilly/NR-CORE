using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using terrain;
using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class LuckyDjinn : ISetPiece
    {
        public int Size { get { return 5; } }

        public void RenderSetPiece(World world, IntPoint pos)
        {
            Entity LuckyDjinn = Entity.Resolve(world.Manager, "Lucky Djinn");
            LuckyDjinn.Move(pos.X + 2.5f, pos.Y + 2.5f);
            world.EnterWorld(LuckyDjinn);
        }
    }
}
