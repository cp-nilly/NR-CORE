using common;

namespace wServer.networking.packets.outgoing
{
    public class EnemyShoot : OutgoingMessage
    {
        public byte BulletId { get; set; }
        public int OwnerId { get; set; }
        public byte BulletType { get; set; }
        public Position StartingPos { get; set; }
        public float Angle { get; set; }
        public short Damage { get; set; }
        public byte NumShots { get; set; }
        public float AngleInc { get; set; }

        public override PacketId ID => PacketId.ENEMYSHOOT;
        public override Packet CreateInstance() { return new EnemyShoot(); }

        protected override void Read(NReader rdr)
        {
            BulletId = rdr.ReadByte();
            OwnerId = rdr.ReadInt32();
            BulletType = rdr.ReadByte();
            StartingPos = Position.Read(rdr);
            Angle = rdr.ReadSingle();
            Damage = rdr.ReadInt16();
            NumShots = rdr.ReadByte();
            AngleInc = rdr.ReadSingle();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(BulletId);
            wtr.Write(OwnerId);
            wtr.Write(BulletType);
            StartingPos.Write(wtr);
            wtr.Write(Angle);
            wtr.Write(Damage);
            wtr.Write(NumShots);
            wtr.Write(AngleInc);
        }
    }
}
