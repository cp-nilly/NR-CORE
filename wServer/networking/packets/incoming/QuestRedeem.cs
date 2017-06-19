using common;

namespace wServer.networking.packets.incoming.arena
{
    public class QuestRedeem : IncomingMessage
    {
        public ObjectSlot Object { get; set; }

        public override PacketId ID => PacketId.QUEST_REDEEM;
        public override Packet CreateInstance() { return new QuestRedeem(); }

        protected override void Read(NReader rdr)
        {
            Object = ObjectSlot.Read(rdr);
        }

        protected override void Write(NWriter wtr)
        {
            Object.Write(wtr);
        }
    }
}
