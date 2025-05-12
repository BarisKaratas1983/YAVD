using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVDConsoleApp.Methods;
using YAVDCore;

namespace YAVDConsoleApp
{
    public class Menu
    {
        private YAVD yavd;
        public Menu()
        {
            yavd = new YAVD();

            bool show = true;

            while (show)
            {
                show = Show();
            }
        }
        public bool Show()
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
    }
}