using common;

namespace wServer.networking.packets.incoming
{
    public class QuestFetch : IncomingMessage
    {
        public override PacketId ID => PacketId.QUEST_FETCH_ASK;
        public override Packet CreateInstance() { return new QuestFetch(); }

        protected override void Read(NReader rdr)
        {
        }

        protected override void Write(NWriter wtr)
        {
        }
    }
}

