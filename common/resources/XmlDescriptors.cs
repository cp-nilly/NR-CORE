using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace common.resources
{
    public enum ItemType
    {
        Weapon,
        Ability,
        Armor,
        Ring,
        Potion,
        StatPot,
        Other,
        None
    }

    public enum CurrencyType
    {
        Gold = 0,
        Fame = 1,
        GuildFame = 2,
        Tokens = 3
    }

    [Flags]
    public enum ConditionEffects : ulong
    {
        Dead =              1 << 0,
        Quiet =             1 << 1,
        Weak =              1 << 2,
        Slowed =            1 << 3,
        Sick =              1 << 4,
        Dazed =             1 << 5,
        Stunned =           1 << 6,
        Blind =             1 << 7,
        Hallucinating =     1 << 8,
        Drunk =             1 << 9,
        Confused =          1 << 10,
        StunImmume =        1 << 11,
        Invisible =         1 << 12,
        Paralyzed =         1 << 13,
        Speedy =            1 << 14,
        Bleeding =          1 << 15,
        ArmorBreakImmune =  1 << 16,
        Healing =           1 << 17,
        Damaging =          1 << 18,
        Berserk =           1 << 19,
        Paused =            1 << 20,
        Stasis =            1 << 21,
        StasisImmune =      1 << 22,
        Invincible =        1 << 23,
        Invulnerable =      1 << 24,
        Armored =           1 << 25,
        ArmorBroken =       1 << 26,
        Hexed =             1 << 27,
        NinjaSpeedy =       1 << 28,
        Unstable =          1 << 29,
        Darkness =          1 << 30,
        SlowedImmune =      (ulong) 1 << 31,
        DazedImmune =       (ulong) 1 << 32,
        ParalyzeImmune =    (ulong) 1 << 33,
        Petrify =           (ulong) 1 << 34,
        PetrifyImmune =     (ulong) 1 << 35,
        PetDisable =        (ulong) 1 << 36,
        Curse =             (ulong) 1 << 37,
        CurseImmune =       (ulong) 1 << 38,
        HPBoost =           (ulong) 1 << 39,
        MPBoost =           (ulong) 1 << 40,
        AttBoost =          (ulong) 1 << 41,
        DefBoost =          (ulong) 1 << 42,
        SpdBoost =          (ulong) 1 << 43,
        DexBoost =          (ulong) 1 << 44,
        VitBoost =          (ulong) 1 << 45,
        WisBoost =          (ulong) 1 << 46,
        Hidden =            (ulong) 1 << 47,
        Muted =             (ulong) 1 << 48,
        PartyVision =       (ulong) 1 << 49,
        XMasVision =        (ulong) 1 << 50
    }

    public enum ConditionEffectIndex
    {
        Dead = 0,
        Quiet = 1,
        Weak = 2,
        Slowed = 3,
        Sick = 4,
        Dazed = 5,
        Stunned = 6,
        Blind = 7,
        Hallucinating = 8,
        Drunk = 9,
        Confused = 10,
        StunImmune = 11,
        Invisible = 12,
        Paralyzed = 13,
        Speedy = 14,
        Bleeding = 15,
        ArmorBreakImmune = 16,
        Healing = 17,
        Damaging = 18,
        Berserk = 19,
        Paused = 20,
        Stasis = 21,
        StasisImmune = 22,
        Invincible = 23,
        Invulnerable = 24,
        Armored = 25,
        ArmorBroken = 26,
        Hexed = 27,
        NinjaSpeedy = 28,
        Unstable = 29,
        Darkness = 30,
        SlowedImmune = 31,
        DazedImmune = 32,
        ParalyzeImmune = 33,
        Petrify = 34,
        PetrifyImmune = 35,
        PetDisable = 36,
        Curse = 37,
        CurseImmune = 38,
        HPBoost = 39,
        MPBoost = 40,
        AttBoost = 41,
        DefBoost = 42,
        SpdBoost = 43,
        DexBoost = 44,
        VitBoost = 45,
        WisBoost = 46,
        Hidden = 47,
        Muted = 48,
        PartyVision = 49,
        XMasVision = 50
    }

    public class ConditionEffect
    {
        public ConditionEffectIndex Effect { get; set; }
        public int DurationMS { get; set; }
        public float Range { get; set; }
        public ConditionEffect() { }
        public ConditionEffect(XElement elem)
        {
            Effect = (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), elem.Value.Replace(" ", ""));
            if (elem.Attribute("duration") != null)
                DurationMS = (int)(float.Parse(elem.Attribute("duration").Value) * 1000);
            if (elem.Attribute("range") != null)
                Range = float.Parse(elem.Attribute("range").Value);
        }
    }

    public class ProjectileDesc
    {
        public int BulletType { get; private set; }
        public string ObjectId { get; private set; }
        public int LifetimeMS { get; private set; }
        public float Speed { get; private set; }
        public int Size { get; private set; }
        public int MinDamage { get; private set; }
        public int MaxDamage { get; private set; }

        public ConditionEffect[] Effects { get; private set; }

        public bool MultiHit { get; private set; }
        public bool PassesCover { get; private set; }
        public bool ArmorPiercing { get; private set; }
        public bool ParticleTrail { get; private set; }
        public bool Wavy { get; private set; }
        public bool Parametric { get; private set; }
        public bool Boomerang { get; private set; }

        public float Amplitude { get; private set; }
        public float Frequency { get; private set; }
        public float Magnitude { get; private set; }

        public ProjectileDesc(XElement elem)
        {
            XElement n;
            if (elem.Attribute("id") != null)
                BulletType = Utils.FromString(elem.Attribute("id").Value);
            ObjectId = elem.Element("ObjectId").Value;
            LifetimeMS = Utils.FromString(elem.Element("LifetimeMS")?.Value ?? "0");
            Speed = float.Parse(elem.Element("Speed").Value);
            if ((n = elem.Element("Size")) != null)
                Size = Utils.FromString(n.Value);

            var dmg = elem.Element("Damage");
            if (dmg != null)
                MinDamage = MaxDamage = Utils.FromString(dmg.Value);
            else
            {
                MinDamage = Utils.FromString(elem.Element("MinDamage").Value);
                MaxDamage = Utils.FromString(elem.Element("MaxDamage").Value);
            }

            List<ConditionEffect> effects = new List<ConditionEffect>();
            foreach (XElement i in elem.Elements("ConditionEffect"))
                effects.Add(new ConditionEffect(i));
            Effects = effects.ToArray();

            MultiHit = elem.Element("MultiHit") != null;
            PassesCover = elem.Element("PassesCover") != null;
            ArmorPiercing = elem.Element("ArmorPiercing") != null;
            ParticleTrail = elem.Element("ParticleTrail") != null;
            Wavy = elem.Element("Wavy") != null;
            Parametric = elem.Element("Parametric") != null;
            Boomerang = elem.Element("Boomerang") != null;

            n = elem.Element("Amplitude");
            if (n != null)
                Amplitude = float.Parse(n.Value);
            else
                Amplitude = 0;
            n = elem.Element("Frequency");
            if (n != null)
                Frequency = float.Parse(n.Value);
            else
                Frequency = 1;
            n = elem.Element("Magnitude");
            if (n != null)
                Magnitude = float.Parse(n.Value);
            else
                Magnitude = 3;
        }
    }

    public enum ActivateEffects
    {
        Shoot,
        DualShoot,
        StatBoostSelf,
        StatBoostAura,
        BulletNova,
        ConditionEffectAura,
        ConditionEffectSelf,
        Heal,
        HealNova,
        Magic,
        MagicNova,
        Teleport,
        VampireBlast,
        Trap,
        StasisBlast,
        Decoy,
        Lightning,
        PoisonGrenade,
        RemoveNegativeConditions,
        RemoveNegativeConditionsSelf,
        IncrementStat,
        Drake,
        PermaPet,
        Create,
        DazeBlast,
        ClearConditionEffectAura,
        ClearConditionEffectSelf,
        Dye,
        ShurikenAbility,
        TomeDamage,
        MultiDecoy,
        Mushroom,
        PearlAbility,
        BuildTower,
        MonsterToss,
        PartyAOE,
        MiniPot,
        Halo,
        Fame,
        SamuraiAbility,
        Summon,
        ChristmasPopper,
        Belt,
        Totem,
        UnlockPortal,
        CreatePet,
        Pet,
        UnlockSkin,
        GenericActivate,
        MysteryPortal,
        ChangeSkin,
        FixedStat,
        Backpack,
        XPBoost,
        LDBoost,
        LTBoost,
        UnlockEmote,
        HealingGrenade,
        PetSkin,
        Unlock,
        MysteryDyes
    }

    public class ActivateEffect
    {
        public ActivateEffects Effect { get; private set; }
        public int Stats { get; private set; }
        public int Amount { get; private set; }
        public float Range { get; private set; }
        public float DurationSec { get; private set; }
        public int DurationMS { get; private set; }
        public int DurationMS2 { get; private set; }
        public ConditionEffectIndex? ConditionEffect { get; private set; }
        public ConditionEffectIndex? CheckExistingEffect { get; private set; }
        public float EffectDuration { get; private set; }
        public int MaximumDistance { get; private set; }
        public float Radius { get; private set; }
        public int TotalDamage { get; private set; }
        public string ObjectId { get; private set; }
        public string ObjectId2 { get; private set; }
        public int AngleOffset { get; private set; }
        public int MaxTargets { get; private set; }
        public string Id { get; private set; }
        public string DungeonName { get; private set; }
        public string LockedName { get; private set; }
        public uint Color { get; private set; }
        public ushort SkinType { get; private set; }
        public int Size { get; private set; }
        public bool NoStack { get; private set; }
        public bool UseWisMod { get; private set; }
        public string Target { get; private set; }
        public string Center { get; private set; }
        public int VisualEffect { get; private set; }

        public ActivateEffect(XElement elem)
        {
            Effect = (ActivateEffects)Enum.Parse(typeof(ActivateEffects), elem.Value);
            if (elem.Attribute("stat") != null)
                Stats = Utils.FromString(elem.Attribute("stat").Value);

            if (elem.Attribute("amount") != null)
                Amount = Utils.FromString(elem.Attribute("amount").Value);

            if (elem.Attribute("range") != null)
                Range = float.Parse(elem.Attribute("range").Value);
            if (elem.Attribute("duration") != null)
            {
                DurationSec = float.Parse(elem.Attribute("duration").Value);
                DurationMS = (int) (DurationSec * 1000);
            }
            if (elem.Attribute("duration2") != null)
                DurationMS = (int)(float.Parse(elem.Attribute("duration2").Value) * 1000);

            if (elem.Attribute("effect") != null)
            {
                ConditionEffectIndex condEff;
                if (Enum.TryParse(elem.Attribute("effect").Value, true, out condEff))
                    ConditionEffect = condEff;
            }
            if (elem.Attribute("checkExistingEffect") != null)
            {
                ConditionEffectIndex condEff;
                if (Enum.TryParse(elem.Attribute("checkExistingEffect").Value, true, out condEff))
                    CheckExistingEffect = condEff;
            }
            if (elem.Attribute("condEffect") != null)
            {
                ConditionEffectIndex condEff;
                if (Enum.TryParse(elem.Attribute("condEffect").Value, true, out condEff))
                    ConditionEffect = condEff;
            }

            if (elem.Attribute("condDuration") != null)
                EffectDuration = float.Parse(elem.Attribute("condDuration").Value);

            if (elem.Attribute("maxDistance") != null)
                MaximumDistance = Utils.FromString(elem.Attribute("maxDistance").Value);

            if (elem.Attribute("radius") != null)
                Radius = float.Parse(elem.Attribute("radius").Value);

            if (elem.Attribute("totalDamage") != null)
                TotalDamage = Utils.FromString(elem.Attribute("totalDamage").Value);

            if (elem.Attribute("objectId") != null)
                ObjectId = elem.Attribute("objectId").Value;
            if (elem.Attribute("objectId2") != null)
                ObjectId = elem.Attribute("objectId2").Value;

            if (elem.Attribute("angleOffset") != null)
                AngleOffset = Utils.FromString(elem.Attribute("angleOffset").Value);

            if (elem.Attribute("maxTargets") != null)
                MaxTargets = Utils.FromString(elem.Attribute("maxTargets").Value);

            if (elem.Attribute("id") != null)
                Id = elem.Attribute("id").Value;

            if (elem.Attribute("dungeonName") != null)
                DungeonName = elem.Attribute("dungeonName").Value;

            if (elem.Attribute("lockedName") != null)
                LockedName = elem.Attribute("lockedName").Value;

            if (elem.Attribute("color") != null)
                Color = uint.Parse(elem.Attribute("color").Value.Substring(2), NumberStyles.AllowHexSpecifier);

            if (elem.Attribute("skinType") != null)
                SkinType = ushort.Parse(elem.Attribute("skinType").Value.Substring(2), NumberStyles.AllowHexSpecifier);

            if (elem.Attribute("size") != null)
                Size = int.Parse(elem.Attribute("size").Value);

            if (elem.Attribute("noStack") != null)
                NoStack = elem.Attribute("noStack").Value.Equals("true");

            if (elem.Attribute("useWisMod") != null)
                UseWisMod = elem.Attribute("useWisMod").Value.Equals("true");

            if (elem.Attribute("target") != null)
                Target = elem.Attribute("target").Value;

            if (elem.Attribute("center") != null)
                Center = elem.Attribute("center").Value;

            if (elem.Attribute("visualEffect") != null)
                VisualEffect = Utils.FromString(elem.Attribute("visualEffect").Value);
        }
    }
    public class Setpiece
    {
        public string Type { get; private set; }
        public ushort Slot { get; private set; }
        public ushort ItemType { get; private set; }

        public Setpiece(XElement elem)
        {
            Type = elem.Value;
            Slot = ushort.Parse(elem.Attribute("slot").Value);
            ItemType = ushort.Parse(elem.Attribute("itemtype").Value.Substring(2), NumberStyles.AllowHexSpecifier);
        }
    }
    public class PortalDesc : ObjectDesc
    {
        public int Timeout { get; private set; }
        public bool NexusPortal { get; private set; }
        public bool Locked { get; private set; }

        public PortalDesc(ushort type, XElement elem) : base(type, elem)
        {
            XElement n;

            if (elem.Element("NexusPortal") != null)
                NexusPortal = true;

            Timeout = (n = elem.Element("Timeout")) != null ? 
                int.Parse(n.Value) : 30;

            Locked = elem.Element("LockedPortal") != null;
        }
    }
    public class Item
    {
        public ushort ObjectType { get; private set; }
        public string ObjectId { get; private set; }
        public int SlotType { get; private set; }
        public int Tier { get; private set; }
        public string Description { get; private set; }
        public float RateOfFire { get; private set; }
        public bool Usable { get; private set; }
        public int BagType { get; private set; }
        public int MpCost { get; private set; }
        public int MpEndCost { get; private set; }
        public int FameBonus { get; private set; }
        public int NumProjectiles { get; private set; }
        public float ArcGap { get; private set; }
        public float ArcGap1 { get; private set; }
        public float ArcGap2 { get; private set; }
        public int NumProjectiles1 { get; private set; }
        public int NumProjectiles2 { get; private set; }
        public bool DualShooting { get; private set; }
        public bool Consumable { get; private set; }
        public bool Potion { get; private set; }
        public string DisplayId { get; private set; }
        public string DisplayName { get; private set; }
        public string SuccessorId { get; private set; }
        public bool Soulbound { get; private set; }
        public bool Undead { get; private set; }
        public bool PUndead { get; private set; }
        public bool SUndead { get; private set; }
        public float Cooldown { get; private set; }
        public bool Resurrects { get; private set; }
        public int Texture1 { get; private set; }
        public int Texture2 { get; private set; }
        public bool Secret { get; private set; }
        public int Doses { get; private set; }
        public int FeedPower { get; private set; }
        public PFamily Family { get; private set; }
        public PRarity Rarity { get; private set; }

        public KeyValuePair<int, int>[] StatsBoost { get; private set; }
        public ActivateEffect[] ActivateEffects { get; private set; }
        public ProjectileDesc[] Projectiles { get; private set; }

        public Item(ushort type, XElement elem)
        {
            XElement n;

            ObjectType = type;

            ObjectId = elem.Attribute(XName.Get("id")).Value;

            SlotType = Utils.FromString(elem.Element("SlotType").Value);

            if ((n = elem.Element("Tier")) != null)
                try
                {
                    Tier = Utils.FromString(n.Value);
                }
                catch
                {
                    Tier = -1;
                }
            else
                Tier = -1;

            Description = elem.Element("Description").Value;

            if ((n = elem.Element("RateOfFire")) != null)
                RateOfFire = float.Parse(n.Value);
            else
                RateOfFire = 1;

            Usable = elem.Element("Usable") != null;

            if ((n = elem.Element("BagType")) != null)
                BagType = Utils.FromString(n.Value);
            else
                BagType = 0;

            if ((n = elem.Element("MpCost")) != null)
                MpCost = Utils.FromString(n.Value);
            else
                MpCost = 0;

            if ((n = elem.Element("MpEndCost")) != null)
                MpEndCost = Utils.FromString(n.Value);
            else
                MpEndCost = 0;

            if ((n = elem.Element("FameBonus")) != null)
                FameBonus = Utils.FromString(n.Value);
            else
                FameBonus = 0;

            if ((n = elem.Element("NumProjectiles")) != null)
                NumProjectiles = Utils.FromString(n.Value);
            else
                NumProjectiles = 1;

            if ((n = elem.Element("ArcGap")) != null)
                ArcGap = Utils.FromString(n.Value);
            else
                ArcGap = 11.25f;

            if ((n = elem.Element("ArcGap1")) != null)
                ArcGap1 = Utils.FromString(n.Value);
            else
                ArcGap1 = 11.25f;

            if ((n = elem.Element("ArcGap2")) != null)
                ArcGap2 = Utils.FromString(n.Value);
            else
                ArcGap2 = 11.25f;

            if ((n = elem.Element("NumProjectiles1")) != null)
                NumProjectiles1 = Utils.FromString(n.Value);
            else
                NumProjectiles1 = 1;

            if ((n = elem.Element("NumProjectiles2")) != null)
                NumProjectiles2 = Utils.FromString(n.Value);
            else
                NumProjectiles2 = 1;

            DualShooting = elem.Element("DualShooting") != null;

            Consumable = elem.Element("Consumable") != null;

            Potion = elem.Element("Potion") != null;

            if ((n = elem.Element("DisplayId")) != null)
                DisplayId = n.Value;
            else
                DisplayId = null;

            DisplayName = GetDisplayName();

            if ((n = elem.Element("Doses")) != null)
                Doses = Utils.FromString(n.Value);
            else
                Doses = 0;

            if ((n = elem.Element("feedPower")) != null)
                FeedPower = Utils.FromString(n.Value);
            else
                FeedPower = 0;

            Family = PetDesc.GetFamily(elem.Element("PetFamily"));
            Rarity = PetDesc.GetRarity(elem.Element("Rarity"));

            if ((n = elem.Element("SuccessorId")) != null)
                SuccessorId = n.Value;
            else
                SuccessorId = null;

            Soulbound = elem.Element("Soulbound") != null;

            Undead = elem.Element("Undead") != null;

            PUndead = elem.Element("PUndead") != null;

            SUndead = elem.Element("SUndead") != null;

            Secret = elem.Element("Secret") != null;

            if ((n = elem.Element("Cooldown")) != null)
                Cooldown = float.Parse(n.Value);
            else
                Cooldown = 0;

            Resurrects = elem.Element("Resurrects") != null;

            if ((n = elem.Element("Tex1")) != null)
                Texture1 = Convert.ToInt32(n.Value, 16);
            else
                Texture1 = 0;

            if ((n = elem.Element("Tex2")) != null)
                Texture2 = Convert.ToInt32(n.Value, 16);
            else
                Texture2 = 0;

            var stats = new List<KeyValuePair<int, int>>();
            foreach (XElement i in elem.Elements("ActivateOnEquip"))
                stats.Add(new KeyValuePair<int, int>(
                    int.Parse(i.Attribute("stat").Value), 
                    int.Parse(i.Attribute("amount").Value)));
            StatsBoost = stats.ToArray();

            var activate = new List<ActivateEffect>();
            foreach (XElement i in elem.Elements("Activate"))
                activate.Add(new ActivateEffect(i));
            ActivateEffects = activate.ToArray();

            var prj = new List<ProjectileDesc>();
            foreach (XElement i in elem.Elements("Projectile"))
                prj.Add(new ProjectileDesc(i));
            Projectiles = prj.ToArray();
        }

        public override string ToString()
        {
            return ObjectId;
        }


        // this gets the item name players can see in their client
        private string GetDisplayName()
        {
            if (DisplayId == null)
            {
                return ObjectId;
            }
            else
            {
                if (DisplayId[0].Equals('{'))
                {
                    return ObjectId;
                }
                else
                {
                    return DisplayId;
                }
            }
        }
    }
    public class EquipmentSetDesc
    {
        public ushort Type { get; private set; }
        public string Id { get; private set; }

        public ActivateEffect[] ActivateOnEquipAll { get; private set; }
        public Setpiece[] Setpieces { get; private set; }

        public static EquipmentSetDesc FromElem(ushort type, XElement setElem, out ushort skinType)
        {
            skinType = 0;

            var activate = new List<ActivateEffect>();
            foreach (XElement i in setElem.Elements("ActivateOnEquipAll"))
            {
                var ae = new ActivateEffect(i);
                activate.Add(ae);

                if (ae.SkinType != 0)
                    skinType = ae.SkinType;
            }
                
            var setpiece = new List<Setpiece>();
            foreach (XElement i in setElem.Elements("Setpiece"))
                setpiece.Add(new Setpiece(i));

            var eqSet = new EquipmentSetDesc();
            eqSet.Type = type;
            eqSet.Id = setElem.Attribute("id").Value;
            eqSet.ActivateOnEquipAll = activate.ToArray();
            eqSet.Setpieces = setpiece.ToArray();

            return eqSet;
        }
    }
    public class SkinDesc
    {
        public ushort Type { get; private set; }
        public ObjectDesc ObjDesc { get; private set; }

        public ushort PlayerClassType { get; private set; }
        public ushort UnlockLevel { get; private set; }
        public bool Restricted { get; private set; }
        public bool Expires { get; private set; }
        public int Cost { get; private set; }
        public bool UnlockSpecial { get; private set; }
        public bool NoSkinSelect { get; private set; }
        public int Size { get; private set; }
        public string PlayerExclusive { get; private set; }

        public static SkinDesc FromElem(ushort type, XElement skinElem)
        {
            var pct = skinElem.Element("PlayerClassType");
            if (pct == null) return null;

            var sd = new SkinDesc();
            sd.Type = type;
            sd.ObjDesc = new ObjectDesc(type, skinElem);
            sd.PlayerClassType = (ushort)Utils.FromString(pct.Value);
            sd.Restricted = skinElem.Element("Restricted") != null;
            sd.Expires = skinElem.Element("Expires") != null;
            sd.UnlockSpecial = skinElem.Element("UnlockSpecial") != null;
            sd.NoSkinSelect = skinElem.Element("NoSkinSelect") != null;
            sd.PlayerExclusive = skinElem.Element("PlayerExclusive") == null ? 
                null : skinElem.Element("PlayerExclusive").Value;

            var ul = skinElem.Element("UnlockLevel");
            if (ul != null) sd.UnlockLevel = ushort.Parse(ul.Value);
            var cost = skinElem.Element("Cost");
            sd.Cost = cost != null ? int.Parse(cost.Value) : 1000;

            var size = skinElem.Attribute("size");
            if (size != null)
                sd.Size = Utils.FromString(size.Value);
            else
                sd.Size = 100;

            return sd;
        }
    }
    public class SpawnCount
    {
        public int Mean { get; private set; }
        public int StdDev { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }

        public SpawnCount(XElement elem)
        {
            Mean = Utils.FromString(elem.Element("Mean").Value);
            StdDev = Utils.FromString(elem.Element("StdDev").Value);
            Min = Utils.FromString(elem.Element("Min").Value);
            Max = Utils.FromString(elem.Element("Max").Value);
        }
    }
    public class UnlockClass
    {
        public ushort? Type { get; private set; }
        public ushort? Level { get; private set; }
        public uint? Cost { get; private set; }

        public UnlockClass(XElement elem)
        {
            XElement n;
            if ((n = elem.Element("UnlockLevel")) != null &&
                n.Attribute("type") != null &&
                n.Attribute("level") != null)
            {
                Type = (ushort) Utils.FromString(n.Attribute("type").Value);
                Level = (ushort) Utils.FromString(n.Attribute("level").Value);
            }
            if ((n = elem.Element("UnlockCost")) != null)
                Cost = (uint) Utils.FromString(n.Value);
        }
    }
    public class Stat
    {
        public string Type { get; private set; }
        public int MaxValue { get; private set; }
        public int StartingValue { get; private set; }
        public int MinIncrease { get; private set; }
        public int MaxIncrease { get; private set; }

        public Stat(int index, XElement elem)
        {
            Type = StatIndexToName(index);

            var x = elem.Element(Type);
            if (x != null)
            {
                StartingValue = int.Parse(x.Value);
                MaxValue = int.Parse(x.Attribute("max").Value);
            }
            
            var y = elem.Elements("LevelIncrease");
            foreach (var s in y)
                if (s.Value == Type)
                {
                    MinIncrease = int.Parse(s.Attribute("min").Value);
                    MaxIncrease = int.Parse(s.Attribute("max").Value);
                    break;
                }
        }

        private static string StatIndexToName(int index)
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
            } return null;
        }
    }
    public class PlayerDesc : ObjectDesc
    {
        public int[] SlotTypes { get; private set; }
        public ushort[] Equipment { get; private set; }
        public Stat[] Stats { get; private set; }
        public UnlockClass Unlock { get; private set; }

        public PlayerDesc(ushort type, XElement elem) : base(type, elem)
        {
            XElement n;
            SlotTypes = elem.Element("SlotTypes").Value.CommaToArray<int>();
            Equipment = elem.Element("Equipment").Value.CommaToArray<ushort>();
            Stats = new Stat[8];
            for (var i = 0; i < Stats.Length; i++)
                Stats[i] = new Stat(i, elem);
            if (elem.Element("UnlockLevel") != null ||
                elem.Element("UnlockCost") != null)
                Unlock = new UnlockClass(elem);
        }
    }
    public class ObjectDesc
    {
        public ushort ObjectType { get; private set; }
        public string ObjectId { get; private set; }
        public string DisplayId { get; private set; }
        public string DungeonName { get; private set; }
        public string Group { get; private set; }
        public string Class { get; private set; }
        public bool Character { get; private set; }
        public bool Player { get; private set; }
        public bool Enemy { get; private set; }
        public bool OccupySquare { get; private set; }
        public bool FullOccupy { get; private set; }
        public bool EnemyOccupySquare { get; private set; }
        public bool Static { get; private set; }
        public bool BlocksSight { get; private set; }
        public bool NoMiniMap { get; private set; }
        public bool ProtectFromGroundDamage { get; private set; }
        public bool ProtectFromSink { get; private set; }
        public bool Flying { get; private set; }
        public bool ShowName { get; private set; }
        public bool DontFaceAttacks { get; private set; }
        public int MinSize { get; private set; }
        public int MaxSize { get; private set; }
        public int SizeStep { get; private set; }
        public TagList Tags { get; private set; }
        public ProjectileDesc[] Projectiles { get; private set; }
        public int MaxHP { get; private set; }
        public int Defense { get; private set; }
        public string Terrain { get; private set; }
        public float SpawnProbability { get; private set; }
        public SpawnCount Spawn { get; private set; }
        public bool Cube { get; private set; }
        public bool God { get; private set; }
        public bool Quest { get; private set; }
        public int? Level { get; private set; }
        public bool ArmorBreakImmune { get; private set; }
        public bool CurseImmune { get; private set; }
        public bool DazedImmune { get; private set; }
        public bool ParalyzeImmune { get; private set; }
        public bool PetrifyImmune { get; private set; }
        public bool SlowedImmune { get; private set; }
        public bool StasisImmune { get; private set; }
        public bool StunImmune { get; private set; }
        public bool Oryx { get; private set; }
        public bool Hero { get; private set; }
        public int? PerRealmMax { get; private set; }
        public float? ExpMultiplier { get; private set; }    //Exp gained = level total / 10 * multi
        public bool Restricted { get; private set; }
        public bool IsPet { get; private set; }
        public bool IsPetSkin { get; private set; }
        public bool IsPetProjectile { get; private set; }
        public bool IsPetBehavior { get; private set; }
        public bool IsPetAbility { get; private set; }
        public bool Connects { get; private set; }
        public bool TrollWhiteBag { get; private set; }

        public ObjectDesc(ushort type, XElement elem)
        {
            XElement n;
            ObjectType = type;
            ObjectId = elem.Attribute("id").Value;
            Class = elem.Element("Class").Value;
            Group = (n = elem.Element("Group")) != null ? n.Value : null;
            DisplayId = (n = elem.Element("DisplayId")) != null ? n.Value : null;
            DungeonName = (n = elem.Element("DungeonName")) != null ? n.Value : DisplayId;
            Character = Class.Equals("Character");
            Player = elem.Element("Player") != null;
            Enemy = elem.Element("Enemy") != null;
            OccupySquare = elem.Element("OccupySquare") != null;
            FullOccupy = elem.Element("FullOccupy") != null;
            EnemyOccupySquare = elem.Element("EnemyOccupySquare") != null;
            Static = elem.Element("Static") != null;
            BlocksSight = elem.Element("BlocksSight") != null;
            NoMiniMap = elem.Element("NoMiniMap") != null;
            ProtectFromGroundDamage = elem.Element("ProtectFromGroundDamage") != null;
            ProtectFromSink = elem.Element("ProtectFromSink") != null;
            Flying = elem.Element("Flying") != null;
            ShowName = elem.Element("ShowName") != null;
            DontFaceAttacks = elem.Element("DontFaceAttacks") != null;

            if (elem.Element("Restricted") != null)
                Restricted = true;

            if ((n = elem.Element("Size")) != null)
            {
                MinSize = MaxSize = Utils.FromString(n.Value);
                SizeStep = 0;
            }
            else
            {
                MinSize = (n = elem.Element("MinSize")) != null ? 
                    Utils.FromString(n.Value) : 100;
                MaxSize = (n = elem.Element("MaxSize")) != null ? 
                    Utils.FromString(n.Value) : 100;
                SizeStep = (n = elem.Element("SizeStep")) != null ? 
                    Utils.FromString(n.Value) : 0;
            }

            Projectiles = elem.Elements("Projectile")
                .Select(i => new ProjectileDesc(i)).ToArray();

            if ((n = elem.Element("MaxHitPoints")) != null)
                MaxHP = Utils.FromString(n.Value);
            if ((n = elem.Element("Defense")) != null)
                Defense = Utils.FromString(n.Value);
            if ((n = elem.Element("Terrain")) != null)
                Terrain = n.Value;
            if ((n = elem.Element("SpawnProbability")) != null)
                SpawnProbability = float.Parse(n.Value);
            if ((n = elem.Element("Spawn")) != null)
                Spawn = new SpawnCount(n);

            God = elem.Element("God") != null;
            Cube = elem.Element("Cube") != null;
            Quest = elem.Element("Quest") != null;
            if ((n = elem.Element("Level")) != null)
                Level = Utils.FromString(n.Value);
            else
                Level = null;

            Tags = new TagList();
            if (elem.Elements("Tag").Any())
                foreach (XElement i in elem.Elements("Tag"))
                    Tags.Add(new Tag(i));

            ArmorBreakImmune = elem.Element("ArmorBreakImmune") != null;
            CurseImmune = elem.Element("CurseImmune") != null;
            DazedImmune = elem.Element("DazedImmune") != null;
            ParalyzeImmune = elem.Element("ParalyzeImmune") != null;
            PetrifyImmune = elem.Element("PetrifyImmune") != null;
            SlowedImmune = elem.Element("SlowedImmune") != null;
            StasisImmune = elem.Element("StasisImmune") != null;
            StunImmune = elem.Element("StunImmune") != null;

            Oryx = elem.Element("Oryx") != null;
            Hero = elem.Element("Hero") != null;
            
            if ((n = elem.Element("PerRealmMax")) != null)
                PerRealmMax = Utils.FromString(n.Value);
            else
                PerRealmMax = null;
            if ((n = elem.Element("XpMult")) != null)
                ExpMultiplier = float.Parse(n.Value);
            else
                ExpMultiplier = null;

            IsPet = elem.Element("Pet") != null;
            IsPetSkin = elem.Element("PetSkin") != null;
            IsPetProjectile = elem.Element("PetProjectile") != null;
            IsPetBehavior = elem.Element("PetBehavior") != null;
            IsPetAbility = elem.Element("PetAbility") != null;
            Connects = elem.Element("Connects") != null;
            TrollWhiteBag = elem.Element("TrollWhiteBag") != null;
        }
    }
    public class TagList : List<Tag>
    {
        public bool ContainsTag(string name)
        {
            return this.Any(i => i.Name == name);
        }

        public string TagValue(string name, string value)
        {
            return
                (from i in this where i.Name == name where i.Values.ContainsKey(value) select i.Values[value])
                    .FirstOrDefault();
        }
    }
    public class Tag
    {
        public Tag(XElement elem)
        {
            Name = elem.Attribute("name").Value;
            Values = new Dictionary<string, string>();
            foreach (XElement i in elem.Elements())
            {
                if (Values.ContainsKey(i.Name.ToString()))
                    Values.Remove(i.Name.ToString());
                Values.Add(i.Name.ToString(), i.Value);
            }
        }

        public string Name { get; private set; }
        public Dictionary<string, string> Values { get; private set; }
    }

    public class TileDesc
    {
        public ushort ObjectType { get; private set; }
        public string ObjectId { get; private set; }
        public bool NoWalk { get; private set; }
        public bool Damaging { get; private set; }
        public int MinDamage { get; private set; }
        public int MaxDamage { get; private set; }
        public float Speed { get; private set; }
        public bool Push { get; private set; }
        public float PushX { get; private set; }
        public float PushY { get; private set; }

        public TileDesc(ushort type, XElement elem)
        {
            XElement n;
            ObjectType = type;
            ObjectId = elem.Attribute("id").Value;
            NoWalk = elem.Element("NoWalk") != null;
            if ((n = elem.Element("MinDamage")) != null)
            {
                MinDamage = Utils.FromString(n.Value);
                Damaging = true;
            }
            if ((n = elem.Element("MaxDamage")) != null)
            {
                MaxDamage = Utils.FromString(n.Value);
                Damaging = true;
            }
            if ((n = elem.Element("Speed")) != null)
                Speed = float.Parse(n.Value);
            Push = elem.Element("Push") != null;
            if (Push)
            {
                var anim = elem.Element("Animate");
                if (anim.Attribute("dx") != null)
                    PushX = float.Parse(anim.Attribute("dx").Value);
                if (elem.Attribute("dy") != null)
                    PushY = float.Parse(anim.Attribute("dy").Value);
            }
        }
    }

    public class DungeonDesc
    {
        public string Name { get; private set; }
        public ushort PortalId { get; private set; }
        public int Background { get; private set; }
        public bool AllowTeleport { get; private set; }
        public string Json { get; private set; }

        public DungeonDesc(XElement elem)
        {
            Name = elem.Attribute("name").Value;
            PortalId = (ushort)Utils.FromString(elem.Attribute("type").Value);
            Background = Utils.FromString(elem.Element("Background").Value);
            AllowTeleport = elem.Element("AllowTeleport") != null;
            Json = elem.Element("Json").Value;
        }
    }
}