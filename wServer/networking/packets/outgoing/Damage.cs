using System.Collections.Generic;
using common;
using common.resources;

namespace wServer.networking.packets.outgoing
{
    public class Damage : OutgoingMessage
    {
        public int TargetId { get; set; }
        public ConditionEffects Effects { get; set; }
        public ushort DamageAmount { get; set; }
        public bool Kill { get; set; }
        public byte BulletId { get; set; }
        public int ObjectId { get; set; }

        public override PacketId ID => PacketId.DAMAGE;
        public override Packet CreateInstance() { return new Damage(); }

        protected override void Read(NReader rdr)
        {
            TargetId = rdr.ReadInt32();
            byte c = rdr.ReadByte();
            Effects = 0;
            for (int i = 0; i < c; i++)
                Effects |= (ConditionEffects)(1 << rdr.ReadByte());
            DamageAmount = rdr.ReadUInt16();
            Kill = rdr.ReadBoolean();
            BulletId = rdr.ReadByte();
            ObjectId = rdr.ReadInt32();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(TargetId);
            List<byte> eff = new List<byte>();
            for (byte i = 1; i < 255; i++)
                if ((Effects & (ConditionEffects)(1 << i)) != 0)
                    eff.Add(i);
            wtr.Write((byte)eff.Count);
            foreach (var i in eff) wtr.Write(i);
            wtr.Write(DamageAmount);
            wtr.Write(Kill);
            wtr.Write(BulletId);
            wtr.Write(ObjectId);
        }
    }
}
