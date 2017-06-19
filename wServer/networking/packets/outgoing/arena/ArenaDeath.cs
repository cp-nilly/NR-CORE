using common;

namespace wServer.networking.packets.outgoing.arena
{
    public class ArenaDeath : OutgoingMessage
    {
        public int Cost { get; set; }

        public override PacketId ID => PacketId.ARENA_DEATH;
        public override Packet CreateInstance() { return new ArenaDeath(); }

        protected override void Read(NReader rdr)
        {
            Cost = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(Cost);
        }
    }
}