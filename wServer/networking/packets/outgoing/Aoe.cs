using common;
using common.resources;

namespace wServer.networking.packets.outgoing
{
    public class Aoe : OutgoingMessage
    {
        public Position Pos { get; set; }
        public float Radius { get; set; }
        public ushort Damage { get; set; }
        public ConditionEffectIndex Effect { get; set; }
        public float Duration { get; set; }
        public ushort OrigType { get; set; }

        public override PacketId ID => PacketId.AOE;
        public override Packet CreateInstance() { return new Aoe(); }

        protected override void Read(NReader rdr)
        {
            Pos = Position.Read(rdr);
            Radius = rdr.ReadSingle();
            Damage = rdr.ReadUInt16();
            Effect = (ConditionEffectIndex)rdr.ReadByte();
            Duration = rdr.ReadSingle();
            OrigType = rdr.ReadUInt16();
        }
        protected override void Write(NWriter wtr)
        {
            Pos.Write(wtr);
            wtr.Write(Radius);
            wtr.Write(Damage);
            wtr.Write((byte)Effect);
            wtr.Write(Duration);
            wtr.Write(OrigType);
        }
    }
}
