using System;
using common;

namespace wServer.networking.packets.outgoing
{
    public class MarketResult : OutgoingMessage
    {
        public const int MARKET_ERROR = 0;
        public const int MARKET_SUCCESS = 1;
        public const int MARKET_REQUEST_RESULT = 2;

        public byte CommandId { get; set; }
        public string Message { get; set; }
        public PlayerShopItem[] Items { get; set; }
        
        public override PacketId ID => PacketId.MARKET_RESULT;

        public override Packet CreateInstance()
        {
            return new MarketResult();
        }

        protected override void Read(NReader rdr)
        {
            throw new InvalidOperationException("Nope, no read server packetzzzz :D");
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(CommandId);

            switch (CommandId)
            {
                case MARKET_ERROR:
                case MARKET_SUCCESS:
                    wtr.WriteUTF(Message);
                    break;
                case MARKET_REQUEST_RESULT:
                    wtr.Write(Items.Length);
                    foreach (var playerShopItem in Items)
                        playerShopItem.Write(wtr);
                    break;
            }
        }

        public static MarketResult Error(string errorMessage)
        {
            return new MarketResult
            {
                CommandId = MARKET_ERROR,
                Message = errorMessage
            };
        }

        public static MarketResult Success(string successMessage)
        {
            return new MarketResult
            {
                CommandId = MARKET_SUCCESS,
                Message = successMessage
            };
        }
    }
}
