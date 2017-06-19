using common;
using wServer.realm.terrain;

namespace wServer.networking.packets.outgoing
{
    public class Update : OutgoingMessage
    {
        public struct TileData
        {
            public short X;
            public short Y;
            public Tile Tile;
        }

        public TileData[] Tiles { get; set; }
        public ObjectDef[] NewObjs { get; set; }
        public int[] Drops { get; set; }

        public override PacketId ID => PacketId.UPDATE;
        public override Packet CreateInstance() { return new Update(); }

        protected override void Read(NReader rdr)
        {
            Tiles = new TileData[rdr.ReadInt16()];
            for (var i = 0; i < Tiles.Length; i++)
            {
                Tiles[i] = new TileData()
                {
                    X = rdr.ReadInt16(),
                    Y = rdr.ReadInt16(),
                    Tile = (Tile)rdr.ReadUInt16(),
                };
            }

            NewObjs = new ObjectDef[rdr.ReadInt16()];
            for (var i = 0; i < NewObjs.Length; i++)
                NewObjs[i] = ObjectDef.Read(rdr);

            Drops = new int[rdr.ReadInt16()];
            for (var i = 0; i < Drops.Length; i++)
                Drops[i] = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write((short)Tiles.Length);
            foreach (var i in Tiles)
            {
                wtr.Write(i.X);
                wtr.Write(i.Y);
                wtr.Write((ushort)i.Tile);
            }
            wtr.Write((short)NewObjs.Length);
            foreach (var i in NewObjs)
            {
                i.Write(wtr);
            }
            wtr.Write((short)Drops.Length);
            foreach (var i in Drops)
            {
                wtr.Write(i);
            }
        }
    }
}
