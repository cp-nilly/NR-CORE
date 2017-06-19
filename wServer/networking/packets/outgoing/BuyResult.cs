using common;

namespace wServer.networking.packets.outgoing
{
    public class BuyResult : OutgoingMessage
    {
        public int Result { get; set; }
        public string ResultString { get; set; }

        public override PacketId ID => PacketId.BUYRESULT;
        public override Packet CreateInstance() { return new BuyResult(); }

        protected override void Read(NReader rdr)
        {
            Result = rdr.ReadInt32();
            ResultString = rdr.ReadUTF();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(Result);
            wtr.WriteUTF(ResultString);
        }
    }
}
