using common;

namespace wServer.networking.packets.incoming
{
    public class Buy : IncomingMessage
    {
        public int ObjectId { get; set; }
        public int Quantity { get; set; }

        public override PacketId ID => PacketId.BUY;
        public override Packet CreateInstance() { return new Buy(); }

        protected override void Read(NReader rdr)
        {
            ObjectId = rdr.ReadInt32();
            Quantity = rdr.ReadInt32();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(ObjectId);
            wtr.Write(Quantity);
        }
    }
}
