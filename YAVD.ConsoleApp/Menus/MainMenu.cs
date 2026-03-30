using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVD.ConsoleApp.Actions;

namespace YAVD.ConsoleApp.Menus
{
    public static class MainMenu
    {
        public static async Task Show()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== YAVD: Youtube Audio Video Downloader ===");
                Console.WriteLine("1. Direkt Video/Ses İndir");
                Console.WriteLine("2. Kanallar");
                Console.WriteLine("0. Çıkış");
                Console.Write("\nSeçiminiz: ");

                string choice = Console.ReadLine();

                if (choice == "1") await DirectDownloadMenu.Show();
                else if (choice == "2") await ChannelsMenu.Show();
                else if (choice == "0") return;
            }
        }
    }
}
