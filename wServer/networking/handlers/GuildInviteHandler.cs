using wServer.networking.packets;
using wServer.networking.packets.incoming;
using wServer.networking.packets.outgoing;

namespace wServer.networking.handlers
{
    class GuildInviteHandler : PacketHandlerBase<GuildInvite>
    {
        public override PacketId ID => PacketId.GUILDINVITE;

        protected override void HandlePacket(Client client, GuildInvite packet)
        {
            //client.Manager.Logic.AddPendingAction(t => Handle(client, packet.Name));
            Handle(client, packet.Name);
        }

        private void Handle(Client src, string playerName)
        {
            if (src.Player == null || IsTest(src))
                return;

            if (src.Account.GuildRank < 20)
            {
                src.Player.SendError("Insufficient privileges.");
                return;
            }

            foreach (var client in src.Manager.Clients.Keys)
            {
                if (client.Player == null ||
                    client.Account == null ||
                    !client.Account.Name.Equals(playerName))
                    continue;

                if (!client.Account.NameChosen)
                {
                    src.Player.SendError("Player needs to choose a name first.");
                    return;
                }

                if (client.Account.GuildId > 0)
                {
                    src.Player.SendError("Player is already in a guild.");
                    return;
                }

                client.Player.GuildInvite = src.Account.GuildId;

                client.SendPacket(new InvitedToGuild()
                {
                    Name = src.Account.Name,
                    GuildName = src.Player.Guild
                });
                return;
            }

            src.Player.SendError("Could not find the player to invite.");
        }
    }
}
