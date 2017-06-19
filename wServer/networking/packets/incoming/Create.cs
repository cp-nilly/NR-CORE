using common;

namespace wServer.networking.packets.incoming
{
    public class Create : IncomingMessage
    {
        public ushort ClassType { get; set; }
        public ushort SkinType { get; set; }

        public override PacketId ID => PacketId.CREATE;
        public override Packet CreateInstance() { return new Create(); }

        protected override void Read(NReader rdr)
        {
            ClassType = rdr.ReadUInt16();
            SkinType = rdr.ReadUInt16();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(ClassType);
            wtr.Write(SkinType);
        }
    }
}
