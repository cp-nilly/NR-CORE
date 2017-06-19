using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using log4net;

namespace common.resources
{
    public class XmlData : IDisposable
    {
        static ILog log = LogManager.GetLogger(typeof(XmlData));

        private readonly List<string> _gameXmls;
        public IList<string> GameXmls { get; private set; }

        private readonly List<string[]> _usedRemoteTextures;
        public IList<string[]> UsedRemoteTextures { get; private set; }

        public byte[] ZippedXmls { get; private set; }

        Dictionary<ushort, XElement> type2elem_obj;
        Dictionary<ushort, string> type2id_obj;
        Dictionary<string, ushort> id2type_obj;
        Dictionary<string, ushort> d_name2type_obj;
        Dictionary<ushort, XElement> type2elem_tile;
        Dictionary<ushort, string> type2id_tile;
        Dictionary<string, ushort> id2type_tile;
        Dictionary<ushort, XElement> type2elem_equipSet;
        Dictionary<ushort, string> type2id_equipSet;
        Dictionary<string, ushort> id2type_equipSet;
        Dictionary<ushort, ushort> skinType2equipSetType;
        Dictionary<ushort, TileDesc> tiles;
        Dictionary<ushort, Item> items;
        Dictionary<ushort, ObjectDesc> objDescs;
        Dictionary<ushort, PortalDesc> portals;
        Dictionary<ushort, SkinDesc> skins;
        Dictionary<ushort, EquipmentSetDesc> equipmentSets;
        Dictionary<ushort, PlayerDesc> classes;
        Dictionary<ushort, ObjectDesc> merchants; 
        Dictionary<ushort, PetDesc> pets;
        Dictionary<ushort, PetSkinDesc> petSkins;
        Dictionary<ushort, PetBehaviorDesc> petBehaviors;
        Dictionary<ushort, PetAbilityDesc> petAbilities;
        Dictionary<int, ItemType> slotType2ItemType;

        public IDictionary<ushort, XElement> ObjectTypeToElement { get; private set; }
        public IDictionary<ushort, string> ObjectTypeToId { get; private set; }
        public IDictionary<string, ushort> IdToObjectType { get; private set; }
        public IDictionary<string, ushort> DisplayIdToObjectType { get; private set; }
        public IDictionary<ushort, XElement> TileTypeToElement { get; private set; }
        public IDictionary<ushort, string> TileTypeToId { get; private set; }
        public IDictionary<string, ushort> IdToTileType { get; private set; }
        public IDictionary<ushort, XElement> EquipSetTypeToElement { get; private set; }
        public IDictionary<ushort, string> EquipSetTypeToId { get; private set; }
        public IDictionary<string, ushort> IdToEquipSetType { get; private set; }
        public IDictionary<ushort, ushort> SkinTypeToEquipSetType { get; private set; } 
        public IDictionary<ushort, TileDesc> Tiles { get; private set; }
        public IDictionary<ushort, Item> Items { get; private set; }
        public IDictionary<ushort, ObjectDesc> ObjectDescs { get; private set; }
        public IDictionary<ushort, PortalDesc> Portals { get; private set; }
        public IDictionary<ushort, SkinDesc> Skins { get; private set; }
        public IDictionary<ushort, EquipmentSetDesc> EquipmentSets { get; private set; } 
        public IDictionary<ushort, PlayerDesc> Classes { get; private set; }
        public IDictionary<ushort, ObjectDesc> Merchants { get; private set; }
        public IDictionary<ushort, PetDesc> Pets { get; private set; }
        public IDictionary<ushort, PetSkinDesc> PetSkins { get; private set; }
        public IDictionary<ushort, PetBehaviorDesc> PetBehaviors { get; private set; }
        public IDictionary<ushort, PetAbilityDesc> PetAbilities { get; private set; }
        public IDictionary<int, ItemType> SlotType2ItemType { get; private set; }

        int updateCount = 0;
        int prevUpdateCount = -1;
        XElement addition;
        string[] addXml;

