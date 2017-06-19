using common;
using wServer.realm.entities;
using wServer.networking.packets;
using wServer.networking.packets.incoming;
using wServer.networking.packets.outgoing;
using wServer.realm;
using wServer.realm.worlds.logic;

namespace wServer.networking.handlers
{
    class CreateHandler : PacketHandlerBase<Create>
    {
        public override PacketId ID => PacketId.CREATE;

        protected override void HandlePacket(Client client, Create packet)
        {
            //client.Manager.Logic.AddPendingAction(t => Handle(client, packet));
            Handle(client, packet);
        }

        private void Handle(Client client, Create packet)
        {
            if (client.State != ProtocolState.Handshaked)
                return;

            DbChar character;
            var status = client.Manager.Database.CreateCharacter(
                client.Manager.Resources.GameData, client.Account, packet.ClassType, packet.SkinType, out character);

            if (status == CreateStatus.ReachCharLimit)
            {
                client.SendFailure("Too many characters",
                    Failure.MessageWithDisconnect);
                return;
            }

            if (status == CreateStatus.SkinUnavailable)
            {
                client.SendFailure("Skin unavailable",
                    Failure.MessageWithDisconnect);
                return;
            }

            if (status == CreateStatus.Locked)
            {
                client.SendFailure("Class locked",
                    Failure.MessageWithDisconnect);
                return;
            }

            CreatePlayer(client, character);
        }

        private void CreatePlayer(Client client, DbChar character)
        {
            client.Character = character;

            var target = client.Manager.Worlds[client.TargetWorld];

            client.Player = target is Test ?
                        new Player(client, false) :
                        new Player(client);

            client.SendPacket(new CreateSuccess()
            {
                CharId = client.Character.CharId,
                ObjectId = target.EnterWorld(client.Player)
            }, PacketPriority.High);

            client.State = ProtocolState.Ready;
            client.Manager.ConMan.ClientConnected(client);
        }
    }
}
