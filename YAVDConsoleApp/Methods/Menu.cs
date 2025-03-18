using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVDCore;
using YAVDCore.Methods;

namespace YAVDConsoleApp.Methods
{
    public class Menu
    {
        public static bool Show()
        {
            Console.Clear();
            Console.WriteLine("YAVD Main Menu");
            Console.WriteLine("==================================");
            Console.WriteLine("1) Add Channel From Video Url");
            Console.WriteLine("2) Channel List");
            Console.WriteLine("3) Check Videos");
            Console.WriteLine("4) Exit");
            Console.Write("\r\nChoice: ");

            switch (Console.ReadLine())
            {
                case "1":
                    //await AddChannelFromVideoUrl();
                    return true;
                case "2":
                    ChannelList();
                    return true;
                case "3":
                    //await CheckVideos();
                    return true;
                case "4":
                    return false;
                default:
                    return true;
            }
        }
        public static void AddChannelFromVideoUrl()
        {

        }
        public static void ChannelList()
        {
            YAVD y = new YAVD();

            y.Channels.RefreshChannels();
            Console.WriteLine("Channel Count : {0}", y.Channels.Items.Count);

            foreach (var channel in y.Channels.Items)
            {
                Console.WriteLine("Title : {0}");
            }
        }
    }
}
