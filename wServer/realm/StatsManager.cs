using common.resources;
using log4net;
using wServer.realm.entities;

namespace wServer.realm
{
    public class StatsManager
    {
        //static readonly ILog Log = LogManager.GetLogger(typeof(StatsManager));

        internal const int NumStatTypes = 11;
        private const float MinAttackMult = 0.5f;
        private const float MaxAttackMult = 2f;
        private const float MinAttackFreq = 0.0015f;
        private const float MaxAttackFreq = 0.008f;

        internal readonly Player Owner;
        internal readonly BaseStatManager Base;
        internal readonly BoostStatManager Boost;

        private readonly SV<int>[] _stats;

        public int this[int index] => Base[index] + Boost[index];

        public StatsManager(Player owner)
        {
            Owner = owner;
            Base = new BaseStatManager(this);
            Boost = new BoostStatManager(this);

            _stats = new SV<int>[NumStatTypes];
            for (var i = 0; i < NumStatTypes; i++)
                _stats[i] = new SV<int>(Owner, GetStatType(i), this[i], i != 0 && i!= 1); // make maxHP and maxMP global update
        }
        
        public void ReCalculateValues(InventoryChangedEventArgs e = null)
        {
            Base.ReCalculateValues(e);
            Boost.ReCalculateValues(e);

            for (var i = 0; i < _stats.Length; i++)
                _stats[i].SetValue(this[i]);
        }

        internal void StatChanged(int index)
        {
            _stats[index].SetValue(this[index]);
        }

        public int GetAttackDamage(int min, int max, bool isAbility = false)
        {
            var ret = Owner.Client.Random.NextIntRange((uint)min, (uint)max) * GetAttackMult(isAbility);
            //Log.Info($"Dmg: {ret}");
            return (int)ret;
        } 

        public float GetAttackMult(bool isAbility)
        {
            if (isAbility)
                return 1;

            if (Owner.HasConditionEffect(ConditionEffects.Weak))
                return MinAttackMult;

            var mult = MinAttackMult + (this[2] / 75f) * (MaxAttackMult - MinAttackMult);
            if (Owner.HasConditionEffect(ConditionEffects.Damaging))
                mult *= 1.5f;

            return mult;
        }

        public float GetAttackFrequency()
        {
            if (Owner.HasConditionEffect(ConditionEffects.Dazed))
                return MinAttackFreq;

            var rof = MinAttackFreq + (this[5] / 75f) * (MaxAttackFreq - MinAttackFreq);

            if (Owner.HasConditionEffect(ConditionEffects.Berserk))
                rof *= 1.5f;

            return rof;
        }

        public static float GetDefenseDamage(Entity host, int dmg, int def)
        {
            if (host.HasConditionEffect(ConditionEffects.Armored))
                def *= 2;
            if (host.HasConditionEffect(ConditionEffects.ArmorBroken))
                def = 0;

            float limit = dmg * 0.25f;//0.15f;

            float ret;
            if (dmg - def < limit) ret = limit;
            else ret = dmg - def;

            if (host.HasConditionEffect(ConditionEffects.Curse))
                ret = (int)(ret * 1.20);

            if (host.HasConditionEffect(ConditionEffects.Invulnerable) ||
                host.HasConditionEffect(ConditionEffects.Invincible))
                ret = 0;
            return ret;
        }
        
        public float GetDefenseDamage(int dmg, bool noDef)
        {
            var def = this[3];
            if (Owner.HasConditionEffect(ConditionEffects.Armored))
                def *= 2;
            if (Owner.HasConditionEffect(ConditionEffects.ArmorBroken) || noDef)
                def = 0;

            float limit = dmg * 0.25f;//0.15f;

            float ret;
            if (dmg - def < limit) ret = limit;
            else ret = dmg - def;

            if (Owner.HasConditionEffect(ConditionEffects.Petrify))
                ret = (int)(ret * .9);
            if (Owner.HasConditionEffect(ConditionEffects.Curse))
                ret = (int)(ret * 1.20);
            if (Owner.HasConditionEffect(ConditionEffects.Invulnerable) ||
                Owner.HasConditionEffect(ConditionEffects.Invincible))
                ret = 0;
            return ret;
        }

