using System;
using System.Collections.Generic;
using System.Linq;
using common;
using common.resources;

namespace wServer.realm.entities
{
    public class Pet : Entity, IPlayer
    {
        public Player PlayerOwner { get; set; }
        public int PetId { get; private set; }
        public PFamily Family { get; private set; }
        public PRarity Rarity { get; private set; }
        public int SkinId { get; private set; }
        public string Skin { get; private set; }
        public int MaxLevel { get; private set; }
        public PetAbility[] Ability { get; private set; }

        private readonly DbPet _dbPet;

        public Pet(RealmManager manager, ushort objType) : base(manager, objType)
        {
            var gameData = manager.Resources.GameData;
            var petDesc = gameData.Pets[objType];
            var petSkin = gameData.PetSkins[gameData.IdToObjectType[petDesc.DefaultSkin]];

            Family = petDesc.Family;
            Rarity = petDesc.Rarity;
            SkinId = petSkin.ObjectType;
            Skin = petSkin.DisplayId;
            SetDefaultSize(petDesc.MinSize);

            MaxLevel = 30;
            Ability = new PetAbility[DbPet.NumAbilities];
            for (var i = 0; i < Ability.Length; i++)
                Ability[i] = new PetAbility(this, i);
        }

        public Pet(RealmManager manager, Player playerOwner, DbPet pet) : base(manager, pet.ObjectType)
        {
            PlayerOwner = playerOwner;
            _dbPet = pet;

            var gameData = manager.Resources.GameData;
            var petDesc = gameData.Pets[(pet.ObjectType == 0) ? (ushort)0x7f05 : pet.ObjectType];
            var petSkin = gameData.PetSkins[gameData.IdToObjectType[petDesc.DefaultSkin]];

            Family = petDesc.Family;
            SkinId = petSkin.ObjectType;
            Skin = petSkin.DisplayId;
            SetDefaultSize(petDesc.MinSize);

            PetId = pet.PetId;
            Rarity = pet.Rarity;
            MaxLevel = pet.MaxLevel;
            Ability = new PetAbility[DbPet.NumAbilities];
            for (var i = 0; i < Ability.Length; i++)
                Ability[i] = new PetAbility(this, pet, i);
        }

        protected override void ExportStats(IDictionary<StatsType, object> stats)
        {
            base.ExportStats(stats);
            
            stats[StatsType.PetType] = (int)ObjectType;
            stats[StatsType.PetRarity] = (int)Rarity;
            stats[StatsType.Skin] = SkinId;
            stats[StatsType.PetSkin] = Skin;
            stats[StatsType.PetId] = PetId;
            stats[StatsType.PetMaxLevel] = MaxLevel;
            stats[StatsType.PetAbilityPower1] = Ability[0].Power;
            stats[StatsType.PetAbilityPower2] = Ability[1].Power;
            stats[StatsType.PetAbilityPower3] = Ability[2].Power;
            stats[StatsType.PetAbilityLevel1] = Ability[0].Level;
            stats[StatsType.PetAbilityLevel2] = Ability[1].Level;
            stats[StatsType.PetAbilityLevel3] = Ability[2].Level;
            stats[StatsType.PetAbilityType1] = (int)Ability[0].Type;
            stats[StatsType.PetAbilityType2] = (int)Ability[1].Type;
            stats[StatsType.PetAbilityType3] = (int)Ability[2].Type;
        }

        public void Damage(int dmg, Entity src) { }

        public bool IsVisibleToEnemy()
        {
            return false;
        }

        public static Pet Create(RealmManager manager, Player player, Item egg)
        {
            // there are no explicit uncommon/legendary pets so this is need
            var rarity = egg.Rarity;
            switch (rarity)
            {
                case PRarity.Uncommon:
                    rarity = PRarity.Common;
                    break;
                case PRarity.Legendary:
                    rarity = PRarity.Rare;
                    break;
            }

            var petDesc = GetRandomPetDesc(manager, rarity, egg.Family);
            if (petDesc == null)
            {
                player.SendErrorFormat("Bad egg. Family: {0} and Rarity: {1} combination does not exist.",
                    egg.Family.ToString(), egg.Rarity.ToString());
                return null;
            }

            var acc = player.Client.Account;
            if (acc.PetList.Length >= player.Manager.Resources.Settings.MaxPetCount)
            {
                player.SendError("Pet cap has been reached. Please release a pet before adding new ones.");
                return null;
            }

            var dbPet = manager.Database.NewPet(acc);
            dbPet.ObjectType = petDesc.ObjectType;
            var pet = new Pet(manager, player, dbPet);
            pet.Rarity = egg.Rarity;
            pet.MaxLevel = GetRarityMaxLevel(egg.Rarity);
            InitializeAbilities(manager, pet, petDesc);
            pet.Feed(GetNeededPower(GetRarityMaxLevel(egg.Rarity - 1) - 1));
            pet.Save();

            return pet;
        }

        public static int GetNeededPower(int currentLevel)
        {
            return (int)(20 * (1 - Math.Pow(1.08, currentLevel)) / (1 - 1.08)) + 1;
        }

        private static PetDesc GetRandomPetDesc(RealmManager manager, 
            PRarity rarity = PRarity.Undefined, PFamily family = PFamily.Undefined)
        {
            var petDescs = manager.Resources.GameData.Pets.Values
                .Where(d => rarity == PRarity.Undefined || rarity == d.Rarity)
                .Where(d => family == PFamily.Undefined || family == d.Family)
                .ToArray();
            
            if (!petDescs.Any())
                return null;

            return petDescs.RandomElement(new Random((int) DateTime.Now.Ticks));
        }