        public XmlData(string path)
        {
            log.Info("Loading xml data...");

            GameXmls = 
                new ReadOnlyCollection<string>(
                    _gameXmls = new List<string>());

            UsedRemoteTextures =
                new ReadOnlyCollection<string[]>(
                    _usedRemoteTextures = new List<string[]>());

            ObjectTypeToElement = 
                new ReadOnlyDictionary<ushort, XElement>(
                    type2elem_obj = new Dictionary<ushort, XElement>());
            ObjectTypeToId = 
                new ReadOnlyDictionary<ushort, string>(
                    type2id_obj = new Dictionary<ushort, string>());
            IdToObjectType = 
                new ReadOnlyDictionary<string, ushort>(
                    id2type_obj = new Dictionary<string, ushort>(StringComparer.InvariantCultureIgnoreCase));
            DisplayIdToObjectType =
                new ReadOnlyDictionary<string, ushort>(
                    d_name2type_obj = new Dictionary<string, ushort>(StringComparer.InvariantCultureIgnoreCase));
            TileTypeToElement = 
                new ReadOnlyDictionary<ushort, XElement>(
                    type2elem_tile = new Dictionary<ushort, XElement>());
            TileTypeToId = 
                new ReadOnlyDictionary<ushort, string>(
                    type2id_tile = new Dictionary<ushort, string>());
            IdToTileType = 
                new ReadOnlyDictionary<string, ushort>(
                    id2type_tile = new Dictionary<string, ushort>(StringComparer.InvariantCultureIgnoreCase));
            EquipSetTypeToElement =
                new ReadOnlyDictionary<ushort, XElement>(
                    type2elem_equipSet = new Dictionary<ushort, XElement>());
            EquipSetTypeToId =
                new ReadOnlyDictionary<ushort, string>(
                    type2id_equipSet = new Dictionary<ushort, string>());
            SkinTypeToEquipSetType =
                new ReadOnlyDictionary<ushort, ushort>(
                    skinType2equipSetType = new Dictionary<ushort, ushort>());
            IdToEquipSetType =
                new ReadOnlyDictionary<string, ushort>(
                    id2type_equipSet = new Dictionary<string, ushort>(StringComparer.InvariantCultureIgnoreCase));
            Tiles = 
                new ReadOnlyDictionary<ushort, TileDesc>(
                    tiles = new Dictionary<ushort, TileDesc>());
            Items = 
                new ReadOnlyDictionary<ushort, Item>(
                    items = new Dictionary<ushort, Item>());
            ObjectDescs = 
                new ReadOnlyDictionary<ushort, ObjectDesc>(
                    objDescs = new Dictionary<ushort, ObjectDesc>());
            Portals = 
                new ReadOnlyDictionary<ushort, PortalDesc>(
                    portals = new Dictionary<ushort, PortalDesc>());
            Skins = 
                new ReadOnlyDictionary<ushort, SkinDesc>(
                    skins = new Dictionary<ushort, SkinDesc>());
            EquipmentSets =
                new ReadOnlyDictionary<ushort, EquipmentSetDesc>(
                    equipmentSets = new Dictionary<ushort, EquipmentSetDesc>());
            Classes = 
                new ReadOnlyDictionary<ushort, PlayerDesc>(
                    classes = new Dictionary<ushort, PlayerDesc>());
            Merchants =
                new ReadOnlyDictionary<ushort, ObjectDesc>(
                    merchants = new Dictionary<ushort, ObjectDesc>());
            Pets =
                new ReadOnlyDictionary<ushort, PetDesc>(
                    pets = new Dictionary<ushort, PetDesc>());
            PetSkins =
                new ReadOnlyDictionary<ushort, PetSkinDesc>(
                    petSkins = new Dictionary<ushort, PetSkinDesc>());
            PetBehaviors =
                new ReadOnlyDictionary<ushort, PetBehaviorDesc>(
                    petBehaviors = new Dictionary<ushort, PetBehaviorDesc>());
            PetAbilities =
                new ReadOnlyDictionary<ushort, PetAbilityDesc>(
                    petAbilities = new Dictionary<ushort, PetAbilityDesc>());
            SlotType2ItemType =
                new ReadOnlyDictionary<int, ItemType>(
                    slotType2ItemType = new Dictionary<int, ItemType>());

            addition = new XElement("ExtData");

            string basePath = Utils.GetBasePath(path);
            
            // load additional xmls into GameXmls string array
            LoadXmls(basePath, "*.xml");

            // compress GameXmls for getServerXmls query
            // - only want to compress additional xml content
            // no need to send xmls that are already in client
            // - kind of a hack job since all additions have the
            // .xml file type while embedded xmls have the .dat
            // file type...
            ZippedXmls = ZipGameXmls();

            // add embedded client xmls to GameXmls string array
            LoadXmls(basePath, "*.dat");

            log.Info("Finish loading game data.");
            log.InfoFormat("{0} Items", items.Count);
            log.InfoFormat("{0} Tiles", tiles.Count);
            log.InfoFormat("{0} Objects", objDescs.Count);
            log.InfoFormat("{0} Skins", skins.Count);
            log.InfoFormat("{0} Equipment Sets", equipmentSets.Count);
            log.InfoFormat("{0} Classes", classes.Count);
            log.InfoFormat("{0} Portals", portals.Count);
            log.InfoFormat("{0} Merchants", merchants.Count);
            log.InfoFormat("{0} Pets", pets.Count);
            log.InfoFormat("{0} PetSkins", petSkins.Count);
            log.InfoFormat("{0} PetBehaviors", petBehaviors.Count);
            log.InfoFormat("{0} PetsAbility", petAbilities.Count);
            log.InfoFormat("{0} Remote Textures", _usedRemoteTextures.Count);
            log.InfoFormat("{0} Additions", addition.Elements().Count());
        }

