using common;

namespace wServer.networking.packets.incoming
{
    public class PlayerShoot : IncomingMessage
    {
        public int Time { get; set; }
        public byte BulletId { get; set; }
        public ushort ContainerType { get; set; }
        public Position StartingPos { get; set; }
        public float Angle { get; set; }

        public override PacketId ID => PacketId.PLAYERSHOOT;
        public override Packet CreateInstance() { return new PlayerShoot(); }

        protected override void Read(NReader rdr)
        {
            Time = rdr.ReadInt32();
            BulletId = rdr.ReadByte();
            ContainerType = rdr.ReadUInt16();
            StartingPos = Position.Read(rdr);
            Angle = rdr.ReadSingle();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(Time);
            wtr.Write(BulletId);
            wtr.Write(ContainerType);
            StartingPos.Write(wtr);
            wtr.Write(Angle);
        }
    }
}