        private static int GetRarityMaxLevel(PRarity rarity)
        {
            switch (rarity)
            {
                case PRarity.Divine:
                    return 100;
                case PRarity.Legendary:
                    return 90;
                case PRarity.Rare:
                    return 70;
                case PRarity.Uncommon:
                    return 50;
                case PRarity.Common:
                    return 30;
                default:
                    return 1;
            }
        }

        private static void InitializeAbilities(RealmManager manager, Pet pet, PetDesc petDesc)
        {
            var rand = new Random((int)DateTime.Now.Ticks);

            var abilities = manager.Resources.GameData
                .PetAbilities.Keys.Select(a => (PAbility)a).ToList();
            if (abilities.Count <= 0)
                throw new Exception("Missing pet abilities.");

            var ability = petDesc.FirstAbility;
            if (ability == PAbility.Undefined)
                ability = abilities.RandomElement(rand);

            for (var i = 0; i < DbPet.NumAbilities; i++)
            {
                pet.Ability[i].Initialize(ability, 1);

                if (ability == PAbility.AttackClose || 
                    ability == PAbility.AttackFar || 
                    ability == PAbility.AttackMid)
                {
                    abilities.Remove(PAbility.AttackClose);
                    abilities.Remove(PAbility.AttackFar);
                    abilities.Remove(PAbility.AttackMid);
                }
                else
                    abilities.Remove(ability);

                ability = abilities.RandomElement(rand);
            }
        }

        public bool Feed(int feedPower)
        {
            // don't feed abilities if enabled abilities are maxed
            if (Ability[0].Level >= MaxLevel && Rarity < PRarity.Uncommon ||
                Ability[1].Level >= MaxLevel && Rarity < PRarity.Legendary ||
                Ability[2].Level >= MaxLevel)
                return false;
            
            foreach (var a in Ability)
                a.Feed(feedPower);
            Save();
            return true;
        }

        private void Save()
        {
            if (_dbPet == null)
                return;

            _dbPet.ObjectType = ObjectType;
            _dbPet.Rarity = Rarity;
            _dbPet.MaxLevel = MaxLevel;
            for (var i = 0; i < _dbPet.Ability.Length; i++)
            {
                _dbPet.Ability[i].Type = Ability[i].Type;
                _dbPet.Ability[i].Level = Ability[i].Level;
                _dbPet.Ability[i].Power = Ability[i].Power;
            }
            _dbPet.FlushAsync();
        }

        public int Fuse(Pet pet)
        {
            FuseStats(pet);

            if (Rarity == PRarity.Uncommon || Rarity == PRarity.Legendary)
            {
                Save();
                return (Rarity == PRarity.Uncommon) ? 1 : 2;
            }
            
            // evolve pet
            var petDesc = GetRandomPetDesc(Manager, Rarity, Family);
            if (petDesc != null)
            {
                var gameData = Manager.Resources.GameData;
                var petSkin = gameData.PetSkins[gameData.IdToObjectType[petDesc.DefaultSkin]];

                ObjectType = petDesc.ObjectType;
                Save();

                return petSkin.ObjectType;
            }

            return 0;
        }

        private void FuseStats(Pet pet)
        {
            for (var i = 0; i < Ability.Length; i++)
                Ability[i].Initialize(Ability[i].Type, (Ability[i].Level + pet.Ability[i].Level) / 2);
            MaxLevel = 20 + Ability[0].Level;
            if (MaxLevel > 100) MaxLevel = 100;
            Rarity += 1;
        }
    }

    public class PetAbility
    {
        public PAbility Type { get; private set; }
        public int Level { get; private set; }
        public int Power { get; private set; }

        private readonly Pet _owner;
        private readonly int _abilityId;
        private int _nextPowerGoal;

        public PetAbility(Pet owner, int abilityId)
        {
            _owner = owner;
            _abilityId = abilityId;

            Type = PAbility.Undefined;
            Level = 1;
            Power = 0;

            _nextPowerGoal = Pet.GetNeededPower(Level);
        }

        public PetAbility(Pet owner, DbPet dbPet, int abilityId)
        {
            _owner = owner;
            _abilityId = abilityId;

            var ability = dbPet.Ability[abilityId];
            Type = ability.Type;
            Level = ability.Level;
            Power = ability.Power;

            _nextPowerGoal = Pet.GetNeededPower(Level);
        }

        public void Initialize(PAbility ability, int level)
        {
            Type = ability;
            Level = level;
            Power = Pet.GetNeededPower(level - 1);
            _nextPowerGoal = Pet.GetNeededPower(level);
        }

        public void Feed(int feedPower)
        {
            if (Level >= _owner.MaxLevel)
                return;

            // modify feed power based on ability
            var p = feedPower;
            switch (_abilityId)
            {
                case 0:
                    break;
                case 1: 
                    p = (int)(p * .65f);
                    break;
                case 2: 
                    p = (int)(p * .30f);
                    break;
                default:
                    p = (int)(p * .10f);
                    break;
            }

            // power up
            Power += p;
            while (Power >= _nextPowerGoal)
            {
                Level++;

                if (Level >= _owner.MaxLevel)
                    Power = _nextPowerGoal;

                _nextPowerGoal = Pet.GetNeededPower(Level);
            }
        }
    }
}
