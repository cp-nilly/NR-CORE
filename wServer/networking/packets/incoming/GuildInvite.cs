using common;

namespace wServer.networking.packets.incoming
{
    class GuildInvite : IncomingMessage
    {
        public string Name;

        public override PacketId ID => PacketId.GUILDINVITE;
        public override Packet CreateInstance() { return new GuildInvite(); }

        protected override void Read(NReader rdr)
        {
            Name = rdr.ReadUTF();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.WriteUTF(Name);
        }
    }
}
