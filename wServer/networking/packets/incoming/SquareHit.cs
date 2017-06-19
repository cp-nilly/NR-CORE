using common;

namespace wServer.networking.packets.incoming
{
    public class SquareHit : IncomingMessage
    {
        public int Time { get; set; }
        public byte BulletId { get; set; }
        public int ObjectId { get; set; }

        public override PacketId ID => PacketId.SQUAREHIT;
        public override Packet CreateInstance() { return new SquareHit(); }

        protected override void Read(NReader rdr)
        {
            Time = rdr.ReadInt32();
            BulletId = rdr.ReadByte();
            ObjectId = rdr.ReadInt32();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(Time);
            wtr.Write(BulletId);
            wtr.Write(ObjectId);
        }
    }
}
