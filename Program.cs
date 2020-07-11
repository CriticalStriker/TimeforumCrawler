using System;
using System.Collections.Generic;

namespace SeleniumCrawlerToTimeforum
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Selenium Crawler Start!");

            string marketId = string.Empty;
            string marketPW = string.Empty;

            if (args.Length < 1)
            {
                Console.Write("ID: ");
                marketId = Console.ReadLine();
            }
            else
            {
                marketId = args[0];
            }

            if (args.Length < 2)
            {
                Console.Write("PW: ");
                marketPW = Console.ReadLine();
            }
            else
            {
                marketPW = args[1];
            }

            int maxPageCount = 1;
            if (args.Length >= 3)
            {
                maxPageCount = System.Convert.ToInt32(args[2]);
            }

            string botId = args[3];
            string chatId = args[4];

            Console.WriteLine("Arguments: {0}, {1}, {2}, {3}, {4}", marketId, marketPW, maxPageCount, botId, chatId);

            MarketFinder finder = new TimeforumMarketFinder(marketId, marketPW);

            finder.messageSender = new TelegramMessageSender(botId, chatId);
            finder.ItemPageMaxCount = maxPageCount;

            Crawler crawler = new Crawler();

            crawler.Run(finder);
        }
    }
}
