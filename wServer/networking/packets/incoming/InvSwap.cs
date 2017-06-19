using common;

namespace wServer.networking.packets.incoming
{
    public class InvSwap : IncomingMessage
    {
        public int Time { get; set; }
        public Position Position { get; set; }
        public ObjectSlot SlotObj1 { get; set; }
        public ObjectSlot SlotObj2 { get; set; }

        public override PacketId ID => PacketId.INVSWAP;
        public override Packet CreateInstance() { return new InvSwap(); }

        protected override void Read(NReader rdr)
        {
            Time = rdr.ReadInt32();
            Position = Position.Read(rdr);
            SlotObj1 = ObjectSlot.Read(rdr);
            SlotObj2 = ObjectSlot.Read(rdr);
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(Time);
            Position.Write(wtr);
            SlotObj1.Write(wtr);
            SlotObj2.Write(wtr);
        }
    }
}
