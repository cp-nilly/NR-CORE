using common;

namespace wServer.networking.packets.outgoing
{
    public class QuestFetchResponse : OutgoingMessage
    {
        public int Tier { get; set; }
        public string Goal { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }

        public override PacketId ID => PacketId.QUEST_FETCH_RESPONSE;
        public override Packet CreateInstance() { return new QuestFetchResponse(); }
        
        protected override void Read(NReader rdr)
        {
            Tier = rdr.ReadInt32();
            Goal = rdr.ReadUTF();
            Description = rdr.ReadUTF();
            Image = rdr.ReadUTF();
        }
        
        protected override void Write(NWriter wtr)
        {
            wtr.Write(Tier);
            wtr.WriteUTF(Goal);
            wtr.WriteUTF(Description);
            wtr.WriteUTF(Image);
        }
    }
}
