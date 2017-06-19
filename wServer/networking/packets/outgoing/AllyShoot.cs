using common;

namespace wServer.networking.packets.outgoing
{
    public class AllyShoot : OutgoingMessage
    {
        public byte BulletId { get; set; }
        public int OwnerId { get; set; }
        public ushort ContainerType { get; set; }
        public float Angle { get; set; }

        public override PacketId ID => PacketId.ALLYSHOOT;
        public override Packet CreateInstance() { return new AllyShoot(); }

        protected override void Read(NReader rdr)
        {
            BulletId = rdr.ReadByte();
            OwnerId = rdr.ReadInt32();
            ContainerType = rdr.ReadUInt16();
            Angle = rdr.ReadSingle();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(BulletId);
            wtr.Write(OwnerId);
            wtr.Write(ContainerType);
            wtr.Write(Angle);
        }
    }
}
