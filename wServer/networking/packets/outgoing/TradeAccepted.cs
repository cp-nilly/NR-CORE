using common;

namespace wServer.networking.packets.outgoing
{
    public class TradeAccepted : OutgoingMessage
    {
        public bool[] MyOffer { get; set; }
        public bool[] YourOffer { get; set; }

        public override PacketId ID => PacketId.TRADEACCEPTED;
        public override Packet CreateInstance() { return new TradeAccepted(); }

        protected override void Read(NReader rdr)
        {
            MyOffer = new bool[rdr.ReadInt16()];
            for (int i = 0; i < MyOffer.Length; i++)
                MyOffer[i] = rdr.ReadBoolean();

            YourOffer = new bool[rdr.ReadInt16()];
            for (int i = 0; i < YourOffer.Length; i++)
                YourOffer[i] = rdr.ReadBoolean();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write((short)MyOffer.Length);
            foreach (var i in MyOffer)
                wtr.Write(i);
            wtr.Write((short)YourOffer.Length);
            foreach (var i in YourOffer)
                wtr.Write(i);
        }
    }
}
