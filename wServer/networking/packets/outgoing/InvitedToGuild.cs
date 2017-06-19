using common;

namespace wServer.networking.packets.outgoing
{
    class InvitedToGuild : OutgoingMessage
    {
        public string Name;
        public string GuildName;

        public override PacketId ID => PacketId.INVITEDTOGUILD;
        public override Packet CreateInstance() { return new InvitedToGuild(); }

        protected override void Read(NReader rdr)
        {
            Name = rdr.ReadUTF();
            GuildName = rdr.ReadUTF();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.WriteUTF(Name);
            wtr.WriteUTF(GuildName);
        }
    }
}
