using common;

namespace wServer.networking.packets.incoming
{
    public class Move : IncomingMessage
    {
        public int ObjectId { get; set; }
        public int TickId { get; set; }
        public int Time { get; set; }
        public Position NewPosition { get; set; }
        public TimedPosition[] Records { get; set; }
        
        public override PacketId ID => PacketId.MOVE;
        public override Packet CreateInstance() { return new Move(); }

        protected override void Read(NReader rdr)
        {
            ObjectId = rdr.ReadInt32();
            TickId = rdr.ReadInt32();
            Time = rdr.ReadInt32();
            NewPosition = Position.Read(rdr);
            Records = new TimedPosition[rdr.ReadInt16()];
            for (var i = 0; i < Records.Length; i++)
                Records[i] = TimedPosition.Read(rdr);
        }
        protected override void Write(NWriter wtr)
        {
            wtr.Write(ObjectId);
            wtr.Write(TickId);
            wtr.Write(Time);
            NewPosition.Write(wtr);
            wtr.Write((short)Records.Length);
            foreach (var i in Records)
                i.Write(wtr);
        }
    }
}
