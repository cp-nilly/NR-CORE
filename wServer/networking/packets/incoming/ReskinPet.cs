using common;

namespace wServer.networking.packets.incoming
{
    public class ReskinPet : IncomingMessage
    {
        public int PetInstanceId { get; set; }
        public int PickedNewPetType { get; set; }
        public ObjectSlot Item { get; set; }

        public override PacketId ID => PacketId.PET_CHANGE_FORM_MSG;
        public override Packet CreateInstance() { return new ReskinPet(); }

        protected override void Read(NReader rdr)
        {
            PetInstanceId = rdr.ReadInt32();
            PickedNewPetType = rdr.ReadInt32();
            Item = ObjectSlot.Read(rdr);
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(PetInstanceId);
            wtr.Write(PickedNewPetType);
            Item.Write(wtr);
        }
    }
}
