using common;

namespace wServer.networking.packets.outgoing.pets
{
    public class HatchPetMessage : OutgoingMessage
    {
        public string PetName { get; set; }
        public int PetSkin { get; set; }

        public override PacketId ID => PacketId.HATCH_PET;
        public override Packet CreateInstance() { return new HatchPetMessage(); }

        protected override void Read(NReader rdr)
        {
            PetName = rdr.ReadUTF();
            PetSkin = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.WriteUTF(PetName);
            wtr.Write(PetSkin);
        }
    }
}
