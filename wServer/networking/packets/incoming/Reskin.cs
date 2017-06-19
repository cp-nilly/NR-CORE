using common;

namespace wServer.networking.packets.incoming
{
    class Reskin : IncomingMessage
    {
        public int SkinId { get; set; }

        public override PacketId ID => PacketId.RESKIN;
        public override Packet CreateInstance() { return new Reskin(); }

        protected override void Read(NReader rdr)
        {
            SkinId = rdr.ReadInt32();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(SkinId);
        }
    }
}
