using common;

namespace wServer.networking.packets.outgoing
{
    public class PasswordPrompt : OutgoingMessage
    {
        public const int SIGN_IN = 2;
        public const int SEND_EMAIL_AND_SIGN_IN = 3;
        public const int REGISTER = 4;

        public int CleanPasswordStatus { get; set; }

        public override PacketId ID => PacketId.PASSWORD_PROMPT;
        public override Packet CreateInstance() { return new PasswordPrompt(); }

        protected override void Read(NReader rdr)
        {
            CleanPasswordStatus = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(CleanPasswordStatus);
        }
    }
}