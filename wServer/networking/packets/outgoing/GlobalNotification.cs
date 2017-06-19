using common;

namespace wServer.networking.packets.outgoing
{
    public class GlobalNotification : OutgoingMessage
    {
        public const int ADD_ARENA = 1;
        public const int DELETE_ARENA = 2;

        public int Type { get; set; }
        public string Text { get; set; }

        public override PacketId ID => PacketId.GLOBAL_NOTIFICATION;
        public override Packet CreateInstance() { return new GlobalNotification(); }

        protected override void Read(NReader rdr)
        {
            Type = rdr.ReadInt32();
            Text = rdr.ReadUTF();
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(Type);
            wtr.WriteUTF(Text);
        }
    }
}
