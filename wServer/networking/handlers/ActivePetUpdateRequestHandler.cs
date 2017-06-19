using System.Linq;
using wServer.networking.packets;
using wServer.networking.packets.incoming;
using wServer.networking.packets.outgoing;
using wServer.networking.packets.outgoing.pets;
using wServer.realm.entities;
using PetYard = wServer.realm.worlds.logic.PetYard;

namespace wServer.networking.handlers
{
    class ActivePetUpdateRequestHandler : PacketHandlerBase<ActivePetUpdateRequest>
    {
        public override PacketId ID => PacketId.ACTIVE_PET_UPDATE_REQUEST;

        protected override void HandlePacket(Client client, ActivePetUpdateRequest packet)
        {
            var petYard = client.Player.Owner as PetYard;
            var pet = petYard?.Pets.Values.SingleOrDefault(p => p.PetId == packet.InstanceId);
            if (pet == null)
                return;

            switch (packet.CommandType)
            {
                case ActivePetUpdateRequest.Follow:
                    Follow(client, pet);
                    break;
                case ActivePetUpdateRequest.Unfollow:
                    Unfollow(client, pet);
                    break;
                case ActivePetUpdateRequest.Release:
                    Release(client, pet);
                    break;
            }
        }

        private void Follow(Client client, Pet pet)
        {
            var player = client.Player;
            if (player.Pet != null)
                player.Pet.PlayerOwner = null;
            pet.PlayerOwner = player;
            player.Pet = pet;
            
            client.SendPacket(new ActivePet()
            {
                InstanceId = pet.PetId
            });
        }

        private void Unfollow(Client client, Pet pet)
        {
            pet.PlayerOwner = null;
            client.Player.Pet = null;
            
            client.SendPacket(new ActivePet()
            {
                InstanceId = -1
            });
        }

        private void Release(Client client, Pet pet)
        {
            // if has owner, remove before releasing
            if (pet.PlayerOwner != null)
            {
                pet.PlayerOwner.Pet = null;
                pet.PlayerOwner.Client
                    .SendPacket(new ActivePet()
                {
                    InstanceId = -1
                });

                pet.PlayerOwner = null;
            }

            client.Manager.Database.RemovePet(client.Account, pet.PetId);
            
            var petYard = client.Player.Owner as PetYard;
            if (petYard == null)
                return;

            petYard.LeaveWorld(pet);

            client.SendPacket(new DeletePetMessage()
            {
                PetId = pet.PetId
            });
        }
    }
}
