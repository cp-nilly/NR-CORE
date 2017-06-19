using common;

namespace wServer.networking.packets.outgoing
{
    public class QuestObjId : OutgoingMessage
    {
        public int ObjectId { get; set; }

        public override PacketId ID => PacketId.QUESTOBJID;
        public override Packet CreateInstance() { return new QuestObjId(); }

        protected override void Read(NReader rdr)
        {
            ObjectId = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(ObjectId);
        }
    }
}
