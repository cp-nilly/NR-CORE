using common;

namespace wServer.networking.packets.outgoing
{
    public class KeyInfoResponse : OutgoingMessage
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Creator { get; set; }

        public override PacketId ID => PacketId.KEY_INFO_RESPONSE;
        public override Packet CreateInstance() { return new InvResult(); }

        protected override void Read(NReader rdr)
        {
            Name = rdr.ReadUTF();
            Description = rdr.ReadUTF();
            Creator = rdr.ReadUTF();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(Name);
            wtr.Write(Description);
            wtr.Write(Creator);
        }
    }
}
