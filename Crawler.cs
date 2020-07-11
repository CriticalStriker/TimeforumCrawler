using System;
using System.Collections.Generic;
using System.Text;

//Selenium Library
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumCrawlerToTimeforum
{
    class Crawler
    {
        protected ChromeDriverService _driverService = null;
        protected ChromeOptions _options = null;
        protected ChromeDriver _driver = null;

        protected MarketItemStorage _storage = new MarketItemStorage();

        public Crawler()
        {
            _driverService = ChromeDriverService.CreateDefaultService();
            _driverService.HideCommandPromptWindow = true;

            _options = new ChromeOptions();
            _options.AddArgument("disable-gpu");
            _options.AddArgument("headless");

            _driver = new ChromeDriver(_driverService, _options);
        }

        public void Run(MarketFinder finder)
        {
            finder.Login(_driver);

            finder.Collect(_driver, _storage);
        }

    }
}
