using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;
using common;
using common.resources;
using DungeonGenerator.Dungeon;
using log4net;
using wServer.realm.entities;
using wServer.realm.entities.vendors;
using wServer.realm.worlds;

namespace wServer.realm.terrain
{
    public enum TileRegion : byte
    {
        None,
        Spawn,
        Realm_Portals,
        Store_1,
        Store_2,
        Store_3,
        Store_4,
        Store_5,
        Store_6,
        Vault,
        Loot,
        Defender,
        Hallway,
        Enemy,
        Hallway_1,
        Hallway_2,
        Hallway_3,
        Store_7,
        Store_8,
        Store_9,
        Gifting_Chest,
        Store_10,
        Store_11,
        Store_12,
        Store_13,
        Store_14,
        Store_15,
        Store_16,
        Store_17,
        Store_18,
        Store_19,
        Store_20,
        Store_21,
        Store_22,
        Store_23,
        Store_24,
        PetRegion,
        Outside_Arena,
        Item_Spawn_Point,
        Arena_Central_Spawn,
        Arena_Edge_Spawn,
        Store_25,
        Store_26,
        Store_27,
        Store_28,
        Store_29,
        Store_30,
        Store_31,
        Store_32,
        Store_33,
        Store_34,
        Store_35,
        Store_36,
        Store_37,
        Store_38,
        Store_39,
        Store_40
    }

    public enum WmapTerrain : byte
    {
        None,
        Mountains,
        HighSand,
        HighPlains,
        HighForest,
        MidSand,
        MidPlains,
        MidForest,
        LowSand,
        LowPlains,
        LowForest,
        ShoreSand,
        ShorePlains,
        BeachTowels,
    }

    public class WmapTile
    { 
        private readonly WmapDesc _originalDesc;

        public byte UpdateCount;

        public ushort TileId;
        public TileDesc TileDesc;

        public int ObjId;
        public ushort ObjType;
        public ObjectDesc ObjDesc;
        public string ObjCfg;

        public WmapTerrain Terrain;
        public TileRegion Region;
        public byte Elevation;

        public bool Spawned;

        public long SightRegion = 1;

        public WmapTile(WmapDesc desc)
        {
            _originalDesc = desc;
            Reset();
        }

        public void Reset(Wmap map = null, int x = 0, int y = 0)
        {
            TileId = _originalDesc.TileId;
            TileDesc = _originalDesc.TileDesc;
            
            ObjType = _originalDesc.ObjType;
            ObjDesc = _originalDesc.ObjDesc;
            ObjCfg = _originalDesc.ObjCfg;

            Terrain = _originalDesc.Terrain;
            Region = _originalDesc.Region;
            Elevation = _originalDesc.Elevation;

            if (map != null)
                InitConnection(map, x, y);
            
            UpdateCount++;
        }

        public void InitConnection(Wmap map, int x, int y)
        {
            if (ObjDesc == null || !ObjDesc.Connects || ObjCfg.Contains("conn:"))
                return;

            var connStr = ConnectionComputer.GetConnString(
                (dx, dy) => map.Contains(x + dx, y + dy) &&
                            map[x + dx, y + dy].ObjType == ObjDesc.ObjectType);
            ObjCfg = $"{ObjCfg};{connStr};";
        }

        public void CopyTo(WmapTile tile)
        {
            tile.TileId = TileId;
            tile.TileDesc = TileDesc;
            
            tile.ObjType = ObjType;
            tile.ObjDesc = ObjDesc;
            tile.ObjCfg = ObjCfg;

            tile.Terrain = Terrain;
            tile.Region = Region;
            tile.Elevation = Elevation;
        }

