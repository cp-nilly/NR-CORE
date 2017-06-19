using common;

namespace wServer.networking.packets.incoming
{
    public class Escape : IncomingMessage
    {
        public override PacketId ID => PacketId.ESCAPE;
        public override Packet CreateInstance() { return new Escape(); }

        protected override void Read(NReader rdr) { }

        protected override void Write(NWriter wtr) { }
    }
}
