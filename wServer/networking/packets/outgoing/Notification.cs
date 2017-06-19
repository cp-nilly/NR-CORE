using common;

namespace wServer.networking.packets.outgoing
{
    public class Notification : OutgoingMessage
    {
        public int ObjectId { get; set; }
        public string Message { get; set; }
        public ARGB Color { get; set; }

        public override PacketId ID => PacketId.NOTIFICATION;
        public override Packet CreateInstance() { return new Notification(); }

        protected override void Read(NReader rdr)
        {
            ObjectId = rdr.ReadInt32();
            Message = rdr.ReadUTF();
            Color = ARGB.Read(rdr);
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(ObjectId);
            wtr.WriteUTF(Message);
            Color.Write(wtr);
        }
    }
}
