using common;

namespace wServer.networking.packets.outgoing
{
    public class Pic : OutgoingMessage
    {
        public BitmapData BitmapData { get; set; }

        public override PacketId ID => PacketId.PIC;
        public override Packet CreateInstance() { return new Pic(); }

        protected override void Read(NReader rdr)
        {
            BitmapData = BitmapData.Read(rdr);
        }

        protected override void Write(NWriter wtr)
        {
            BitmapData.Write(wtr);
        }
    }
}