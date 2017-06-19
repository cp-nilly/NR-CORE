using common;

namespace wServer.networking.packets.outgoing
{
    public class PlaySound : OutgoingMessage
    {
        public int OwnerId { get; set; }
        public int SoundId { get; set; }

        public override PacketId ID => PacketId.PLAYSOUND;
        public override Packet CreateInstance() { return new PlaySound(); }

        protected override void Read(NReader rdr)
        {
            OwnerId = rdr.ReadInt32();
            SoundId = rdr.ReadByte();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(OwnerId);
            wtr.Write((byte) SoundId);
        }
    }
}
