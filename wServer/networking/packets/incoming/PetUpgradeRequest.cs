using common;
using common.resources;

namespace wServer.networking.packets.incoming
{
    public class PetUpgradeRequest : IncomingMessage
    {
        public const int UPGRADE_PET_YARD = 1;
        public const int FEED_PET = 2;
        public const int FUSE_PET = 3;

        public byte PetTransType { get; set; }
        public int PetId1 { get; set; }
        public int PetId2 { get; set; }
        public int ObjectId { get; set; }
        public ObjectSlot SlotObject { get; set; }
        public CurrencyType PaymentTransType { get; set; }

        public override PacketId ID => PacketId.PETUPGRADEREQUEST;
        public override Packet CreateInstance() { return new PetUpgradeRequest(); }

        protected override void Read(NReader rdr)
        {
            PetTransType = rdr.ReadByte();
            PetId1 = rdr.ReadInt32();
            PetId2 = rdr.ReadInt32();
            ObjectId = rdr.ReadInt32();
            SlotObject = ObjectSlot.Read(rdr);
            PaymentTransType = (CurrencyType)rdr.ReadByte();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(PetTransType);
            wtr.Write(PetId1);
            wtr.Write(PetId2);
            wtr.Write(ObjectId);
            SlotObject.Write(wtr);
            wtr.Write((byte)PaymentTransType);
        }
    }
}
