using common;

namespace wServer.networking.packets.outgoing
{
    public class ClientStat : OutgoingMessage
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public override PacketId ID => PacketId.CLIENTSTAT;
        public override Packet CreateInstance() { return new ClientStat(); }

        protected override void Read(NReader rdr)
        {
            Name = rdr.ReadUTF();
            Value = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.WriteUTF(Name);
            wtr.Write(Value);
        }
    }
}