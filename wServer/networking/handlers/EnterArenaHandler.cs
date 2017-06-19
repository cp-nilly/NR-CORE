using common.resources;
using wServer.networking.packets;
using wServer.networking.packets.incoming.arena;
using wServer.realm.worlds.logic;

namespace wServer.networking.handlers
{
    class EnterArenaHandler : PacketHandlerBase<EnterArena>
    {
        private const int FameCost = 500;
        private const int GoldCost = 50;

        public override PacketId ID => PacketId.ENTER_ARENA;

        protected override void HandlePacket(Client client, EnterArena packet)
        {
            //client.Manager.Logic.AddPendingAction(t => Handle(client, (CurrencyType)packet.PaymentTransType));
            Handle(client, (CurrencyType)packet.Currency);
        }

        void Handle(Client client, CurrencyType currency)
        {
            if (IsTest(client))
                return;

            var plr = client.Player;
            var acnt = client.Account;

            if (plr == null || plr.Owner is Test) return;

            if (!client.Account.NameChosen)
            {
                plr.SendError("You need to pick a name first.");
                return;
            }

            switch (currency)
            {
                case CurrencyType.Fame:
                    if (acnt.Fame < FameCost)
                    {
                        plr.SendError("Not enough fame.");
                        return;
                    }
                    client.Manager.Database.UpdateFame(acnt, -FameCost);
                    break;

                case CurrencyType.Gold:
                    if (acnt.Credits < GoldCost)
                    {
                        plr.SendError("Not enough gold.");
                        return;
                    }
                    client.Manager.Database.UpdateCredit(acnt, -GoldCost);
                    break;

                default:
                    plr.SendError("Currency type invalid.");
                    return;
            }

            var proto = plr.Manager.Resources.Worlds["Arena"];
            var world = plr.Manager.GetWorld(proto.id);
            plr.Reconnect(world);
        }
    }
}
