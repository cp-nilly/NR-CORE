using common;

namespace wServer.networking.packets.incoming
{
    public class Pong : IncomingMessage
    {
        public int Serial { get; set; }
        public int Time { get; set; }

        public override PacketId ID => PacketId.PONG;
        public override Packet CreateInstance() { return new Pong(); }

        protected override void Read(NReader rdr)
        {
            Serial = rdr.ReadInt32();
            Time = rdr.ReadInt32();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(Serial);
            wtr.Write(Time);
        }
    }
}
