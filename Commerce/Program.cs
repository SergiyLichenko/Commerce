using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FoodMarket;
using log4net;
using System.IO;

namespace Commerce
{
    class Program
    {
        static void Main(string[] args)
        {
            if (File.Exists("../../Logs/logInfo.txt"))
                File.Delete("../../Logs/logInfo.txt");
            log4net.Config.XmlConfigurator.Configure();

            Console.WriteLine("Нажмите 'Enter' для открытия магазина"); 
            Console.ReadLine();

            Shop shop = new Shop();
            Thread shopThread = new Thread(shop.Start);
            shopThread.Name = "Shop Thread";
            shopThread.Start();


            Console.ReadKey();
            shop.Finish();

            Console.ReadKey();

        }
    }
}