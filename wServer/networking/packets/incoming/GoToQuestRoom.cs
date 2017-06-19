using common;

namespace wServer.networking.packets.incoming
{
    public class GoToQuestRoom : IncomingMessage
    {
        public override PacketId ID => PacketId.QUEST_ROOM_MSG;
        public override Packet CreateInstance() { return new GoToQuestRoom(); }

        protected override void Read(NReader rdr) { }

        protected override void Write(NWriter wtr) { }
    }
}
