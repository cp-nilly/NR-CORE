using System.Linq;
using wServer.networking.packets;
using wServer.networking.packets.incoming;

namespace wServer.networking.handlers
{
    class ChangeGuildRankHandler : PacketHandlerBase<ChangeGuildRank>
    {
        public override PacketId ID => PacketId.CHANGEGUILDRANK;

        protected override void HandlePacket(Client client, ChangeGuildRank packet)
        {
            //client.Manager.Logic.AddPendingAction(t => Handle(client, packet.Name, packet.GuildRank));
            Handle(client, packet.Name, packet.GuildRank);
        }

        private void Handle(Client client, string name, int rank)
        {
            var manager = client.Manager;
            var srcAcnt = client.Account;
            var srcPlayer = client.Player;

            if (srcPlayer == null || IsTest(client))
                return;

            var targetId = client.Manager.Database.ResolveId(name);
            if (targetId == 0)
            {
                srcPlayer.SendError("A player with that name does not exist.");
                return;
            }

            // get target client if available (player is currently connected to the server)
            // otherwise pull account from db
            var target = client.Manager.Clients.Keys
                .SingleOrDefault(c => c.Account.AccountId == targetId);
            var targetAcnt = target != null ? target.Account : manager.Database.GetAccount(targetId);

            if (srcAcnt.GuildId <= 0 ||
                srcAcnt.GuildRank < 20 ||
                srcAcnt.GuildRank <= targetAcnt.GuildRank ||
                srcAcnt.GuildRank < rank ||
                rank == 40 ||
                srcAcnt.GuildId != targetAcnt.GuildId)
            {
                srcPlayer.SendError("No permission");
                return;
            }

            var targetRank = targetAcnt.GuildRank;
            if (targetRank == rank)
            {
                srcPlayer.SendError("Player is already a " + ResolveRank(rank));
                return;
            }

            // change rank
            if (!client.Manager.Database.ChangeGuildRank(targetAcnt, rank))
            {
                srcPlayer.SendError("Failed to change rank.");
                return;
            }

            // update player if connected
            if (target != null)
                target.Player.GuildRank = rank;
            
            // notify guild
            if (targetRank < rank)
                client.Manager.Chat.Guild(
                    srcPlayer,
                    targetAcnt.Name + " has been promoted to " + ResolveRank(rank) + ".",
                    true);
            else
                client.Manager.Chat.Guild(
                    srcPlayer,
                    targetAcnt.Name + " has been demoted to " + ResolveRank(rank) + ".",
                    true);
        }

        private string ResolveRank(int rank)
        {
            switch (rank)
            {
                case 0:  return "Initiate";
                case 10: return "Member";
                case 20: return "Officer";
                case 30: return "Leader";
                case 40: return "Founder";
                default: return "";
            }
        }
    }
}
