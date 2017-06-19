using common;

namespace wServer.networking.packets.outgoing
{
    public class ShowEffect : OutgoingMessage
    {
        public EffectType EffectType { get; set; }
        public int TargetObjectId { get; set; }
        public Position Pos1 { get; set; }
        public Position Pos2 { get; set; }
        public ARGB Color { get; set; }

        public override PacketId ID => PacketId.SHOWEFFECT;
        public override Packet CreateInstance() { return new ShowEffect(); }

        protected override void Read(NReader rdr)
        {
            EffectType = (EffectType)rdr.ReadByte();
            TargetObjectId = rdr.ReadInt32();
            Pos1 = Position.Read(rdr);
            Pos2 = Position.Read(rdr);
            Color = ARGB.Read(rdr);
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write((byte)EffectType);
            wtr.Write(TargetObjectId);
            Pos1.Write(wtr);
            Pos2.Write(wtr);
            Color.Write(wtr);
        }
    }
}
