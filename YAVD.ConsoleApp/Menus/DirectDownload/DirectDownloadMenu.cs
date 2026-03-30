using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVD.ConsoleApp.Actions;
using YAVD.ConsoleApp.Helpers;

namespace YAVD.ConsoleApp.Menus
{
    public static class DirectDownloadMenu
    {
        public static async Task Show()
        {
            while (true)
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
                    ConsoleHelper.WaitForKey();                    
                }
            }
        }
    }
}
