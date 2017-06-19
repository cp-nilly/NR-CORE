using common;

namespace wServer.networking.packets.outgoing
{
    public class ReskinUnlock : OutgoingMessage
    {
        public int SkinId { get; set; }

        public override PacketId ID => PacketId.RESKIN_UNLOCK;
        public override Packet CreateInstance() { return new ReskinUnlock(); }

        protected override void Read(NReader rdr)
        {
            SkinId = rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(SkinId);
        }
    }
}