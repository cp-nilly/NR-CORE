using common;

namespace wServer.networking.packets.incoming
{
    public class ClaimDailyRewardMessage : IncomingMessage
    {
        public string ClaimKey { get; set; }
        public string Type { get; set; }

        public override PacketId ID => PacketId.CLAIM_LOGIN_REWARD_MSG;
        public override Packet CreateInstance() { return new ClaimDailyRewardMessage(); }

        protected override void Read(NReader rdr)
        {
            ClaimKey = rdr.ReadUTF();
            Type = rdr.ReadUTF();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.WriteUTF(ClaimKey);
            wtr.WriteUTF(Type);
        }
    }
}
