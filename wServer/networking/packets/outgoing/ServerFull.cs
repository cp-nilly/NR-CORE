using common;

namespace wServer.networking.packets.outgoing
{
    public class ServerFull : OutgoingMessage
    {
        public int Position { get; set; }
        public int Count { get; set; }

        public override PacketId ID => PacketId.SERVER_FULL;
        public override Packet CreateInstance() { return new ServerFull(); }

        protected override void Read(NReader rdr)
        {
            Position = rdr.ReadInt32();
            Count = rdr.ReadInt32();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(Position);
            wtr.Write(Count);
        }
    }
}
