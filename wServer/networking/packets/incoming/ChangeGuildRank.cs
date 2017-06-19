using common;

namespace wServer.networking.packets.incoming
{
    class ChangeGuildRank : IncomingMessage
    {
        public string Name;
        public int GuildRank;

        public override PacketId ID => PacketId.CHANGEGUILDRANK;
        public override Packet CreateInstance() { return new ChangeGuildRank(); }

        protected override void Read(NReader rdr)
        {
            Name = rdr.ReadUTF();
            GuildRank = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.WriteUTF(Name);
            wtr.Write(GuildRank);
        }
    }
}
