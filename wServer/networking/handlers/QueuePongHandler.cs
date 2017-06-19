using wServer.networking.packets;
using wServer.networking.packets.incoming;
using wServer.realm;

namespace wServer.networking.handlers
{
    class QueuePongHandler : PacketHandlerBase<QueuePong>
    {
        public override PacketId ID => PacketId.QUEUE_PONG;

        protected override void HandlePacket(Client client, QueuePong packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client, packet, t));
        }

        private void Handle(Client client, QueuePong packet, RealmTime t)
        {
            client.Pong(t, packet);
        }
    }
}
