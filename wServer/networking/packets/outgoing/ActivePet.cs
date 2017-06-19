using common;

namespace wServer.networking.packets.outgoing
{
    public class ActivePet : OutgoingMessage
    {
        public int InstanceId { get; set; }

        public override PacketId ID => PacketId.ACTIVEPETUPDATE;
        public override Packet CreateInstance() { return new ActivePet(); }

        protected override void Read(NReader rdr)
        {
            InstanceId = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(InstanceId);
        }
    }
}