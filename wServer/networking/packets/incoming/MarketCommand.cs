using System;
using common;

namespace wServer.networking.packets.incoming
{
    public class MarketCommand : IncomingMessage
    {
        public const int REQUEST_MY_ITEMS = 0;
        public const int ADD_OFFER = 1;
        public const int REMOVE_OFFER = 2;

        public byte CommandId { get; set; }
        public MarketOffer[] NewOffers { get; set; }
        public uint OfferId { get; set; }

        public override PacketId ID => PacketId.MARKET_COMMAND;
        public override Packet CreateInstance() { return new MarketCommand(); }

        protected override void Read(NReader rdr)
        {
            CommandId = rdr.ReadByte();

            switch (CommandId)
            {
                case REQUEST_MY_ITEMS:
                    break;
                case ADD_OFFER:
                    NewOffers = new MarketOffer[rdr.ReadInt32()];
                    for (int i = 0; i < NewOffers.Length; i++)
                        NewOffers[i] = MarketOffer.Read(rdr);
                    break;
                case REMOVE_OFFER:
                    OfferId = rdr.ReadUInt32();
                    break;
            }

        }

        protected override void Write(NWriter wtr)
        {
            throw new InvalidOperationException("Nope, no write client packetzzzz :D");
        }
    }
}
