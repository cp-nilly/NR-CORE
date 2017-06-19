using common;

namespace wServer.networking.packets.outgoing
{
    public class SetFocus : OutgoingMessage
    {
        public int ObjectId { get; set; }

        public override PacketId ID => PacketId.SET_FOCUS;
        public override Packet CreateInstance() { return new SetFocus(); }

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