        public static float GetSpeed(Entity entity, float stat)
        {
            var ret = 4 + 5.6f * (stat / 75f);
            if (entity.HasConditionEffect(ConditionEffects.Speedy))
                ret *= 1.5f;
            if (entity.HasConditionEffect(ConditionEffects.Slowed))
                ret = 4;
            if (entity.HasConditionEffect(ConditionEffects.Paralyzed))
                ret = 0;
            return ret;
        }

        public float GetSpeed()
        {
            return GetSpeed(Owner, this[4]);
        }

        public float GetHPRegen()
        {
            var vit = this[6];
            if (Owner.HasConditionEffect(ConditionEffects.Sick))
                vit = 0;
            return 6 + vit * .12f;
        }

        public float GetMPRegen()
        {
            if (Owner.HasConditionEffect(ConditionEffects.Quiet))
                return 0;
            return 0.5f + this[7] * .06f;
        }

        /*public float Dex()
        {
            var dex = this[5];
            if (Owner.HasConditionEffect(ConditionEffects.Dazed))
                dex = 0;

            var ret = 1.5f + 6.5f * (dex / 75f);
            if (Owner.HasConditionEffect(ConditionEffects.Berserk))
                ret *= 1.5f;
            if (Owner.HasConditionEffect(ConditionEffects.Stunned))
                ret = 0;
            return ret;
        }*/

        public static string StatIndexToName(int index)
        {
            switch (index)
            {
                case 0: return "MaxHitPoints";
                case 1: return "MaxMagicPoints";
                case 2: return "Attack";
                case 3: return "Defense";
                case 4: return "Speed";
                case 5: return "Dexterity";
                case 6: return "HpRegen";
                case 7: return "MpRegen";
                case 8: return "DamageMin";
                case 9: return "DamageMax";
                case 10: return "LuckBoost";
            } return null;
        }

        public static int GetStatIndex(string name)
        {
            switch (name)
            {
                case "MaxHitPoints": return 0;
                case "MaxMagicPoints": return 1;
                case "Attack": return 2;
                case "Defense": return 3;
                case "Speed": return 4;
                case "Dexterity": return 5;
                case "HpRegen": return 6;
                case "MpRegen": return 7;
                case "DamageMin": return 8;
                case "DamageMax": return 9;
                case "LuckBoost": return 10;
            } return -1;
        }

        public static int GetStatIndex(StatsType stat)
        {
            switch (stat)
            {
                case StatsType.MaximumHP:
                    return 0;
                case StatsType.MaximumMP:
                    return 1;
                case StatsType.Attack:
                    return 2;
                case StatsType.Defense:
                    return 3;
                case StatsType.Speed:
                    return 4;
                case StatsType.Dexterity:
                    return 5;
                case StatsType.Vitality:
                    return 6;
                case StatsType.Wisdom:
                    return 7;
                case StatsType.DamageMin:
                    return 8;
                case StatsType.DamageMax:
                    return 9;
                case StatsType.Luck:
                    return 10;
                default:
                    return -1;
            }
        }

        public static StatsType GetStatType(int stat)
        {
            switch (stat)
            {
                case 0:
                    return StatsType.MaximumHP;
                case 1:
                    return StatsType.MaximumMP;
                case 2:
                    return StatsType.Attack;
                case 3:
                    return StatsType.Defense;
                case 4:
                    return StatsType.Speed;
                case 5:
                    return StatsType.Dexterity;
                case 6:
                    return StatsType.Vitality;
                case 7:
                    return StatsType.Wisdom;
                case 8:
                    return StatsType.DamageMin;
                case 9:
                    return StatsType.DamageMax;
                case 10:
                    return StatsType.Luck;
                default:
                    return StatsType.None;
            }
        }

        public static StatsType GetBoostStatType(int stat)
        {
            switch (stat)
            {
                case 0:
                    return StatsType.HPBoost;
                case 1:
                    return StatsType.MPBoost;
                case 2:
                    return StatsType.AttackBonus;
                case 3:
                    return StatsType.DefenseBonus;
                case 4:
                    return StatsType.SpeedBonus;
                case 5:
                    return StatsType.DexterityBonus;
                case 6:
                    return StatsType.VitalityBonus;
                case 7:
                    return StatsType.WisdomBonus;
                case 8:
                    return StatsType.DamageMinBonus;
                case 9:
                    return StatsType.DamageMaxBonus;
                case 10:
                    return StatsType.LuckBonus;
                default:
                    return StatsType.None;
            }
        }
    }
}
