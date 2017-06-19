using System.Collections.Generic;
using System.Linq;
using common;
using common.resources;
using wServer.networking.packets;
using wServer.networking.packets.incoming;
using wServer.networking.packets.outgoing;
using wServer.networking.packets.outgoing.pets;
using wServer.realm.entities;
using wServer.realm.worlds.logic;
using PetYard = wServer.networking.packets.outgoing.PetYard;

namespace wServer.networking.handlers
{
    class PetUpgradeRequestHandler : PacketHandlerBase<PetUpgradeRequest>
    {
        public override PacketId ID => PacketId.PETUPGRADEREQUEST;

        protected override void HandlePacket(Client client, PetUpgradeRequest packet)
        {
            if (packet.PaymentTransType != CurrencyType.Fame &&
                packet.PaymentTransType != CurrencyType.Gold)
            {
                client.Player.SendError("Unsupported currency type.");
                return;
            }

            switch (packet.PetTransType)
            {
                case PetUpgradeRequest.UPGRADE_PET_YARD:
                    UpgradePetYard(client, packet);
                    break;
                case PetUpgradeRequest.FEED_PET:
                    FeedPet(client, packet);
                    break;
                case PetUpgradeRequest.FUSE_PET:
                    FusePet(client, packet);
                    break;
            }
        }

        public static readonly Dictionary<int, int[]> PetYardCost = 
            new Dictionary<int, int[]>()
        {
            {1, new int[] {150, 500} },
            {2, new int[] {400, 2000} },
            {3, new int[] {1200, 25000} },
            {4, new int[] {2000, 50000} }
        };

        public static readonly Dictionary<PRarity, int[]> FeedCost =
            new Dictionary<PRarity, int[]>()
        {
            {PRarity.Common, new int[] {5, 0}},//10} },
            {PRarity.Uncommon, new int[] {12, 0}},//30} },
            {PRarity.Rare, new int[] {30, 0}},//100} },
            {PRarity.Legendary, new int[] {60, 0}},//350} },
            {PRarity.Divine, new int[] {150, 0}},//1000} }
        };

        public static readonly Dictionary<PRarity, int[]> FuseCost =
            new Dictionary<PRarity, int[]>()
        {
            {PRarity.Common, new int[] {100, 0}},//300} },
            {PRarity.Uncommon, new int[] {240, 0}},//1000} },
            {PRarity.Rare, new int[] {600, 0}},//4000} },
            {PRarity.Legendary, new int[] {1800, 0}},//15000} }
        };

        private void FusePet(Client client, PetUpgradeRequest packet)
        {
            var petYard = client.Player.Owner as realm.worlds.logic.PetYard;
            if (petYard == null)
                return;

            var pet1 = petYard.Pets.Values.SingleOrDefault(p => p.PetId == packet.PetId1);
            var pet2 = petYard.Pets.Values.SingleOrDefault(p => p.PetId == packet.PetId2);
            if (pet1 == null || pet2 == null || pet1.Rarity != pet2.Rarity || 
                pet1.Rarity == PRarity.Divine || pet1.Rarity == PRarity.Undefined)
                return;

            if (TryDeduct(packet.PaymentTransType, client.Player,
                FuseCost[pet1.Rarity][(int) packet.PaymentTransType]))
            {
                client.SendPacket(new BuyResult
                {
                    Result = 0,
                    ResultString = "{\"key\":\"server.buy_success\"}"
                });

                var result = pet1.Fuse(pet2);
                if (result < pet1.Ability.Length && result != 0)
                    client.SendPacket(new NewAbilityMessage()
                    {
                        Type = pet1.Ability[result].Type
                    });
                else
                {
                    // reload pet with new evolved skin
                    petYard.LeaveWorld(pet1);
                    var player = client.Player;
                    var dPet = new DbPet(client.Account, pet1.PetId);
                    var pet = new Pet(client.Manager, null, dPet);
                    if (pet1.PlayerOwner == player)
                    {
                        player.Pet = pet;
                        pet.PlayerOwner = player;
                        pet.Move(player.X, player.Y);
                    }
                    else
                    {
                        var sp = petYard.GetPetSpawnPosition();
                        pet.Move(sp.X, sp.Y);
                    }
                    petYard.EnterWorld(pet);
                    
                    client.SendPacket(new EvolvedPetMessage()
                    {
                        PetId = pet1.PetId,
                        InitialSkin = pet1.SkinId,
                        FinalSkin = result
                    });
                }

                // remove fused pet
                client.Manager.Database.RemovePet(client.Account, pet2.PetId);
                petYard.LeaveWorld(pet2);
                client.SendPacket(new DeletePetMessage()
                {
                    PetId = pet2.PetId
                });
            }
        }

        private void FeedPet(Client client, PetUpgradeRequest packet)
        {
            var petYard = client.Player.Owner as realm.worlds.logic.PetYard;
            var pet = petYard?.Pets.Values.SingleOrDefault(p => p.PetId == packet.PetId1);
            if (pet == null)
                return;

            var player = client.Player;
            var slot = packet.SlotObject.SlotId;
            if (slot < client.Manager.Resources.Settings.InventorySize &&
                TryDeduct(packet.PaymentTransType, player, FeedCost[pet.Rarity][(int)packet.PaymentTransType]))
            {
                client.SendPacket(new BuyResult
                {
                    Result = 0,
                    ResultString = "{\"key\":\"server.buy_success\"}"
                });

                var item = player.Inventory[slot];
                player.Inventory[slot] = null;
                pet.Feed(item?.FeedPower ?? 0);
            }
        }

        private void UpgradePetYard(Client client, PetUpgradeRequest packet)
        {
            var acc = client.Account;
            if (acc.PetYardType > 4)
            {
                client.Player.SendError("Your PetYard is already maxed.");
                return;
            }

            if (TryDeduct(packet.PaymentTransType, client.Player, 
                PetYardCost[acc.PetYardType][(int)packet.PaymentTransType]))
            {
                acc.PetYardType++;
                acc.FlushAsync();

                client.SendPacket(new PetYard
                {
                    Type = client.Account.PetYardType
                });
            }
        }

        private bool TryDeduct(CurrencyType currency, Player player, int price)
        {
            if (player.Owner is Test)
                return false;

            var acc = player.Client.Account;
            var db = player.Manager.Database;
            if (acc.Guest)
            {
                // reload acc just in case user registered in game
                acc.FlushAsync();
                acc.Reload();
                if (acc.Guest) return false;
            }

            if (currency == CurrencyType.Fame)
            {
                if (acc.Fame < price)
                {
                    player.SendError("{\"key\":\"server.not_enough_fame\"}");
                    return false;
                }
                db.UpdateFame(acc, -price);
                player.CurrentFame = acc.Fame;
                return true;
            }
            if (currency == CurrencyType.Gold)
            {
                if (acc.Credits < price)
                {
                    player.SendError("{\"key\":\"server.not_enough_gold\"}");
                    return false;
                }
                db.UpdateCredit(acc, -price);
                player.Credits = acc.Credits;
                return true;
            }

            return false;
        }
    }
}
