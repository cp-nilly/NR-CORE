using common;

namespace wServer.networking.packets.outgoing
{
    class GuildResult : OutgoingMessage
    {
        public bool Success { get; set; }
        public string LineBuilderJSON { get; set; }
        
        public override PacketId ID => PacketId.GUILDRESULT;
        public override Packet CreateInstance() { return new GuildResult(); }

        protected override void Read(NReader rdr)
        {
            Success = rdr.ReadBoolean();
            LineBuilderJSON = rdr.ReadUTF();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(Success);
            wtr.WriteUTF(LineBuilderJSON);
        }
    }
}
