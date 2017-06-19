using common;

namespace wServer.networking.packets.outgoing
{
    public class EvolvedPetMessage : OutgoingMessage
    {
        public int PetId { get; set; }
        public int InitialSkin { get; set; }
        public int FinalSkin { get; set; }

        public override PacketId ID => PacketId.EVOLVE_PET;
        public override Packet CreateInstance() { return new EvolvedPetMessage(); }

        protected override void Read(NReader rdr)
        {
            PetId = rdr.ReadInt32();
            InitialSkin = rdr.ReadInt32();
            FinalSkin = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(PetId);
            wtr.Write(InitialSkin);
            wtr.Write(FinalSkin);
        }
    }
}
