
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebScraper
{
    class Program
    {
        private static DateTime? currentDateTime;
        private static Double currentPrice;
        private static Int64 currentVolume;
        private static bool keepRunning = true;
        private static bool newData = true;

        private static string folder = @"C:\temp\";
        private static string fileName = DateTime.Today.ToString("yyyy-MM-dd") + "_etsSMP.csv";
        private static string path = folder + fileName;

        static async Task Main(string[] args)
        {

            Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                Program.keepRunning = false;
            };

            while (Program.keepRunning)
            {   
              
                var html = await GetHtml();
                var data = ParseHtmlUsingHtmlAgilityPack(html);
                
                if (newData)
                {
                    latestPrice();
                    Console.Beep();
                    File.AppendAllText(path, data);
                }
                
                System.Threading.Thread.Sleep(1000 * 10);
            }

        }

        public static void latestPrice()
        {
            Console.WriteLine(currentDateTime + "," + currentPrice + "," + currentVolume + "\n");
        }

        private static Task<string> GetHtml()
        {
            var client = new HttpClient();
            return client.GetStringAsync("http://ets.aeso.ca/ets_web/ip/Market/Reports/CSMPriceReportServlet");
        }

        private static String ParseHtmlUsingHtmlAgilityPack(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);


            var rows =
                htmlDoc
                    .DocumentNode
                    .SelectNodes("//tr[contains(@bgcolor, 'd0f0ff')]");

            var date = "";
            var time = "";
            var price = new Double();
            var volume = new Int64();

            foreach (var rowdata in rows)
            {
                date = new String(rowdata.SelectSingleNode("td[1]").InnerText.Substring(0, 10));
                time = rowdata.SelectSingleNode("td[2]").InnerText;
                price =  Convert.ToDouble(rowdata.SelectSingleNode("td[3]").InnerText);
                volume = Convert.ToInt64(rowdata.SelectSingleNode("td[4]").InnerText);

                var newDateTime = DateTime.Parse(date + " " + time);

               
                if (newDateTime > currentDateTime || currentDateTime is null)
                    {
                        currentDateTime = newDateTime;
                        currentPrice = price;
                        currentVolume = volume;
                        newData = true;
                        

                } 
                else
                {
                        newData = false;
                }
                
                
            }

     
            return date + "," + time + "," + price + "," + volume + "\n";

        }
    }
}