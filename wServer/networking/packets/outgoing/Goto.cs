using common;

namespace wServer.networking.packets.outgoing
{
    public class Goto : OutgoingMessage
    {
        public int ObjectId { get; set; }
        public Position Pos { get; set; }

        public override PacketId ID => PacketId.GOTO;
        public override Packet CreateInstance() { return new Goto(); }

        protected override void Read(NReader rdr)
        {
            ObjectId = rdr.ReadInt32();
            Pos = Position.Read(rdr);
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(ObjectId);
            Pos.Write(wtr);
        }
    }
}
