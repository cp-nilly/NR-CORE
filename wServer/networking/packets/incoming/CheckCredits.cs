using common;

namespace wServer.networking.packets.incoming
{
    public class CheckCredits : IncomingMessage
    {
        public override PacketId ID => PacketId.CHECKCREDITS;
        public override Packet CreateInstance() { return new CheckCredits(); }

        protected override void Read(NReader rdr) { }

        protected override void Write(NWriter wtr) { }
    }
}