        public ObjectDef ToDef(int x, int y)
        {
            var stats = new List<KeyValuePair<StatsType, object>>();
            if (!string.IsNullOrEmpty(ObjCfg))
                foreach (var item in ObjCfg.Split(';'))
                {
                    var kv = item.Split(':');
                    switch (kv[0])
                    {
                        case "hp":
                            var hp = Utils.FromString(kv[1]);
                            stats.Add(new KeyValuePair<StatsType, object>(StatsType.HP, hp));
                            stats.Add(new KeyValuePair<StatsType, object>(StatsType.MaximumHP, hp));
                            break;
                        case "name":
                            stats.Add(new KeyValuePair<StatsType, object>(StatsType.Name, kv[1]));
                            break;
                        case "size":
                            stats.Add(new KeyValuePair<StatsType, object>(StatsType.Size, Math.Min(500, Utils.FromString(kv[1]))));
                            break;
                        case "eff":
                            stats.Add(new KeyValuePair<StatsType, object>(StatsType.Effects, Utils.FromString(kv[1])));
                            break;
                        case "conn":
                            stats.Add(new KeyValuePair<StatsType, object>(StatsType.ObjectConnection, Utils.FromString(kv[1])));
                            break;
                        case "mtype":
                            stats.Add(new KeyValuePair<StatsType, object>(StatsType.MerchantMerchandiseType, Utils.FromString(kv[1])));
                            break;
                        case "mcost":
                            stats.Add(new KeyValuePair<StatsType, object>(StatsType.SellablePrice, Math.Max(0, Utils.FromString(kv[1]))));
                            break;
                        case "mcur":
                            stats.Add(new KeyValuePair<StatsType, object>(StatsType.SellablePriceCurrency, Utils.FromString(kv[1])));
                            break;
                        case "mamnt":
                            stats.Add(new KeyValuePair<StatsType, object>(StatsType.MerchantRemainingCount, Utils.FromString(kv[1])));
                            break;
                        case "mtime":
                            stats.Add(new KeyValuePair<StatsType, object>(StatsType.MerchantRemainingMinute, Utils.FromString(kv[1])));
                            break;
                        case "mdisc":
                            stats.Add(new KeyValuePair<StatsType, object>(StatsType.MerchantDiscount, Utils.FromString(kv[1])));
                            break;
                        case "mrank":
                        case "stars":
                            stats.Add(new KeyValuePair<StatsType, object>(StatsType.SellableRankRequirement, Utils.FromString(kv[1])));
                            break;
                    }
                }
            return new ObjectDef()
            {
                ObjectType = ObjType,
                Stats = new ObjectStats()
                {
                    Id = ObjId,
                    Position = new Position()
                    {
                        X = x + 0.5f,
                        Y = y + 0.5f
                    },
                    Stats = stats.ToArray()
                }
            };
        }
    }

    public class WmapDesc
    {
        public ushort TileId;
        public TileDesc TileDesc;

        public ushort ObjType;
        public ObjectDesc ObjDesc;
        public string ObjCfg;

        public WmapTerrain Terrain;
        public TileRegion Region;
        public byte Elevation;
    }

