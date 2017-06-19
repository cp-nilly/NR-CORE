using common;

namespace wServer.networking.packets.incoming
{
    public class GroundDamage : IncomingMessage
    {
        public int Time { get; set; }
        public Position Position { get; set; }

        public override PacketId ID => PacketId.GROUNDDAMAGE;
        public override Packet CreateInstance() { return new GroundDamage(); }

        protected override void Read(NReader rdr)
        {
            Time = rdr.ReadInt32();
            Position = Position.Read(rdr);
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(Time);
            Position.Write(wtr);
        }
    }
}
