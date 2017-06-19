using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class GhostShip : ISetPiece
    {
        public int Size { get { return 40; } }

        public void RenderSetPiece(World world, IntPoint pos)
        {
            var proto = world.Manager.Resources.Worlds["GhostShip"];
            SetPieces.RenderFromProto(world, pos, proto);
        }
    }
}
