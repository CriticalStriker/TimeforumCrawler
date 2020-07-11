using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.IO;

namespace SeleniumCrawlerToTimeforum
{
    interface MessageSender
    {
        bool SendMessage(string message);
    }

    class TelegramMessageSender : MessageSender
    {
        private readonly string TelegramMessageUrlFormat = "https://api.telegram.org/bot{0}/sendmessage?chat_id={1}&text={2}";

        private string _botId = string.Empty;
        private string _chatId = string.Empty;

        public TelegramMessageSender(string botId, string chatId)
        {
            _botId = botId;
            _chatId = chatId;
        }

        public bool SendMessage(string message)
        {
            if (string.IsNullOrEmpty(_botId) || string.IsNullOrEmpty(_chatId))
            {
                Console.WriteLine("Telegram botId or chatId is null.");
                return false;
            }

            string messageUrl = string.Format(TelegramMessageUrlFormat, _botId, _chatId, message);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(messageUrl);
            request.Method = "GET";
            request.Timeout = 30 * 1000; // 30 seconds

            string responseText = string.Empty;

            using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
            {
                HttpStatusCode status = resp.StatusCode;
                Console.WriteLine(status);

                Stream respStream = resp.GetResponseStream();
                using (StreamReader sr = new StreamReader(respStream))
                {
                    responseText = sr.ReadToEnd();
                }
            }

            Console.WriteLine(responseText);

            return true;
        }
    }
}
