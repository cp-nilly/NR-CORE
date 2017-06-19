using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class FanaticofChaos : ISetPiece
    {
        public int Size { get { return 32; } }

        public void RenderSetPiece(World world, IntPoint pos)
        {
            var FanaticofChaos = Entity.Resolve(world.Manager, "Fanatic of Chaos");
            FanaticofChaos.Move(pos.X + 2.5f, pos.Y + 2.5f);
            world.EnterWorld(FanaticofChaos);
        }
    }
}
