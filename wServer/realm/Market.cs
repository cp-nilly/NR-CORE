using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using common;
using common.resources;
using log4net;
using StackExchange.Redis;
using wServer.realm.entities;
using wServer.realm.entities.vendors;
using wServer.realm.terrain;
using wServer.realm.worlds;

namespace wServer.realm
{
    public class Market
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Market));

        private readonly RealmManager _manager;
        private readonly DbMarket _dbMarket;
        private readonly ConcurrentDictionary<IntPoint, PlayerMerchant> _merchants;
        private readonly Dictionary<ItemType, ConcurrentDictionary<IntPoint, int>> _shopTypeLocations;
        private World _marketplace;

        private readonly ConcurrentDictionary<ushort, List<PlayerShopItem>> _weapons;
        private readonly ConcurrentDictionary<ushort, List<PlayerShopItem>> _abilities;
        private readonly ConcurrentDictionary<ushort, List<PlayerShopItem>> _armor;
        private readonly ConcurrentDictionary<ushort, List<PlayerShopItem>> _rings;
        private readonly ConcurrentDictionary<ushort, List<PlayerShopItem>> _statPots;
        private readonly ConcurrentDictionary<ushort, List<PlayerShopItem>> _other;
        
        private readonly Dictionary<ItemType, ConcurrentQueue<ushort>> _queues;
        private readonly Dictionary<ItemType, ReadOnlyDictionary<ushort, List<PlayerShopItem>>> _shops;
        private readonly Dictionary<ItemType, HashSet<ushort>> _items; 

        public Market(RealmManager manager)
        {
            Log.Info("Initializing player market...");

            _manager = manager;
            _dbMarket = new DbMarket(manager.Database.Conn);
            _merchants = new ConcurrentDictionary<IntPoint, PlayerMerchant>();
            _shopTypeLocations = new Dictionary<ItemType, ConcurrentDictionary<IntPoint, int>>();

            _weapons = new ConcurrentDictionary<ushort, List<PlayerShopItem>>();
            _abilities = new ConcurrentDictionary<ushort, List<PlayerShopItem>>();
            _armor = new ConcurrentDictionary<ushort, List<PlayerShopItem>>();
            _rings = new ConcurrentDictionary<ushort, List<PlayerShopItem>>();
            _statPots = new ConcurrentDictionary<ushort, List<PlayerShopItem>>();
            _other = new ConcurrentDictionary<ushort, List<PlayerShopItem>>();

            _shops = new Dictionary<ItemType, ReadOnlyDictionary<ushort, List<PlayerShopItem>>>
            {
                {ItemType.Weapon, new ReadOnlyDictionary<ushort, List<PlayerShopItem>>(_weapons)},
                {ItemType.Ability, new ReadOnlyDictionary<ushort, List<PlayerShopItem>>(_abilities)},
                {ItemType.Armor, new ReadOnlyDictionary<ushort, List<PlayerShopItem>>(_armor)},
                {ItemType.Ring, new ReadOnlyDictionary<ushort, List<PlayerShopItem>>(_rings)},
                {ItemType.StatPot, new ReadOnlyDictionary<ushort, List<PlayerShopItem>>(_statPots)},
                {ItemType.Other, new ReadOnlyDictionary<ushort, List<PlayerShopItem>>(_other)}
            };

            _queues = new Dictionary<ItemType, ConcurrentQueue<ushort>>
            {
                {ItemType.Weapon, new ConcurrentQueue<ushort>()},
                {ItemType.Ability, new ConcurrentQueue<ushort>()},
                {ItemType.Armor, new ConcurrentQueue<ushort>()},
                {ItemType.Ring, new ConcurrentQueue<ushort>()},
                {ItemType.StatPot, new ConcurrentQueue<ushort>()},
                {ItemType.Other, new ConcurrentQueue<ushort>()}
            };

            _items = new Dictionary<ItemType, HashSet<ushort>>
            {
                {ItemType.Weapon, new HashSet<ushort>()},
                {ItemType.Ability, new HashSet<ushort>()},
                {ItemType.Armor, new HashSet<ushort>()},
                {ItemType.Ring, new HashSet<ushort>()},
                {ItemType.StatPot, new HashSet<ushort>()},
                {ItemType.Other, new HashSet<ushort>()}
            };

            PopulateLists(_dbMarket.GetAll());

            Log.Info("Player market an initialized...");
        }

        private static readonly object MarketLock = new object();
        
        public void Add(PlayerShopItem shopItem, ITransaction transaction = null)
        {
            var trans = transaction ?? _manager.Database.Conn.CreateTransaction();

            if (!ValidateShopItem(shopItem))
                return;

            var task = _dbMarket
                .InsertAsync(shopItem, trans)
                .ContinueWith(t =>
                {
                    using (TimedLock.Lock(MarketLock))
                    {
                        if (t.IsCanceled || !t.Result)
                            return false;

                        var index = AddShopItem(shopItem);
                        if (index != 0)
                            return true;

                        var merchant = _merchants
                            .FirstOrDefault(m => m.Value.Item == shopItem.ItemId);
                        if (merchant.Value != null)
                        {
                            merchant.Value.TimeLeft = 30000;
                            merchant.Value.Reload();
                            return true;
                        }

                        var itemType = GetItemType(shopItem);

                        var shopItems = _items[itemType];
                        using (TimedLock.Lock(shopItems))
                        {
                            if (!shopItems.Contains(shopItem.ItemId))
                            {
                                shopItems.Add(shopItem.ItemId);
                                _queues[itemType].Enqueue(shopItem.ItemId);
                            }
                        }

                        if (_merchants.Count < _shopTypeLocations.Select(s => s.Value.Count).Sum())
                            AddMerchants();
                        return true;
                    }
                });

            task.ContinueWith(e =>
                Log.Error(e.Exception.InnerException.ToString()),
                TaskContinuationOptions.OnlyOnFaulted);

            if (transaction == null)
                trans.Execute(CommandFlags.FireAndForget);
        }

        public void Remove(PlayerShopItem shopItem, ITransaction transaction = null)
        {
            var trans = transaction ?? _manager.Database.Conn.CreateTransaction();

            var task = _dbMarket
                .RemoveAsync(shopItem, trans)
                .ContinueWith(t =>
                {
                    using (TimedLock.Lock(MarketLock))
                    {
                        var success = !t.IsCanceled && t.Result;
                        if (success)
                        {
                            var itemList = GetMarket(shopItem)[shopItem.ItemId];
                            using (TimedLock.Lock(itemList))
                            {
                                itemList.Remove(shopItem);
                            }

                            var merchant = _merchants.Values.FirstOrDefault(m => m.PlayerShopItem == shopItem);
                            merchant?.Reload();
                        }

                        return success;
                    }
                });

            task.ContinueWith(e =>
                Log.Error(e.Exception.InnerException.ToString()),
                TaskContinuationOptions.OnlyOnFaulted);

            if (transaction == null)
                trans.Execute(CommandFlags.FireAndForget);
        }

        public PlayerShopItem GetShopItem(uint id)
        {
            return _dbMarket.GetById(id);
        }

        public PlayerShopItem[] GetItems(Player player)
        {
            return _dbMarket.GetAll()
                .Where(e => e.AccountId == player.AccountId)
                .ToArray();
        }
        
        public void Reload(PlayerMerchant merchant)
        {
            using (TimedLock.Lock(MarketLock))
            {
                if (!ValidateShopItem(merchant.PlayerShopItem))
                {
                    RemoveMerchant(merchant);
                    return;
                }

                var itemType = GetItemType(merchant.PlayerShopItem);
                var market = _shops[itemType];
                var itemCount = market.ContainsKey(merchant.Item) ?
                    market[merchant.Item].Count :
                    0;

                ushort nextItem;
                if (merchant.TimeLeft <= 0 || itemCount <= 0)
                {
                    if (itemCount > 0)
                        _queues[itemType].Enqueue(merchant.Item);
                    else
                        _items[itemType].Remove(merchant.Item);

                    if (!_queues[itemType].TryDequeue(out nextItem))
                    {
                        RemoveMerchant(merchant);
                        return;
                    }
                    merchant.TimeLeft = 30000;
                }
                else
                    nextItem = merchant.Item;

                PlayerShopItem nextShopItem;
                var shop = market[nextItem];
                using (TimedLock.Lock(shop))
                {
                    nextShopItem = shop[0];
                }
                merchant.PlayerShopItem = nextShopItem;
                merchant.Item = nextShopItem.ItemId;
                merchant.Price = nextShopItem.Price;
                merchant.Count = shop.Count;
            }
        }

        public void InitMarketplace(World marketplace)
        {
            using (TimedLock.Lock(MarketLock))
            {
                _marketplace = marketplace;
                InitShopLocations();
                AddMerchants();
            }
        }

        private void InitShopLocations()
        {
            var regionOffset = new Dictionary<ItemType, int>();
            foreach (var region in _marketplace.Map.Regions)
            {
                var itemType = GetItemType(region.Value);
                if (itemType == ItemType.None)
                    continue;

                if (!_shopTypeLocations.ContainsKey(itemType))
                    _shopTypeLocations[itemType] = new ConcurrentDictionary<IntPoint, int>();

                if (!regionOffset.ContainsKey(itemType))
                    regionOffset.Add(itemType, 0);
                regionOffset[itemType] += 500;

                _shopTypeLocations[itemType].TryAdd(region.Key, regionOffset[itemType]);
            }
        }

        private void AddMerchants()
        {
            foreach (var shopLocs in _shopTypeLocations)
                foreach (var shopLoc in shopLocs.Value)
                {
                    if (_merchants.ContainsKey(shopLoc.Key))
                        continue;

                    ushort objectType;
                    if (!_queues[shopLocs.Key].TryDequeue(out objectType))
                        continue;
                        
                    var itemList = _shops[shopLocs.Key][objectType];

                    PlayerShopItem item;
                    using (TimedLock.Lock(itemList))
                    {
                        item = itemList[0];
                    }

                    var m = new PlayerMerchant(_manager, 0x01ca)
                    {
                        PlayerShopItem = item,
                        Item = item.ItemId,
                        Price = item.Price,
                        Currency = CurrencyType.Fame,
                        Count = itemList.Count,
                        //TimeLeft = Rand.Next(30000, 90000),
                        ReloadOffset = shopLoc.Value
                    };

                    _merchants.TryAdd(shopLoc.Key, m);
                    m.Move(shopLoc.Key.X + .5f, shopLoc.Key.Y + .5f);
                    _marketplace.EnterWorld(m);
                }
        }

        private void PopulateLists(IEnumerable<PlayerShopItem> shopItems)
        {
            foreach (var shopItem in shopItems)
                AddShopItem(shopItem);

            foreach (var market in _shops)
            {
                var items = market.Value.Keys.ToList();
                items.Shuffle();
                foreach (var item in items)
                {
                    _items[market.Key].Add(item);
                    _queues[market.Key].Enqueue(item);
                }
            }
        }

        private int AddShopItem(PlayerShopItem shopItem)
        {
            if (!ValidateShopItem(shopItem))
            {
                Log.Warn($"Invalid PlayerShopItem. {shopItem.AccountId}'s item ({shopItem.ItemId}) will not be merched.");
                return -1;
            }

            var market = GetMarket(shopItem);
            if (!market.ContainsKey(shopItem.ItemId))
                market.TryAdd(shopItem.ItemId, new List<PlayerShopItem>());
            return PlaceShopItem(market[shopItem.ItemId], shopItem);
        }
        
        private void RemoveMerchant(PlayerMerchant merchant)
        {
            PlayerMerchant dummy;
            _merchants.TryRemove(new IntPoint((int)merchant.X, (int)merchant.Y), out dummy);

            merchant.Owner?.LeaveWorld(merchant);
        }
        
        private bool ValidateShopItem(PlayerShopItem shopItem)
        {
            if (shopItem == null ||
                !_manager.Resources.GameData.Items.ContainsKey(shopItem.ItemId) ||
                shopItem.Price < 0 ||
                shopItem.AccountId <= 0)
                return false;

            return true;
        }

        private int PlaceShopItem(List<PlayerShopItem> shopItemList, PlayerShopItem shopItem)
        {
            using (TimedLock.Lock(shopItemList))
            {
                for (var i = 0; i < shopItemList.Count; i++)
                {
                    if (shopItemList[i].Price < shopItem.Price)
                        continue;

                    if (shopItemList[i].Price > shopItem.Price)
                    {
                        shopItemList.Insert(i, shopItem);
                        return i;
                    }

                    if (shopItemList[i].InsertTime > shopItem.InsertTime)
                    {
                        shopItemList.Insert(i, shopItem);
                        return i;
                    }
                }

                shopItemList.Add(shopItem);
                return shopItemList.Count - 1;
            }
        }

        private ConcurrentDictionary<ushort, List<PlayerShopItem>> GetMarket(PlayerShopItem shopItem)
        {
            switch (GetItemType(shopItem))
            {
                case ItemType.Weapon:
                    return _weapons;
                case ItemType.Ability:
                    return _abilities;
                case ItemType.Armor:
                    return _armor;
                case ItemType.Ring:
                    return _rings;
                case ItemType.StatPot:
                    return _statPots;
                default:
                    return _other;
            }
        }

        private ItemType GetItemType(PlayerShopItem shopItem)
        {
            var gameData = _manager.Resources.GameData;
            var item = gameData.Items[shopItem.ItemId];

            if (item.Potion && item.ActivateEffects.Any(a => a.Effect == ActivateEffects.IncrementStat))
                return ItemType.StatPot;

            if (!gameData.SlotType2ItemType.ContainsKey(item.SlotType))
                return ItemType.Other;

            switch (gameData.SlotType2ItemType[item.SlotType])
            {
                case ItemType.Weapon:
                    return ItemType.Weapon;
                case ItemType.Ability:
                    return ItemType.Ability;
                case ItemType.Armor:
                    return ItemType.Armor;
                case ItemType.Ring:
                    return ItemType.Ring;
                default:
                    return ItemType.Other;
            }
        }

        private ItemType GetItemType(TileRegion region)
        {
            switch (region)
            {
                case TileRegion.Store_9:
                    return ItemType.Weapon;
                case TileRegion.Store_10:
                    return ItemType.Ability;
                case TileRegion.Store_11:
                    return ItemType.Armor;
                case TileRegion.Store_12:
                    return ItemType.Ring;
                case TileRegion.Store_13:
                    return ItemType.StatPot;
                case TileRegion.Store_14:
                    return ItemType.Other;
                default:
                    return ItemType.None;
            }
        }
    }
}
