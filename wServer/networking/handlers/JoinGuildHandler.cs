using System;
using common;
using wServer.networking.packets;
using wServer.networking.packets.incoming;

namespace wServer.networking.handlers
{
    class JoinGuildHandler : PacketHandlerBase<JoinGuild>
    {
        public override PacketId ID => PacketId.JOINGUILD;

        protected override void HandlePacket(Client client, JoinGuild packet)
        {
            //client.Manager.Logic.AddPendingAction(t => Handle(client, packet.GuildName));
            Handle(client, packet.GuildName);
        }

        private void Handle(Client src, string guildName)
        {
            if (src.Player == null || IsTest(src))
                return;

            if (src.Player.GuildInvite == null)
            {
                src.Player.SendError("You have not been invited to a guild.");
                return;
            }

            var guild = src.Manager.Database.GetGuild((int)src.Player.GuildInvite);

            if (guild == null)
            {
                src.Player.SendError("Internal server error.");
                return;
            }

            if (!guild.Name.Equals(guildName, StringComparison.InvariantCultureIgnoreCase))
            {
                src.Player.SendError("You have not been invited to join " + guildName + ".");
                return;
            }

            var result = src.Manager.Database.AddGuildMember(guild, src.Account);
            if (result != AddGuildMemberStatus.OK)
            {
                src.Player.SendError("Could not join guild. (" + result + ")");
                return;
            }

            src.Player.Guild = guild.Name;
            src.Player.GuildRank = 0;

            src.Manager.Chat.Guild(src.Player, src.Player.Name + " has joined the guild!", true);
        }
    }
}
