using common;

namespace wServer.networking.packets.incoming
{
    public class RequestTrade : IncomingMessage
    {
        public string Name { get; set; }

        public override PacketId ID => PacketId.REQUESTTRADE;
        public override Packet CreateInstance() { return new RequestTrade(); }

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
