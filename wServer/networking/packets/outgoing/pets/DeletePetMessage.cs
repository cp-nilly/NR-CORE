using common;

namespace wServer.networking.packets.outgoing.pets
{
    public class DeletePetMessage : OutgoingMessage
    {
        public int PetId { get; set; }

        public override PacketId ID => PacketId.DELETE_PET;
        public override Packet CreateInstance() { return new DeletePetMessage(); }

        protected override void Read(NReader rdr)
        {
            PetId = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(PetId);
        }
    }
}
