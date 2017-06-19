using common;

namespace wServer.networking.packets.incoming
{
    public class InvDrop : IncomingMessage
    {
        public ObjectSlot SlotObject { get; set; }

        public override PacketId ID => PacketId.INVDROP;
        public override Packet CreateInstance() { return new InvDrop(); }

        protected override void Read(NReader rdr)
        {
            SlotObject = ObjectSlot.Read(rdr);
        }
        protected override void Write(NWriter wtr)
        {
            SlotObject.Write(wtr);
        }
    }
}
