using common;

namespace wServer.networking.packets.outgoing
{
    public class NewTick : OutgoingMessage
    {
        public int TickId { get; set; }
        public int TickTime { get; set; }
        public ObjectStats[] Statuses { get; set; }

        public override PacketId ID => PacketId.NEWTICK;
        public override Packet CreateInstance() { return new NewTick(); }

        protected override void Read(NReader rdr)
        {
            TickId = rdr.ReadInt32();
            TickTime = rdr.ReadInt32();

            Statuses = new ObjectStats[rdr.ReadInt16()];
            for (var i = 0; i < Statuses.Length; i++)
                Statuses[i] = ObjectStats.Read(rdr);
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(TickId);
            wtr.Write(TickTime);

            wtr.Write((short)Statuses.Length);
            foreach (var i in Statuses)
                i.Write(wtr);
        }
    }
}
