using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class Sanic : ISetPiece
    {
        public int Size { get { return 5; } }

        public void RenderSetPiece(World world, IntPoint pos)
        {
            var sanic = Entity.Resolve(world.Manager, "Sanic");
            sanic.Move(pos.X + 2.5f, pos.Y + 2.5f);
            world.EnterWorld(sanic);
        }
    }
}
