using System.IO;
using System.Linq;
using common;
using common.resources;
using wServer.networking;
using wServer.realm.entities;
using wServer.realm.terrain;

namespace wServer.realm.worlds.logic
{
    public class PetYard : World
    {
        private readonly Client _client;
        private readonly int _accountId;

        public PetYard(ProtoWorld proto, Client client = null) : base(proto)
        {
            if (client == null)
                return;

            _client = client;
            _accountId = _client.Account.AccountId;
        }

        public override bool AllowedAccess(Client client)
        {
            return base.AllowedAccess(client) && _accountId == client.Account.AccountId;
        }

        protected override void Init()
        {
            if (IsLimbo)
                return;

            switch (_client.Account.PetYardType)
            {
                case 2:
                    FromWorldMap(new MemoryStream(Manager.Resources.Worlds[Name].wmap[1]));
                    break;
                case 3:
                    FromWorldMap(new MemoryStream(Manager.Resources.Worlds[Name].wmap[2]));
                    break;
                case 4:
                    FromWorldMap(new MemoryStream(Manager.Resources.Worlds[Name].wmap[3]));
                    break;
                case 5:
                    FromWorldMap(new MemoryStream(Manager.Resources.Worlds[Name].wmap[4]));
                    break;
                default:
                    FromWorldMap(new MemoryStream(Manager.Resources.Worlds[Name].wmap[0]));
                    break;
            }

            LoadPets();
        }

        private void LoadPets()
        {
            if (!Manager.Config.serverSettings.enablePets)
                return;

            var removedPet = false;
            var acc = _client.Account;
            foreach (var petId in acc.PetList)
            {
                var dbPet = new DbPet(acc, petId);
                if (dbPet.ObjectType == 0)
                {
                    removedPet = true;
                    Manager.Database.RemovePet(acc, petId);
                    continue;
                }
                
                var pet = new Pet(Manager, null, dbPet);
                var sPos = GetPetSpawnPosition();
                pet.Move(sPos.X, sPos.Y);
                EnterWorld(pet);
            }
            if (removedPet)
                _client.Account.Reload();
        }

        public Position GetPetSpawnPosition()
        {
            var x = 0;
            var y = 0;

            var spawnRegions = Map.Regions.Where(t => t.Value == TileRegion.PetRegion).ToArray();
            if (spawnRegions.Length > 0)
            {
                var sRegion = spawnRegions.ElementAt(Rand.Next(0, spawnRegions.Length));
                x = sRegion.Key.X;
                y = sRegion.Key.Y;
            }
            
            return new Position() { X = x, Y = y };
        }

        public override int EnterWorld(Entity entity)
        {
            var pet = entity as Pet;
            if (pet == null) 
                return base.EnterWorld(entity);
            
            var existingPet = Pets.Values.SingleOrDefault(p => p.PetId == pet.PetId);
            if (existingPet != null)
                LeaveWorld(existingPet);

            return base.EnterWorld(entity);
        }
    }
}
