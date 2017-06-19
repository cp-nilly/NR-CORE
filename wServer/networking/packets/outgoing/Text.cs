using common;

namespace wServer.networking.packets.outgoing
{
    public class Text : OutgoingMessage
    {
        public string Name { get; set; }
        public int ObjectId { get; set; }
        public int NumStars { get; set; }
        public int Admin { get; set; }
        public byte BubbleTime { get; set; }
        public string Recipient { get; set; }
        public string Txt { get; set; }
        public string CleanText { get; set; }
        public int NameColor { get; set; } = 0x123456;
        public int TextColor { get; set; } = 0x123456;

        public override PacketId ID => PacketId.TEXT;
        public override Packet CreateInstance() { return new Text(); }

        protected override void Read(NReader rdr)
        {
            Name = rdr.ReadUTF();
            ObjectId = rdr.ReadInt32();
            NumStars = rdr.ReadInt32();
            Admin = rdr.ReadInt32();
            BubbleTime = rdr.ReadByte();
            Recipient = rdr.ReadUTF();
            Txt = rdr.ReadUTF();
            CleanText = rdr.ReadUTF();
            NameColor = rdr.ReadInt32();
            TextColor = rdr.ReadInt32();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.WriteUTF(Name);
            wtr.Write(ObjectId);
            wtr.Write(NumStars);
            wtr.Write(Admin);
            wtr.Write(BubbleTime);
            wtr.WriteUTF(Recipient);
            wtr.WriteUTF(Txt);
            wtr.WriteUTF(CleanText);
            wtr.Write(NameColor);
            wtr.Write(TextColor);
        }
    }
}