        private void LoadXmls(string basePath, string ext)
        {
            var xmls = Directory.EnumerateFiles(basePath, ext, SearchOption.AllDirectories).ToArray();
            for (var i = 0; i < xmls.Length; i++)
            {
                //log.InfoFormat("Loading '{0}'({1}/{2})...", xmls[i], i + 1, xmls.Length);
                var xml = File.ReadAllText(xmls[i]);
                _gameXmls.Add(xml);
                ProcessXml(XElement.Parse(xml));
            }
        }

        private byte[] ZipGameXmls()
        {
            using (var ms = new MemoryStream())
            {
                var wtr = new NWriter(ms);
                wtr.Write(GameXmls.Count);
                foreach (var xml in GameXmls)
                    wtr.Write32UTF(xml);

                return Utils.Deflate(ms.ToArray());
            }
        }

        private void AddObjects(XElement root)
        {
            foreach (var elem in root.XPathSelectElements("//Object"))
            {
                if (elem.Element("Class") == null)
                    continue;

                var cls = elem.Element("Class").Value;
                var id = elem.Attribute("id").Value;

                ushort type;
                var typeAttr = elem.Attribute("type");
                if (typeAttr == null)
                {
                    log.Error($"{id} is missing type number. Skipped.");
                    continue;
                }
                type = (ushort)Utils.FromString(typeAttr.Value);

                if (type2id_obj.ContainsKey(type))
                    log.WarnFormat("'{0}' and '{1}' has the same ID of 0x{2:x4}!", id, type2id_obj[type], type);
                else
                {
                    type2id_obj[type] = id;
                    type2elem_obj[type] = elem;
                }
                
                if (id2type_obj.ContainsKey(id))
                    log.WarnFormat("0x{0:x4} and 0x{1:x4} has the same name of {2}!", type, id2type_obj[id], id);
                else
                    id2type_obj[id] = type;
                
                var displayId = elem.Element("DisplayId") != null ? elem.Element("DisplayId").Value : null;

                string displayName;

                if (displayId == null)
                {
                    displayName = id;
                }
                else
                {
                    if (displayId[0].Equals('{'))
                    {
                        displayName = id;
                    }
                    else
                    {
                        displayName =  displayId;
                    }
                }

                d_name2type_obj[displayName] = type;
   
                switch (cls)
                {
                    case "Equipment":
                    case "Dye":
                        items[type] = new Item(type, elem);
                        break;
                    case "Pet":
                        pets[type] = new PetDesc(type, elem);
                        objDescs[type] = pets[type];
                        break;
                    case "PetSkin":
                        petSkins[type] = new PetSkinDesc(type, elem);
                        break;
                    case "PetBehavior":
                        petBehaviors[type] = new PetBehaviorDesc(type, elem);
                        break;
                    case "PetAbility":
                        petAbilities[type] = new PetAbilityDesc(type, elem);
                        break;
                    case "Skin":
                        var skinDesc = SkinDesc.FromElem(type, elem);
                        if (skinDesc != null)
                            skins.Add(type, skinDesc);
                        // might want to add skin description to objDesc
                        // dictionary so that skins can be merched...
                        // perhaps later
                        break;
                    case "Player":
                        var pDesc = new PlayerDesc(type, elem);
                        slotType2ItemType[pDesc.SlotTypes[0]] = ItemType.Weapon;
                        slotType2ItemType[pDesc.SlotTypes[1]] = ItemType.Ability;
                        slotType2ItemType[pDesc.SlotTypes[2]] = ItemType.Armor;
                        slotType2ItemType[pDesc.SlotTypes[3]] = ItemType.Ring;
                        classes[type] = new PlayerDesc(type, elem);
                        objDescs[type] = classes[type];
                        break;
                    case "Portal":
                        portals[type] = new PortalDesc(type, elem);
                        objDescs[type] = portals[type];
                        break;
                    case "GuildMerchant":
                    case "Merchant":
                        merchants[type] = new ObjectDesc(type, elem);
                        break;
                    default:
                        objDescs[type] = new ObjectDesc(type, elem);
                        break;
                }

                // collect info on used remote textures
                var rt = elem.Element("RemoteTexture");
                if (rt != null)
                    try
                    {
                        var texType =  GetRemoteTexType(rt);
                        if (_usedRemoteTextures.All(tex => tex[1] != texType[1]))
                            _usedRemoteTextures.Add(texType);
                    }
                    catch (Exception e)
                    {
                        log.WarnFormat("Getting remote texture info for '{0}, {1}' failed! {2}",
                            id, type, e.Message);
                    }

                var extAttr = elem.Attribute("ext");
                bool ext;
                if (extAttr != null && bool.TryParse(extAttr.Value, out ext) && ext)
                {
                    if (elem.Attribute("type") == null)
                        elem.Add(new XAttribute("type", type));
                    this.addition.Add(elem);
                    updateCount++;
                }
            }
        }

