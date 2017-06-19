using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
    class KageKami : ISetPiece
    {
        public int Size { get { return 65; } }

        public void RenderSetPiece(World world, IntPoint pos)
        {
            var proto = world.Manager.Resources.Worlds["KageKami"];
            SetPieces.RenderFromProto(world, pos, proto);
        }
    }
}
