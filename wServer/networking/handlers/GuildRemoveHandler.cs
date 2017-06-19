using System.Linq;
using wServer.networking.packets;
using wServer.networking.packets.incoming;

namespace wServer.networking.handlers
{
    class GuildRemoveHandler : PacketHandlerBase<GuildRemove>
    {
        public override PacketId ID => PacketId.GUILDREMOVE;

        protected override void HandlePacket(Client client, GuildRemove packet)
        {
            //client.Manager.Logic.AddPendingAction(t => Handle(client, packet.Name));
            Handle(client, packet.Name);
        }

        private void Handle(Client source, string name)
        {
            if (source.Player == null || IsTest(source))
                return;

            var srcPlayer = source.Player;
            var manager = source.Manager;

            // if resigning
            if (source.Account.Name.Equals(name))
            {
                // chat needs to be done before removal so we can use
                // srcPlayer as a source for guild info
                manager.Chat.Guild(srcPlayer, srcPlayer.Name + " has left the guild.", true);

                if (!manager.Database.RemoveFromGuild(source.Account))
                {
                    srcPlayer.SendError("Guild not found.");
                    return;
                }

                srcPlayer.Guild = "";
                srcPlayer.GuildRank = 0;
                
                return;
            }

            // get target account id
            var targetAccId = source.Manager.Database.ResolveId(name);
            if (targetAccId == 0)
            {
                source.Player.SendError("Player not found");
                return;
            }
            
            // find target player (if connected)
            var targetClient = (from client in source.Manager.Clients.Keys 
                                where client.Account != null 
                                where client.Account.AccountId == targetAccId 
                                select client)
                                .FirstOrDefault();

            // try to remove connected member
            if (targetClient != null)
            {
                if (source.Account.GuildRank >= 20 &&
                    source.Account.GuildId == targetClient.Account.GuildId &&
                    source.Account.GuildRank > targetClient.Account.GuildRank)
                {
                    var targetPlayer = targetClient.Player;

                    if (!manager.Database.RemoveFromGuild(targetClient.Account))
                    {
                        srcPlayer.SendError("Guild not found.");
                        return;
                    }

                    targetPlayer.Guild = "";
                    targetPlayer.GuildRank = 0;

                    manager.Chat.Guild(srcPlayer,
                        targetPlayer.Name + " has been kicked from the guild by " + srcPlayer.Name, true);
                    targetPlayer.SendInfo("You have been kicked from the guild.");
                    return;
                }

                srcPlayer.SendError("Can't remove member. Insufficient privileges.");
                return;
            }
            
            // try to remove member via database
            var targetAccount = manager.Database.GetAccount(targetAccId);

            if (source.Account.GuildRank >= 20 &&
                source.Account.GuildId == targetAccount.GuildId &&
                source.Account.GuildRank > targetAccount.GuildRank)
            {
                if (!manager.Database.RemoveFromGuild(targetAccount))
                {
                    srcPlayer.SendError("Guild not found.");
                    return;
                }

                manager.Chat.Guild(srcPlayer,
                    targetAccount.Name + " has been kicked from the guild by " + srcPlayer.Name, true);
                manager.Chat.SendInfo(targetAccId, "You have been kicked from the guild.");
                return;
            }
            
            srcPlayer.SendError("Can't remove member. Insufficient privileges.");
        }
    }
}
