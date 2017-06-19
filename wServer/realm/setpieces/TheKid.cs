using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class TheKid : ISetPiece
    {
        public int Size { get { return 5; } }

        public void RenderSetPiece(World world, IntPoint pos)
        {
            var theKid = Entity.Resolve(world.Manager, "The Kid");
            theKid.Move(pos.X + 2.5f, pos.Y + 2.5f);
            world.EnterWorld(theKid);
        }
    }
}
