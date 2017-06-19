using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using StackExchange.Redis;

namespace common.resources
{
    public enum MBoxPurchaseResult
    {
        Success,
        InvalidCurrency,
        NotEnoughGold,
        NotEnoughFame
    }

    public class MBoxPurchase
    {
        public MBoxPurchaseResult Result { get; set; }
        public ushort[] Awards { get; set; }
        public string Xml { get; set; }
    }

    public class MBoxCost
    {
        public int Amount => SaleEndTime < DateTime.UtcNow ? NormalPrice : SalePrice;
        
        public int NormalPrice { get; set; }
        public int SalePrice { get; set; }
        public DateTime SaleEndTime { get; set; }
        public int Currency { get; set; }
    }

    public class MysteryBox
    {
        private static readonly Random Rand = new Random();

        public int Id { get; set; }
        public string Title { get; set; }
        public int Weight { get; set; }
        public string Description { get; set; }
        public ushort[][] Contents { get; set; }
        public string Image { get; set; }
        public string Icon { get; set; }
        public MBoxCost Cost { get; set; }
        public DateTime StartTime { get; set; }

        public MBoxPurchase Purchase(Database db, DbAccount acc, out ITransaction tran)
        {
            var purchase = new MBoxPurchase
            {
                Result = MBoxPurchaseResult.Success,
                Awards = Contents.Select(t => t[Rand.Next(0, t.Length)]).ToArray()
            };

            tran = db.Conn.CreateTransaction();
            switch (Cost.Currency)
            {
                case 0:
                    if (acc.Credits < Cost.Amount)
                        purchase.Result = MBoxPurchaseResult.NotEnoughGold;
                    else
                        db.UpdateCurrency(acc, -Cost.Amount, CurrencyType.Gold, tran);
                    break;
                case 1:
                    if (acc.Fame < Cost.Amount)
                        purchase.Result = MBoxPurchaseResult.NotEnoughFame;
                    else
                        db.UpdateCurrency(acc, -Cost.Amount, CurrencyType.Fame, tran);
                    break;
                default:
                    purchase.Result = MBoxPurchaseResult.InvalidCurrency;
                    break;
            }

            purchase.Xml =
                (new XElement("Success",
                    new XElement("Awards", purchase.Awards.ToCommaSepString()),
                    new XElement(Cost.Currency == 0 ? "Gold" : "Fame",
                                 Cost.Currency == 0 ? acc.Credits - Cost.Amount : 
                                                      acc.Fame - Cost.Amount))).ToString();

            return purchase;
        }
    }

    public class MysteryBoxes
    {
        private readonly Dictionary<int, MysteryBox> _boxes;

        public MysteryBoxes()
        {
            _boxes = new Dictionary<int, MysteryBox>();
        }

        public void Load(string path)
        {
            var data = File.ReadAllText(path);
            var root = XElement.Parse(data);
            foreach (var elem in root.XPathSelectElements("//MysteryBox"))
                AddMysteryBox(elem);
        }

        private void AddMysteryBox(XElement elem)
        {
            var mBox = new MysteryBox()
            {
                Id = elem.Attribute("id").Value.ToInt32(),
                Title = elem.Attribute("title").Value,
                Weight = elem.Attribute("weight").Value.ToInt32(),
                Description = elem.Element("Description")?.Value,
                Image = elem.Element("Image")?.Value,
                Icon = elem.Element("Icon")?.Value,
                StartTime =
                    DateTime.ParseExact(elem.Element("StartTime")?.Value, "yyyy-MM-dd HH:mm:ss",
                        CultureInfo.InvariantCulture)
            };

            // get contents
            var awards = elem.Element("Contents")?.Value.Split(';');
            mBox.Contents = new ushort[awards?.Length ?? 0][];
            for (var i = 0; i < mBox.Contents.Length; i++)
                mBox.Contents[i] = awards[i].CommaToArray<ushort>();
            
            // init costs
            mBox.Cost = new MBoxCost()
            {
                NormalPrice = elem.Element("Price").Attribute("amount").Value.ToInt32(),
                Currency = elem.Element("Price").Attribute("currency").Value.ToInt32(),
                SaleEndTime = DateTime.MinValue
            };
            if (elem.Element("Sale") != null)
            {
                mBox.Cost.SalePrice = elem.Element("Sale").Attribute("price").Value.ToInt32();
                mBox.Cost.SaleEndTime = DateTime.ParseExact(elem.Element("Sale").Element("End").Value,
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture);
            }

            _boxes.Add(mBox.Id, mBox);
        }

        public MysteryBox this[int index]
        {
            get
            {
                if (!_boxes.ContainsKey(index))
                    return null;
                
                var box = _boxes[index];
                return box.StartTime > DateTime.UtcNow ? null : box;
            }
        } 
    }
}
