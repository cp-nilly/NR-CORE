using common;

namespace wServer.networking.packets.outgoing
{
    public class ClaimDailyRewardResponse : OutgoingMessage
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public int Gold { get; set; }

        public override PacketId ID => PacketId.LOGIN_REWARD_MSG;
        public override Packet CreateInstance() { return new ClaimDailyRewardResponse(); }

        protected override void Read(NReader rdr)
        {
            ItemId = rdr.ReadInt32();
            Quantity = rdr.ReadInt32();
            Gold = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(ItemId);
            wtr.Write(Quantity);
            wtr.Write(Gold);
        }
    }
}
