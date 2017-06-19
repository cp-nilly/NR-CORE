using common;

namespace wServer.networking.packets.incoming
{
    public class AcceptArenaDeath : IncomingMessage
    {
        public override PacketId ID => PacketId.ACCEPT_ARENA_DEATH;
        public override Packet CreateInstance() { return new AcceptArenaDeath(); }

        protected override void Read(NReader rdr) { }
        protected override void Write(NWriter wtr) { }
    }
}