using common;

namespace wServer.networking.packets.incoming
{
    public class KeyInfoRequest : IncomingMessage
    {
        public int ItemType;

        public override PacketId ID => PacketId.KEY_INFO_REQUEST;
        public override Packet CreateInstance() { return new KeyInfoRequest(); }

        protected override void Read(NReader rdr)
        {
            ItemType = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(ItemType);
        }
    }
}