        private void AddGrounds(XElement root)
        {
            foreach (var elem in root.XPathSelectElements("//Ground"))
            {
                string id = elem.Attribute("id").Value;

                ushort type;
                var typeAttr = elem.Attribute("type");
                type = (ushort)Utils.FromString(typeAttr.Value);

                if (type2id_tile.ContainsKey(type))
                    log.WarnFormat("'{0}' and '{1}' has the same ID of 0x{2:x4}!", id, type2id_tile[type], type);
                if (id2type_tile.ContainsKey(id))
                    log.WarnFormat("0x{0:x4} and 0x{1:x4} has the same name of {2}!", type, id2type_tile[id], id);

                type2id_tile[type] = id;
                id2type_tile[id] = type;
                type2elem_tile[type] = elem;

                tiles[type] = new TileDesc(type, elem);

                var extAttr = elem.Attribute("ext");
                bool ext;
                if (extAttr != null && bool.TryParse(extAttr.Value, out ext) && ext)
                {
                    this.addition.Add(elem);
                    updateCount++;
                }
            }
        }

        private void AddEquipmentSets(XElement root)
        {
            foreach (var elem in root.XPathSelectElements("//EquipmentSet"))
            {
                string id = elem.Attribute("id").Value;

                ushort type;
                var typeAttr = elem.Attribute("type");
                type = (ushort) Utils.FromString(typeAttr.Value);

                if (type2id_equipSet.ContainsKey(type))
                    log.WarnFormat("'{0}' and '{1}' has the same ID of 0x{2:x4}!", id, type2id_equipSet[type], type);
                if (id2type_equipSet.ContainsKey(id))
                    log.WarnFormat("0x{0:x4} and 0x{1:x4} has the same name of {2}!", type, id2type_equipSet[id], id);

                type2id_equipSet[type] = id;
                id2type_equipSet[id] = type;
                type2elem_equipSet[type] = elem;

                ushort skinType;
                equipmentSets[type] = EquipmentSetDesc.FromElem(type, elem, out skinType);

                if (skinType != 0)
                {
                    if (skinType2equipSetType.ContainsKey(skinType))
                        log.WarnFormat("'{0}' and '{1}' has the same skinType of 0x{2:x4}!", 
                            id, type2id_equipSet[skinType2equipSetType[skinType]], skinType);

                    skinType2equipSetType[skinType] = type;
                }
            }
        }

        /* GetRemoteTexType - Generates texture type 
         * given the different formats people have
         * been using to specify remote textures.
         */
        private static string[] GetRemoteTexType(XElement rt)
        {
            var texType = rt.Element("Id").Value.Split(':');
            if (rt.Element("Instance") == null)
            {
                if (texType.Length != 2)
                    throw new Exception("Invalid remote texture.");

                if (texType[0].ToLower().Equals("production"))
                    texType[0] = "draw";
                else if (texType[0].ToLower().Equals("testing"))
                    texType[0] = "tdraw";

                return texType;
            }

            if (texType.Length != 1)
                throw new Exception("Invalid remote texture.");

            var instance = rt.Element("Instance").Value.ToLower();
            if (instance.Equals("production") || instance.Equals("draw"))
                return new[] { "draw", texType[0] };
            if (instance.Equals("testing") || instance.Equals("tdraw"))
                return new[] { "tdraw", texType[0] };
            throw new Exception("Invalid remote texture.");
        }

        private void ProcessXml(XElement root)
        {
            AddObjects(root);
            AddGrounds(root);
            AddEquipmentSets(root);
        }

        private void UpdateXml()
        {
            if (prevUpdateCount != updateCount)
            {
                addXml = new string[] { addition.ToString() };
                prevUpdateCount = updateCount;
            }
        }

        public string[] AdditionXml
        {
            get
            {
                UpdateXml();
                return addXml;
            }
        }

        public void Dispose()
        {
        }
    }
}