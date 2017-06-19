using common;

namespace wServer.networking.packets.outgoing
{
    public class FailureJsonDialogMessage
    {
        public string build; // provided as a check versus the client build, if failed gives client update message instead
        public string title;
        public string description;
    }

    public class Failure : OutgoingMessage
    {
        public const int ClientUpdateNeeded = 4;
        public const int MessageWithDisconnect = 5;
        public const int MessageWithImmediateReconnect = 6;
        public const int NoMessageDisconnect = 7;
        public const int JsonDialogDisconnect = 8;
        
        public int ErrorId { get; set; }
        public string ErrorDescription { get; set; }

        public override PacketId ID => PacketId.FAILURE;
        public override Packet CreateInstance() { return new Failure(); }

        protected override void Read(NReader rdr)
        {
            ErrorId = rdr.ReadInt32();
            ErrorDescription = rdr.ReadUTF();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(ErrorId);
            wtr.WriteUTF(ErrorDescription);
        }
    }
}
