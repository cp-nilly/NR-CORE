using common;

namespace wServer.networking.packets.outgoing
{
    public class PetYard : OutgoingMessage
    {
        public int Type { get; set; }

        public override PacketId ID => PacketId.PETYARDUPDATE;
        public override Packet CreateInstance() { return new PetYard(); }

        protected override void Read(NReader rdr)
        {
            Type = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(Type);
        }
    }
}