using common;

namespace wServer.networking.packets.outgoing
{
    public class SwitchMusic : OutgoingMessage
    {
        public string Music { get; set; }

        public override PacketId ID => PacketId.SWITCH_MUSIC;
        public override Packet CreateInstance() { return new SwitchMusic(); }

        protected override void Read(NReader rdr)
        {
            Music = rdr.ReadUTF();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.WriteUTF(Music);
        }
    }
}
