using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class Megaman : ISetPiece
    {
        public int Size { get { return 5; } }

        public void RenderSetPiece(World world, IntPoint pos)
        {
            var Megaman = Entity.Resolve(world.Manager, "Megaman");
            Megaman.Move(pos.X + 2.5f, pos.Y + 2.5f);
            world.EnterWorld(Megaman);
        }
    }
}
