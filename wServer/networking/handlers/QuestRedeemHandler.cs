#define NO_TINKER_QUESTS //remove this line to enable tinkering

using System.Threading.Tasks;
using common.resources;
using wServer.networking.packets;
using wServer.networking.packets.incoming.arena;
using wServer.networking.packets.outgoing;
using wServer.realm;

namespace wServer.networking.handlers
{
    class QuestRedeemHandler : PacketHandlerBase<QuestRedeem>
    {
        public override PacketId ID => PacketId.QUEST_REDEEM;

        protected override void HandlePacket(Client client, QuestRedeem redeem)
        {
#if !NO_TINKER_QUESTS
            var quest = client.Manager.Tinker.GetQuestForAccount(client.Account.AccountId);

            if (!client.Player.Inventory.ValidateSlot(redeem.Object))
            {
                client.SendPacket(new QuestRedeemResponse
                {
                    Ok = false,
                    Message = "Invalid slot."
                });
                return;
            }

            if (redeem.Object.ObjectType != quest.Goal)
            {
                client.SendPacket(new QuestRedeemResponse
                {
                    Ok = false,
                    Message = "I HAVENT REQUESTED THAT ITEM YOU FUCKING SHITCUNT."
                });
                return;
            }

            //credits for now, just so I can test it
            var t1 = client.Manager.Database.UpdateCredit(client.Account, 1);
            client.Player.Credits++;
            client.Player.Inventory.QueueChange(redeem.Object.SlotId, null);
            var trans = client.Manager.Database.Conn.CreateTransaction();
            var t2 = Inventory.TrySaveChangesAsync(trans, client.Player);
            var t3 = trans.ExecuteAsync();
            Task.WhenAll(t1, t2, t3).ContinueWith(t => client.Player.UpdateCount++);
            client.SendPacket(new QuestRedeemResponse
            {
                Ok = true,
                Message = ""
            });

            quest.Goal = Tinker.RandomGoal(client.Manager.Resources.GameData);
            quest.Tier++;

            client.Manager.Tinker.UpdateAsync(quest);
#else
            client.SendPacket(new QuestRedeemResponse
            {
                Ok = false,
                Message = "Tinkering is disabled atm."
            });
#endif
        }
    }
}
