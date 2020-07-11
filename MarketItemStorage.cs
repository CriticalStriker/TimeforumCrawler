using System;
using System.Collections.Generic;
using System.Text;

using LiteDB;

namespace SeleniumCrawlerToTimeforum
{
    class MarketItem
    {
        public int Id { get; set; }
        public int No { get; set; }
        public string Brand { get; set; }
        public string Title { get; set; }
        public string Price { get; set; }

        public override string ToString()
        {
            return string.Format("{0} / {1} / {2}", Brand, Title, Price);
        }
    }

    class MarketItemStorage
    {
        private string DBName = @"MarketItems.db";
        private string TableName = "market_items";

        public int FindLastItemMarketNo()
        {
            using (var db = new LiteDatabase(DBName))
            {
                bool exists = db.CollectionExists(TableName);
                if (exists)
                {
                    // Get a collection (or create, if doesn't exist)
                    var collection = db.GetCollection<MarketItem>(TableName);

                    var maxId = collection.Max();
                    var maxItem = collection.FindById(maxId);

                    return maxItem.No;
                }

                return 0;
            }
        }

        public bool SaveItem(MarketItem item)
        {
            using (var db = new LiteDatabase(DBName))
            {
                // Get a collection (or create, if doesn't exist)
                var collection = db.GetCollection<MarketItem>(TableName);

                collection.Insert(item);
            }

            return true;
        }

        public bool SaveItems(MarketItem[] items)
        {
            using (var db = new LiteDatabase(DBName))
            {
                // Get a collection (or create, if doesn't exist)
                var collection = db.GetCollection<MarketItem>(TableName);

                foreach (var item in items)
                {
                    collection.Upsert(item);
                }
            }

            return true;
        }
    }
}
