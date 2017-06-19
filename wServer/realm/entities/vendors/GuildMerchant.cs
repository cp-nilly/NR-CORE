using System;
using common.resources;
using wServer.networking.packets.outgoing;

namespace wServer.realm.entities.vendors
{
    class GuildMerchant : SellableObject
    {
        private readonly int[] _hallTypes = new int[] {0x736, 0x737, 0x738};
        private readonly int[] _hallPrices = new int[] {10000, 100000, 250000};
        private readonly int[] _hallLevels = new int[] {1, 2, 3};

        private readonly int _upgradeLevel;

        public GuildMerchant(RealmManager manager, ushort objType) : base(manager, objType)
        {
            Currency = CurrencyType.Fame;
            Price = Int32.MaxValue; // just in case for some reason _hallType isn't found
            for (int i = 0; i < _hallTypes.Length; i++)
            {
                if (objType != _hallTypes[i])
                    continue;

                Price = _hallPrices[i];
                _upgradeLevel = _hallLevels[i];
            }
        }

        public override void Buy(Player player)
        {
            var account = player.Manager.Database.GetAccount(player.AccountId);
            var guild = player.Manager.Database.GetGuild(account.GuildId);


            if (guild.IsNull || account.GuildRank < 30)
            {
                player.SendError("Verification failed.");
                return;
            }

            if (guild.Fame < Price)
            {
                player.Client.SendPacket(new networking.packets.outgoing.BuyResult
                {
                    ResultString = "Not enough Guild Fame!",
                    Result = 9
                });
                return;
            }

            // change guild level
            if (!player.Manager.Database.ChangeGuildLevel(guild, _upgradeLevel))
            {
                player.SendError("Internal server error.");
                return;
            }

            player.Manager.Database.UpdateGuildFame(guild, -Price);
            player.Client.SendPacket(new networking.packets.outgoing.BuyResult
            {
                ResultString = "Upgrade successful! Please leave the Guild Hall to have it upgraded.",
                Result = 0
            });
        }
    }
}
