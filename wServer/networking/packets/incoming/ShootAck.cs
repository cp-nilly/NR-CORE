using common;

namespace wServer.networking.packets.incoming
{
    public class ShootAck : IncomingMessage
    {
        public int Time { get; set; }

        public override PacketId ID => PacketId.SHOOTACK;
        public override Packet CreateInstance() { return new ShootAck(); }

        protected override void Read(NReader rdr)
        {
            Time = rdr.ReadInt32();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(Time);
        }
    }
}
