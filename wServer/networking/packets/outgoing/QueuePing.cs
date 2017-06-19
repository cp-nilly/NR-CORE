using common;

namespace wServer.networking.packets.outgoing
{
    public class QueuePing : OutgoingMessage
    {
        public int Serial { get; set; }
        public int Position { get; set; }
        public int Count { get; set; }

        public override PacketId ID => PacketId.QUEUE_PING;
        public override Packet CreateInstance() { return new QueuePing(); }

        protected override void Read(NReader rdr)
        {
            Serial = rdr.ReadInt32();
            Position = rdr.ReadInt32();
            Count = rdr.ReadInt32();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(Serial);
            wtr.Write(Position);
            wtr.Write(Count);
        }
    }
}
