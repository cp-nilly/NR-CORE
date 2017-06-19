using common;

namespace wServer.networking.packets.incoming
{
    public class ChangeTrade : IncomingMessage
    {
        public bool[] Offer { get; set; }

        public override PacketId ID => PacketId.CHANGETRADE;
        public override Packet CreateInstance() { return new ChangeTrade(); }

        protected override void Read(NReader rdr)
        {
            Offer = new bool[rdr.ReadInt16()];
            for (int i = 0; i < Offer.Length; i++)
                Offer[i] = rdr.ReadBoolean();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write((short)Offer.Length);
            foreach (var i in Offer)
                wtr.Write(i);
        }
    }
}
