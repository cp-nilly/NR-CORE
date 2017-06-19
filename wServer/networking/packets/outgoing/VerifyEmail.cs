using common;

namespace wServer.networking.packets.outgoing
{
    public class VerifyEmail : OutgoingMessage
    {
        public override PacketId ID => PacketId.VERIFY_EMAIL;
        public override Packet CreateInstance() { return new VerifyEmail(); }

        protected override void Read(NReader rdr) { }
        protected override void Write(NWriter wtr) { }
    }
}
