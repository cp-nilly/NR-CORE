using wServer.realm.worlds;

namespace wServer.realm.setpieces
{
	class BedlamGod : ISetPiece
	{
		public int Size { get { return 5; } }

		public void RenderSetPiece(World world, IntPoint pos)
		{
			var BedlamGod = Entity.Resolve(world.Manager, "BedlamGod");
			BedlamGod.Move(pos.X + 2.5f, pos.Y + 2.5f);
			world.EnterWorld(BedlamGod);
		}
	}
}
