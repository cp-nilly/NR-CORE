using common;

namespace wServer.networking.packets.incoming.arena
{
    class EnterArena : IncomingMessage
    {
        public int Currency { get; set; }

        public override PacketId ID => PacketId.ENTER_ARENA;
        public override Packet CreateInstance() { return new EnterArena(); }

        protected override void Read(NReader rdr)
        {
            Currency = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(Currency);
        }
    }
}
