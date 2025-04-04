using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVDCore;
using YAVDCore.Methods;
using YAVDCore.Models;

namespace YAVDConsoleApp.Methods
{
    public class Menu
    {
        public static bool Show()
        {
            Console.Clear();
            Console.WriteLine("YAVD Main Menu");
            Console.WriteLine("==============");
            Console.WriteLine("1) Add Channel");
            Console.WriteLine("2) Channel List");
            Console.WriteLine("3) Check Videos");
            Console.WriteLine("4) Exit");
            Console.Write("\r\nChoice: ");

            switch (Console.ReadLine())
            {
                case "1":
                    AddChannelMenu();
                    return true;
                case "2":
                    ChannelList();
                    return true;
                case "3":                    
                    return true;
                case "4":
                    return false;
                default:
                    return true;
            }
        }
        private static void AddChannelMenu()
        {
            bool showChannelMenu = true;

            while (showChannelMenu)
            {
                Console.Clear();
                Console.WriteLine("1) Add From Video Url");
                Console.WriteLine("2) Add From Channel Url");
                Console.WriteLine("3) Back");
                Console.Write("\r\nChoice: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        AddChannelFromVideoUrl();
                        showChannelMenu = false;
                        break;
                    case "2":
                        AddChannelFromChannelUrl();
                        showChannelMenu = false;
                        break;
                    case "3":                        
                        showChannelMenu = false;
                        break;
                    default:
                        showChannelMenu = true;
                        break;
                }
            }
        }
        private static void AddChannelFromVideoUrl()
        {
            Console.Clear();
            Console.Write("Video link :");

            string videoLink = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(videoLink))
            {
                YAVD y = new YAVD();
                ChannelModel c = y.Channels.Methods.SaveChannelFromVideoLink(videoLink, y.ApiKey.Items.First(x => x.ApiKeyId == y.MainSettings.Item.ApiKeyId));
                Console.WriteLine();

                if (c != null)
                    Console.WriteLine("{0} added", c.Title);                   
            }

            Console.WriteLine("");
            Console.WriteLine("Press any key to return to the main menu...");
            Console.ReadKey();
        }
        private static void AddChannelFromChannelUrl()
        {
            Console.Clear();
            Console.Write("Channel link :");

            string channelLink = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(channelLink))
            {
                YAVD y = new YAVD();
                ChannelModel c = y.Channels.Methods.SaveChannelFromChannelLink(channelLink, y.ApiKey.Items.First(x => x.ApiKeyId == y.MainSettings.Item.ApiKeyId));
                Console.WriteLine();

                if (c != null)
                    Console.WriteLine("{0} added", c.Title);
            }

            Console.WriteLine("");
            Console.WriteLine("Press any key to return to the main menu...");
            Console.ReadKey();
        }
        private static void ChannelList()
        {
            Console.Clear();

            YAVD y = new YAVD();            

            Console.WriteLine("Channel Count : {0}", y.Channels.Items.Count);
            Console.WriteLine();

            foreach (var channel in y.Channels.Items)
            {
                Console.WriteLine("Name : {0} - Last Check : {1}", channel.Title, channel.LastCheckDateTime);
            }

            Console.WriteLine("");
            Console.WriteLine("Press any key to return to the main menu...");
            Console.ReadKey();
        }
    }
}
