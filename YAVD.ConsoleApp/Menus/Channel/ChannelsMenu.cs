using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVD.ConsoleApp.Actions;
using YAVD.ConsoleApp.Helpers;

namespace YAVD.ConsoleApp.Menus
{
    public static class ChannelsMenu
    {
        public static async Task Show()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Kanal Yönetimi ===");
                Console.WriteLine("1) Kanal Ekle/Düzenle/Sil");
                Console.WriteLine("2) Kanal Listesi");
                Console.WriteLine("3) Video Listesi");
                Console.WriteLine("4) Kanalların Videolarını Kontrol Et");
                Console.WriteLine("0) Ana Menüye Dön");
                Console.Write("\nSeçiminiz: ");
                string choice = Console.ReadLine();

                if (choice == "1") await ChannelModifyMenu.Show();
                else if (choice == "2")
                {
                    var list = await ChannelActions.GetChannels("Listeleme");
                    if (list != null)
                    {
                        ChannelActions.PrintChannels(list);
                        ConsoleHelper.WaitForKey();
                    }
                }
                else if (choice == "3") await VideoActions.GetVideos();
                else if (choice == "4") await ChannelActions.CheckVideosMenu();
                else if (choice == "0") break;
            }
        }
    }
}
