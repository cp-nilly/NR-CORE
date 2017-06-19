using common;

namespace wServer.networking.packets.incoming
{
    public class ChooseName : IncomingMessage
    {
        public string Name { get; set; }

        public override PacketId ID => PacketId.CHOOSENAME;
        public override Packet CreateInstance() { return new ChooseName(); }

        protected override void Read(NReader rdr)
        {
            Name = rdr.ReadUTF();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.WriteUTF(Name);
        }
    }
}
