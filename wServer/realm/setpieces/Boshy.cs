using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class Boshy : ISetPiece
    {
        public int Size { get { return 5; } }

        public void RenderSetPiece(World world, IntPoint pos)
        {
            var boshy = Entity.Resolve(world.Manager, "Boshy");
            boshy.Move(pos.X + 2.5f, pos.Y + 2.5f);
            world.EnterWorld(boshy);
        }
    }
}
