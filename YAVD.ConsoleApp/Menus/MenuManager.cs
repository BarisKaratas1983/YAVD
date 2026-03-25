using YAVD.ConsoleApp.Actions;
using YAVD.Core.Models;

namespace YAVD.ConsoleApp.Menus
{
    public static class MenuManager
    {        
        public static async Task DirectDownloadMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Direkt Video/Ses İndirme ===");
            Console.Write("\nYouTube videosunun veya playlistin linkini girin: ");
            string url = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(url)) return;

            var playlistId = YoutubeExplode.Playlists.PlaylistId.TryParse(url);
            var videoId = YoutubeExplode.Videos.VideoId.TryParse(url);

            if (playlistId != null)
            {
                Console.WriteLine("[TESPİT] Çalma listesi (Playlist) algılandı.");
                await DownloadActions.HandleDirectDownload(url, true);
            }
            else if (videoId != null)
            {
                Console.WriteLine("[TESPİT] Tekil video algılandı.");
                await DownloadActions.HandleDirectDownload(url, false);
            }
            else
            {
                Console.WriteLine("\n[HATA] Geçersiz YouTube linki!");
                ChannelActions.WaitForKey();
            }
        }        
        public static async Task ChannelsMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Kanal Yönetimi ===");
                Console.WriteLine("1) Kanal Ekle/Düzenle/Sil");
                Console.WriteLine("2) Kanal Listesi");
                Console.WriteLine("3) Kanalların Videolarını Kontrol Et (Yakında)");
                Console.WriteLine("0) Ana Menüye Dön");
                Console.Write("\nSeçiminiz: ");
                string choice = Console.ReadLine();

                if (choice == "1") await ChannelModifyMenu();
                else if (choice == "2")
                {
                    var list = await ChannelActions.GetChannelsOrWarn("Listeleme");
                    if (list != null)
                    {
                        ChannelActions.PrintChannels(list);
                        ChannelActions.WaitForKey();
                    }
                }
                else if (choice == "0") break;
            }
        }
        private static async Task ChannelModifyMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Kanal Ekle / Düzenle / Sil ===");
                Console.WriteLine("1) Kanal Ekle");
                Console.WriteLine("2) Kanal Düzenle (Aktif/Pasif)");
                Console.WriteLine("3) Kanal Sil");
                Console.WriteLine("0) Önceki Menüye Dön");
                Console.Write("\nSeçiminiz: ");
                string choice = Console.ReadLine();

                if (choice == "1") await ChannelActions.AddChannel();
                else if (choice == "2") await ChannelActions.ToggleChannelStatus();
                else if (choice == "3") await ChannelActions.DeleteChannel();
                else if (choice == "0") break;
            }
        }
    }
}