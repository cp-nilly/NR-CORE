using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using log4net;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace common.resources
{
    public enum PkgPurchaseResult
    {
        Success,
        InvalidCurrency,
        NotEnoughGold,
        NotEnoughFame,
        TransactionFailed,
        Error,
        QuantityZero,
        MaxedPurchase
    }

    public struct PackageItem
    {
        public ushort Item { get; set; }
        public int Count { get; set; }    
    }

    public struct PackageContents
    {
        public int CharacterSlots { get; set; }
        public int VaultChests { get; set; }
        public PackageItem[] Items { get; set; }
    }

    public class PackageCost
    {
        public int Amount { get; private set; }
        public int Currency { get; private set; }

        public PackageCost(XElement price)
        {
            Amount = price.Value.ToInt32();
            Currency = price.Attribute("currency").Value.ToInt32();
        }
    }

    public class Package
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Package));
        
        public int Id { get; private set; }
        public string Name { get; private set; }
        public PackageCost Cost { get; private set; }
        public int Quantity { get; private set; }
        public int MaxPurchase { get; private set; }
        public int Weight { get; private set; }
        public string BgURL { get; private set; }
        public DateTime EndDate { get; private set; }
        public PackageContents Contents { get; private set; }

        private string _key;

        public Package(XElement package)
        {
            Id = package.Attribute("id").Value.ToInt32();
            Name = package.Element("Name").Value;
            Cost = new PackageCost(package.Element("Price"));
            Quantity = package.Element("Quantity").Value.ToInt32();
            MaxPurchase = package.Element("MaxPurchase").Value.ToInt32();
            Weight = package.Element("Weight").Value.ToInt32();
            BgURL = package.Element("BgURL").Value;
            EndDate = DateTime.ParseExact(
                package.Element("EndDate").Value, 
                "MM/dd/yyyy HH:mm:ss 'GMT'K",
                CultureInfo.InvariantCulture);
            Contents = JsonConvert.DeserializeObject<PackageContents>(package.Element("Contents").Value);
            
            _key = $"package.{Id}";
        }

        public async Task<PkgPurchaseResult> Purchase(Database db, DbAccount acc)
        {
            var tran = db.Conn.CreateTransaction();
            
            // handle quantity and max purchase checks
            ConditionResult quantityResult = null;
            ConditionResult maxPurchaseResult = null;
            if (Quantity > -1)
                quantityResult = tran.AddCondition(Condition.HashNotEqual(_key, "amountPurchased", Quantity));
            tran.HashIncrementAsync(_key, "amountPurchased");
            if (MaxPurchase > -1)
                maxPurchaseResult = tran.AddCondition(Condition.HashNotEqual(_key, acc.AccountId, MaxPurchase));
            tran.HashIncrementAsync(_key, acc.AccountId);

            // deduct cost
            switch (Cost.Currency)
            {
                case 0:
                    if (acc.Credits < Cost.Amount)
                        return PkgPurchaseResult.NotEnoughGold;
                    db.UpdateCurrency(acc, -Cost.Amount, CurrencyType.Gold, tran);
                    break;
                case 1:
                    if (acc.Fame < Cost.Amount)
                        return PkgPurchaseResult.NotEnoughFame;
                    db.UpdateCurrency(acc, -Cost.Amount, CurrencyType.Fame, tran);
                    break;
                default:
                    return PkgPurchaseResult.InvalidCurrency;
            }

            // save gifts
            var items = new List<ushort>();
            foreach (var pkgItem in Contents.Items ?? new PackageItem[0])
                items.AddRange(Enumerable.Repeat(pkgItem.Item, pkgItem.Count));
            db.AddGifts(acc, items, tran);

            // initiate transaction
            var t1 = tran.ExecuteAsync();

            // upon success add character slots / vault chests
            var t2 = t1.ContinueWith(t =>
            {
                var success = !t.IsCanceled && t.Result;
                if (!success)
                {
                    if (quantityResult != null && !quantityResult.WasSatisfied)
                        return PkgPurchaseResult.QuantityZero;
                    if (maxPurchaseResult != null && !maxPurchaseResult.WasSatisfied)
                        return PkgPurchaseResult.MaxedPurchase;
                    return PkgPurchaseResult.TransactionFailed;
                }
                
                if (Contents.CharacterSlots > 0)
                    acc.MaxCharSlot += Contents.CharacterSlots;

                if (Contents.VaultChests > 0)
                    acc.VaultCount += Contents.VaultChests;

                acc.FlushAsync();
                return PkgPurchaseResult.Success;
            });

            // await tasks
            try
            {
                await Task.WhenAll(t1, t2);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return PkgPurchaseResult.Error;
            }
            
            return t2.Result;
        }
    }

    public class Packages
    {
        private readonly Dictionary<int, Package> _packages;

        public Packages()
        {
            _packages = new Dictionary<int, Package>();
        }

        public void Load(string path)
        {
            var data = File.ReadAllText(path);
            var root = XElement.Parse(data);
            foreach (var elem in root.XPathSelectElements("//Package"))
            {
                var pkg = new Package(elem);
                _packages.Add(pkg.Id, pkg);
            }
        }

        public Package this[int index]
        {
            get
            {
                if (!_packages.ContainsKey(index))
                    return null;

                var pkg = _packages[index];
                return pkg.EndDate < DateTime.UtcNow ? null : pkg;
            }
        }
    }
}
