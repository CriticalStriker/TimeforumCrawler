using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

//Selenium Library
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.ComponentModel.DataAnnotations;

namespace SeleniumCrawlerToTimeforum
{
    interface MarketFinder
    {
        string URL
        {
            get;
        }

        string ID
        {
            get;
        }

        string PW
        {
            get;
        }

        int ItemPageMaxCount
        {
            set;
        }

        MessageSender messageSender
        {
            set;
        }

        void Login(ChromeDriver driver);
        void Collect(ChromeDriver driver, MarketItemStorage storage);
    }

    class TimeforumMarketFinder : MarketFinder
    {
        private readonly string ItemBrand = "Rolex";
        private readonly string ItemStatus = "진행";
        private readonly string ItemSellType = "판매";

        private readonly string MarketSellByMemberUrlFormatText = "https://www.timeforum.co.kr/index.php?mid=BuyMarket&category=6114906&page={0}";

        private string _marketID = string.Empty;
        private string _marketPW = string.Empty;

        private readonly int ItemPageMaxLimit = 5;
        private int _itemPageMaxCount = 1;
        public int ItemPageMaxCount
        { 
            set
            {
                _itemPageMaxCount = System.Math.Min(value, ItemPageMaxLimit);
            }
        }

        private MessageSender _messageSender = null;
        public MessageSender messageSender
        {
            set
            {
                _messageSender = value;
            }
        }

        public string URL
        {
            get
            {
                return "https://www.timeforum.co.kr";
            }
        }


        public string ID
        {
            get
            {
                return _marketID;
            }
        }

        public string PW
        {
            get
            {
                return _marketPW;
            }
        }

        public TimeforumMarketFinder(string id, string pw)
        {
            _marketID = id;
            _marketPW = pw;
        }

        public void Login(ChromeDriver driver)
        {
            driver.Navigate().GoToUrl(this.URL);

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            var element = driver.FindElementByXPath("//*[@id='forum-nav']/div/div[2]/ul/li[2]");
            element.Click();

            Thread.Sleep(1000);

            element = driver.FindElementByXPath("//*[@id='uid']");
            element.SendKeys(this.ID);

            element = driver.FindElementByXPath("//*[@id='upw']");
            element.SendKeys(this.PW);

            element = driver.FindElementByXPath("//*[@id='fo_member_login']/fieldset/div[2]/input");
            element.Click();

            Thread.Sleep(1000);
        }

        
        protected bool IsMatchBrand(string brand)
        {
            return brand == this.ItemBrand;
        }

        protected bool IsMatchStatus(string status)
        {
            return status == this.ItemStatus;
        }

        protected bool IsMatchItemSellType(string type)
        {
            return type.Contains(this.ItemSellType);
        }

        protected string AttrBrand(ReadOnlyCollection<IWebElement> elements)
        {
            return elements[2].Text;
        }

        protected string AttrStatus(ReadOnlyCollection<IWebElement> elements)
        {
            return elements[3].Text;
        }

        protected string AttrSellType(ReadOnlyCollection<IWebElement> elements)
        {
            return elements[4].Text;
        }
        protected MarketItem[] CollectItemRecord(IWebElement element, MarketItemStorage storage)
        {
            var children = element.FindElements(By.TagName("tr"));

            var items = children.Where((child) => string.IsNullOrEmpty(child.GetAttribute("class")));

            int maxItemNo = storage.FindLastItemMarketNo();

            List<MarketItem> marketItems = new List<MarketItem>();
            foreach (var item in items)
            {   
                var marketItem = CollectItemInformation(item);
                if (marketItem != null)
                {
                    if (marketItem.No > 0 && marketItem.No <= maxItemNo)
                    {
                        Console.WriteLine("{0} is max item No.", marketItem.No);
                        break;
                    }

                    marketItems.Add(marketItem);
                }
            }

            return marketItems.ToArray();

            
        }

        protected MarketItem CollectItemInformation(IWebElement element)
        {
            var children = element.FindElements(By.TagName("td"));

            string brandName = AttrBrand(children);
            string itemStatus = AttrStatus(children);
            string itemSellType = AttrSellType(children);

            bool brandMatch = IsMatchBrand(brandName);
            bool statucMatch = IsMatchStatus(itemStatus);
            bool sellTypeMatch = IsMatchItemSellType(itemSellType);

            if (brandMatch && statucMatch && sellTypeMatch)
            {
                int number = 0;
                string price = string.Empty;
                string title = string.Empty;

                foreach (var child in children)
                {
                    string className = child.GetAttribute("class");
                    switch (className)
                    {
                        case "no":
                            {
                                number = System.Convert.ToInt32(child.Text);
                            }
                            break;

                        case "title":
                            {
                                title = child.Text;
                            }
                            break;
                        default:
                            {
                                string attrStyle = child.GetAttribute("style");
                                if (string.IsNullOrEmpty(attrStyle) == false)
                                {
                                    price = child.Text;
                                }
                            }
                            break;
                    }
                }

                Console.WriteLine("NO: {0} | Brand: {1} | Status: {2} | Type: {3} | Title: {4} | Price: {5}",
                    number, brandName, itemStatus, itemSellType, title, price);

                return new MarketItem()
                {
                    No = number,
                    Brand = brandName,
                    Title = title,
                    Price = price
                };
            }

            return null;
        }

        public void Collect(ChromeDriver driver, MarketItemStorage storage)
        {
            List<MarketItem> newItems = new List<MarketItem>();
            for (int i = 1; i < _itemPageMaxCount; ++i)
            {
                string collectUrl = string.Format(this.MarketSellByMemberUrlFormatText, i);

                driver.Navigate().GoToUrl(collectUrl);

                var element = driver.FindElementByXPath("//*[@id='board_list']/table/tbody");

                var items = CollectItemRecord(element, storage);

                newItems.AddRange(items);

                Thread.Sleep(500);
            }

            newItems.Sort((lhs, rhs) => lhs.No.CompareTo(rhs.No));

            foreach (var marketItem in newItems)
            {
                storage.SaveItem(marketItem);

                _messageSender.SendMessage(marketItem.ToString());
            }
        }
    }
}
