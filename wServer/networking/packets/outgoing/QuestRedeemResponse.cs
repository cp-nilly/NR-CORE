using common;

namespace wServer.networking.packets.outgoing
{
    public class QuestRedeemResponse : OutgoingMessage
    {
        public bool Ok { get; set; }
        public string Message { get; set; }

        public override PacketId ID => PacketId.QUEST_REDEEM_RESPONSE;
        public override Packet CreateInstance() { return new QuestRedeemResponse(); }

        protected override void Read(NReader rdr)
        {
            Ok = rdr.ReadBoolean();
            Message = rdr.ReadUTF();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(Ok);
            wtr.WriteUTF(Message);
        }
    }
}
