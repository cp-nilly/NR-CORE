using wServer.networking.packets;
using wServer.networking.packets.incoming;
using wServer.networking.packets.outgoing;

namespace wServer.networking.handlers
{
    class QuestFetchHandler : PacketHandlerBase<QuestFetch>
    {
        public override PacketId ID => PacketId.QUEST_FETCH_ASK;

        protected override void HandlePacket(Client client, QuestFetch packet)
        {
            var quest = client.Manager.Tinker.GetQuestForAccount(client.Account.AccountId) ??
                    client.Manager.Tinker.GenerateNew(client.Account);

            client.SendPacket(new QuestFetchResponse
            {
                Description =
                    "Hi my name is Mike Sellers, I need you to bring me a {goal} so I can make big money out of it.",
                Goal = quest.Goal.ToString(),
                Image = "http://i.imgur.com/i5UeOZF.png",
                Tier = quest.Tier
            });
        }
    }
}
