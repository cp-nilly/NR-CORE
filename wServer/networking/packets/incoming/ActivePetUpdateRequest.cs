using common;

namespace wServer.networking.packets.incoming
{
    public class ActivePetUpdateRequest : IncomingMessage
    {
        public const int Follow = 1;
        public const int Unfollow = 2;
        public const int Release = 3;

        public int CommandType { get; set; }
        public uint InstanceId { get; set; }

        public override PacketId ID => PacketId.ACTIVE_PET_UPDATE_REQUEST;
        public override Packet CreateInstance() { return new ActivePetUpdateRequest(); }

        protected override void Read(NReader rdr)
        {
            CommandType = rdr.ReadByte();
            InstanceId = (uint) rdr.ReadInt32();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write((byte) CommandType);
            wtr.Write((int) InstanceId);
        }
    }
}