    public class Wmap
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Wmap));

        private readonly XmlData _dat;
        
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Dictionary<IntPoint, TileRegion> Regions { get; }
        private Tuple<IntPoint, ushort, string>[] _entities;
        
        private WmapTile[,] _tiles;
        public WmapTile this[int x, int y]
        {
            get { return _tiles[x, y]; }
            set { _tiles[x, y] = value; }
        }

        public Wmap(XmlData dat)
        {
            _dat = dat;
            Regions = new Dictionary<IntPoint, TileRegion>();
        }

        public bool Contains(IntPoint point)
        {
            var x = point.X;
            var y = point.Y;

            if (x < 0 || x >= Width ||
                y < 0 || y >= Height)
                return false;

            return true;
        }

        public bool Contains(int x, int y)
        {
            if (x < 0 || x >= Width ||
                y < 0 || y >= Height)
                return false;

            return true;
        }

        public int Load(Stream stream, int idBase)
        {
            var ver = stream.ReadByte();
            if (ver < 0 || ver > 2)
                throw new NotSupportedException("WMap version " + ver);

            using (var rdr = new BinaryReader(new ZlibStream(stream, CompressionMode.Decompress)))
            {
                var dict = new List<WmapDesc>();
                var c = rdr.ReadInt16();
                for (var i = 0; i < c; i++)
                {
                    var desc = new WmapDesc();
                    desc.TileId = rdr.ReadUInt16();
                    desc.TileDesc = _dat.Tiles[desc.TileId];
                    var obj = rdr.ReadString();
                    desc.ObjType = 0;
                    if (_dat.IdToObjectType.ContainsKey(obj))
                        desc.ObjType = _dat.IdToObjectType[obj];
                    else if (!string.IsNullOrEmpty(obj))
                        Log.Warn($"Object: {obj} not found.");
                    desc.ObjCfg = rdr.ReadString();
                    desc.Terrain = (WmapTerrain)rdr.ReadByte();
                    desc.Region = (TileRegion)rdr.ReadByte();
                    if (ver == 1)
                        desc.Elevation = rdr.ReadByte();
                    _dat.ObjectDescs.TryGetValue(desc.ObjType, out desc.ObjDesc);
                    dict.Add(desc);
                }

                Width = rdr.ReadInt32();
                Height = rdr.ReadInt32();
                _tiles = new WmapTile[Width, Height];

                var enCount = 0;
                var entities = new List<Tuple<IntPoint, ushort, string>>();
                for (var y = 0; y < Height; y++)
                    for (var x = 0; x < Width; x++)
                    {
                        var tile = new WmapTile(dict[rdr.ReadInt16()]);
                        if (ver == 2)
                            tile.Elevation = rdr.ReadByte();
                        
                        if (tile.Region != 0)
                            Regions.Add(new IntPoint(x, y), tile.Region);
                        
                        var desc = tile.ObjDesc;
                        if (tile.ObjType != 0 && (desc == null || !desc.Static || desc.Enemy))
                        {
                            entities.Add(new Tuple<IntPoint, ushort, string>(new IntPoint(x, y), tile.ObjType, tile.ObjCfg));
                            if (desc == null || !(desc.Enemy && desc.Static))
                                tile.ObjType = 0;
                        }

                        if (tile.ObjType != 0 && (desc == null || !(desc.Enemy && desc.Static)))
                        {
                            enCount++;
                            tile.ObjId = idBase + enCount;
                        }
                        
                        _tiles[x, y] = tile;
                    }

                for (var x = 0; x < Width; x++)
                    for (var y = 0; y < Height; y++)
                        _tiles[x, y].InitConnection(this, x, y);
                
                _entities = entities.ToArray();
                return enCount;
            }
        }

        public int Load(DungeonTile[,] tiles, int idBase)
        {
            Width = tiles.GetLength(0);
            Height = tiles.GetLength(1);

            var wTiles = new WmapDesc[Width,Height];
            for (var i = 0; i < Width; i++)
                for (var j = 0; j < Height; j++)
                {
                    var dTile = tiles[i, j];
                    
                    var wTile = new WmapDesc();
                    wTile.TileId = _dat.IdToTileType[dTile.TileType.Name];
                    wTile.TileDesc = _dat.Tiles[wTile.TileId];
                    wTile.Terrain = WmapTerrain.None;
                    wTile.Region = (dTile.Region == null) ? 
                        TileRegion.None :
                        (TileRegion)Enum.Parse(typeof(TileRegion), dTile.Region);
                    if (dTile.Object != null)
                    {
                        wTile.ObjType = _dat.IdToObjectType[dTile.Object.ObjectType.Name];
                        wTile.ObjCfg = dTile.Object.ToString();
                        _dat.ObjectDescs.TryGetValue(wTile.ObjType, out wTile.ObjDesc);
                    }
                    
                    wTiles[i, j] = wTile;
                }
            
            _tiles = new WmapTile[Width, Height];

            var enCount = 0;
            var entities = new List<Tuple<IntPoint, ushort, string>>();
            for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var tile = new WmapTile(wTiles[x,y]);

                    if (tile.Region != 0)
                        Regions.Add(new IntPoint(x, y), tile.Region);

                    var desc = tile.ObjDesc;
                    if (tile.ObjType != 0 && (desc == null || !desc.Static || desc.Enemy))
                    {
                        entities.Add(new Tuple<IntPoint, ushort, string>(new IntPoint(x, y), tile.ObjType, tile.ObjCfg));
                        if (desc == null || !(desc.Enemy && desc.Static))
                            tile.ObjType = 0;
                    }

                    if (tile.ObjType != 0 && (desc == null || !(desc.Enemy && desc.Static)))
                    {
                        enCount++;
                        tile.ObjId = idBase + enCount;
                    }
                    
                    _tiles[x, y] = tile;
                }
            
            for (var x = 0; x < Width; x++)
                for (var y = 0; y < Height; y++)
                    _tiles[x, y].InitConnection(this, x, y);
            
            _entities = entities.ToArray();
            return enCount;
        }

        public IEnumerable<Entity> InstantiateEntities(RealmManager manager, IntPoint offset = new IntPoint())
        {
            foreach (var i in _entities)
            {
                var entity = Entity.Resolve(manager, i.Item2);
                entity.Move(i.Item1.X + 0.5f + offset.X, i.Item1.Y + 0.5f + offset.Y);
                if (i.Item3 != null)
                    foreach (var item in i.Item3.Split(';'))
                    {
                        string[] kv = item.Split(':');
                        switch (kv[0])
                        {
                            case "hp":
                                (entity as Enemy).HP = Utils.FromString(kv[1]);
                                (entity as Enemy).MaximumHP = (entity as Enemy).HP; 
                                break;
                            case "name":
                                entity.Name = kv[1]; break;
                            case "size":
                                entity.SetDefaultSize(Math.Min(500, Utils.FromString(kv[1])));
                                break;
                            case "eff":
                                entity.ConditionEffects = (ConditionEffects) ulong.Parse(kv[1]);
                                break;
                            case "conn":
                                (entity as ConnectedObject).Connection = ConnectionInfo.Infos[(uint)Utils.FromString(kv[1])];
                                break;
                            case "mtype":
                                (entity as Merchant).Item = (ushort) Utils.FromString(kv[1]);
                                break;
                            case "mcost":
                                (entity as SellableObject).Price = Math.Max(0, Utils.FromString(kv[1]));
                                break;
                            case "mcur":
                                (entity as SellableObject).Currency = (CurrencyType)Utils.FromString(kv[1]);
                                break;
                            case "mamnt":
                                (entity as Merchant).Count = Utils.FromString(kv[1]);
                                break;
                            case "mtime":
                                (entity as Merchant).TimeLeft = Utils.FromString(kv[1]);
                                break;
                            case "mdisc": // not implemented
                                break;
                            case "mrank":
                            case "stars": // provided for backwards compatibility with older maps
                                (entity as SellableObject).RankReq = Utils.FromString(kv[1]);
                                break;
                            case "mtax":
                                (entity as SellableObject).Tax = Utils.FromString(kv[1]);
                                break;
                            case "xOffset":
                                var xo = float.Parse(kv[1]);
                                entity.Move(entity.X + xo, entity.Y);
                                break;
                            case "yOffset":
                                var yo = float.Parse(kv[1]);
                                entity.Move(entity.X, entity.Y + yo);
                                break;
                        }
                    }

                yield return entity;
            }
        }

        public void ResetTiles()
        {
            Regions.Clear();
            for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var t = _tiles[x, y];
                    t.Reset();
                    if (t.Region != 0)
                        Regions.Add(new IntPoint(x, y), t.Region);
                }
        }

        // typically this method is used with setpieces. It's data is
        // copied to the supplied world at the said position
        public void ProjectOntoWorld(World world, IntPoint pos)
        {
            for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var projX = pos.X + x;
                    var projY = pos.Y + y;
                    if (!world.Map.Contains(projX, projY))
                        continue;

                    var tile = world.Map[projX, projY];

                    var spTile = _tiles[x, y];
                    if (spTile.TileId == 255)
                        continue;
                    spTile.CopyTo(tile);

                    if (spTile.ObjId != 0)
                        tile.ObjId = world.GetNextEntityId();

                    if (tile.Region != 0)
                        world.Map.Regions.Add(new IntPoint(projX, projY), spTile.Region);

                    tile.UpdateCount++;
                }

            foreach (var e in InstantiateEntities(world.Manager, pos))
            {
                if (!world.Map.Contains((int)e.X, (int)e.Y))
                    continue;

                world.EnterWorld(e);
            }
        }
    }
}
