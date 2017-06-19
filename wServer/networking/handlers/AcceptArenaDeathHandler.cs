using wServer.networking.packets;
using wServer.networking.packets.incoming;

namespace wServer.networking.handlers
{
    class AcceptArenaDeathHandler : PacketHandlerBase<AcceptArenaDeath>
    {
        public override PacketId ID => PacketId.ACCEPT_ARENA_DEATH;

        protected override void HandlePacket(Client client, AcceptArenaDeath death)
        {
            //TODO: implement something
        }
    }
}
