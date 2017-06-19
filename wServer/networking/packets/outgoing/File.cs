using common;

namespace wServer.networking.packets.outgoing
{
    public class File : OutgoingMessage
    {
        public string Name { get; set; }
        public byte[] Bytes { get; set; }

        public override PacketId ID => PacketId.FILE;
        public override Packet CreateInstance() { return new File(); }

        protected override void Read(NReader rdr)
        {
            Name = rdr.ReadUTF();
            Bytes = new byte[rdr.ReadInt32()];
            Bytes = rdr.ReadBytes(Bytes.Length);
        }

        protected override void Write(NWriter wtr)
        {
            wtr.WriteUTF(Name);
            wtr.Write(Bytes.Length);
            wtr.Write(Bytes);
        }
    }
}