using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace common.resources
{
    public enum PFamily : ushort
    {
        Undefined,
        Aquatic,
        Automaton,
        Avian,
        Canine,
        Exotic,
        Farm,
        Feline,
        Humanoid,
        Insect,
        Penguin,
        Reptile,
        Spooky,
        Unknown, //this is the ? ? ? ? family
        Woodland
    }

    public enum PRarity : ushort
    {
        Common,
        Uncommon,
        Rare,
        Legendary,
        Divine,
        Undefined
    }

    public enum PAbility : ushort
    {
        Undefined = 0,
        AttackClose = 0x192,
        AttackMid = 0x14f,
        AttackFar = 0x195,
        Electric = 0x196,
        Heal = 0x154,
        MagicHeal = 0x155,
        Savage = 0x156,
        Decoy = 0x157,
        RisingFury = 0x158
    }

    public class PetDesc : ObjectDesc
    {
        public PetDesc(ushort objType, XElement elem) : base(objType, elem)
        {
            Family = GetFamily(elem.Element("Family"));
            Rarity = GetRarity(elem.Element("Rarity"));
            FirstAbility = GetAbility(elem.Element("FirstAbility"));
            DefaultSkin = elem.Element("DefaultSkin").Value;
        }

        public PFamily Family { get; private set; }
        public PRarity Rarity { get; private set; }
        public PAbility FirstAbility { get; private set; }
        public string DefaultSkin { get; private set; }

        public static PRarity GetRarity(XElement rarity)
        {
            if (rarity == null)
                return PRarity.Undefined;
            return (PRarity)Enum.Parse(typeof(PRarity), rarity.Value);
        }

        public static PFamily GetFamily(XElement family)
        {
            if (family == null)
                return PFamily.Undefined;
            if (family.Value.Equals("? ? ? ?"))
                return PFamily.Unknown;
            return (PFamily)Enum.Parse(typeof(PFamily), family.Value);
        }

        public static PAbility GetAbility(XElement ability)
        {
            if (ability == null)
                return PAbility.Undefined;

            switch (ability.Value)
            {
                case "Attack Close":
                    return PAbility.AttackClose;
                case "Attack Mid":
                    return PAbility.AttackMid;
                case "Attack Far":
                    return PAbility.AttackFar;
                case "Electric":
                    return PAbility.Electric;
                case "Heal":
                    return PAbility.Heal;
                case "Magic Heal":
                    return PAbility.MagicHeal;
                case "Savage":
                    return PAbility.Savage;
                case "Decoy":
                    return PAbility.Decoy;
                case "Rising Fury":
                    return PAbility.RisingFury;
                default:
                    return PAbility.Undefined;
            }
        }
    }

    public class PetSkinDesc : ObjectDesc
    {
        public PetSkinDesc(ushort objType, XElement elem) : base(objType, elem)
        {
        }
    }

    public class PetBehaviorDesc : ObjectDesc
    {
        public PetBehaviorDesc(ushort objType, XElement elem) : base(objType, elem)
        {
            BaseBehavior = elem.Element("BaseBehavior").Attribute("id").Value;
            Parameters = elem.Element("Parameters");
        }

        public string BaseBehavior { get; private set; }
        public XElement Parameters { get; private set; }
    }

    public class PetAbilityDesc : ObjectDesc
    {
        public PetAbilityDesc(ushort objType, XElement elem) : base (objType, elem)
        {
            Group = elem.Element("Group").Value;
        }

        public string Group { get; private set; }
    }
}
