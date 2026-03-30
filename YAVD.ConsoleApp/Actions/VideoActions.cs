using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVD.ConsoleApp.Helpers;
using YAVD.Core.Data;
using YAVD.Core.Models;

namespace YAVD.ConsoleApp.Actions
{
    public static class VideoActions
    {
        public static async Task GetVideos()
        {
            using var db = new YAVDContext();

            if (!await db.Videos.AnyAsync())
            {
                Console.WriteLine("\n[UYARI] Henüz veritabanında kayıtlı bir video bulunamadı.");

                ConsoleHelper.WaitForKey();
                return;
            }

            while (true)
            {
                var channels = await db.Channels.ToListAsync();
                var channelData = new List<(Channel channel, int videoCount)>();

                foreach (var c in channels)
                {
                    int count = await db.Videos.CountAsync(v => v.ChannelId == c.Id);
                    channelData.Add((c, count));
                }

                channelData = channelData.OrderByDescending(x => x.channel.LastVideoDate).ToList();
                ChannelActions.PrintChannelsWithCount(channelData);

                Console.Write("\nVideolarını listelemek istediğiniz Kanal ID (0 : Geri): ");

                if (!int.TryParse(Console.ReadLine(), out int channelId) || channelId == 0) break;

                var selectedChannel = channels.FirstOrDefault(c => c.Id == channelId);
                if (selectedChannel == null)
                {
                    Console.WriteLine("[HATA] Geçersiz Kanal ID.");

                    ConsoleHelper.WaitForKey();
                    continue;
                }

                var videos = await db.Videos
                    .Where(v => v.ChannelId == channelId)
                    .OrderByDescending(v => v.PublishedAt)
                    .ToListAsync();

                if (!videos.Any())
                {
                    Console.WriteLine($"\n[BİLGİ] {selectedChannel.Name} kanalına ait henüz video kaydı yok.");


                    ConsoleHelper.WaitForKey();
                    continue;
                }

                await ShowVideoSubMenu(selectedChannel, videos);
            }
        }
        private static async Task ShowVideoSubMenu(Core.Models.Channel channel, List<Video> videos)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== {channel.Name} - Video Listesi ===");


                Console.WriteLine("{0,-5} {1,-40} {2,-15} {3,-12}", "ID", "Video Başlığı", "Tarih", "Durum");


                Console.WriteLine(new string('-', 80));

                foreach (var v in videos)
                {
                    string status = v.IsDownloaded ? "[İNDİRİLDİ]" : "[BEKLEMEDE]";
                    string title = v.Title.Length > 37 ? v.Title.Substring(0, 37) + "..." : v.Title;
                    Console.WriteLine("{0,-5} {1,-40} {2,-15} {3,-12}", v.Id, title, v.PublishedAt.ToString("yyyy-MM-dd"), status);
                }

                Console.Write("\nİşlem yapmak istediğiniz Video ID (0 : Geri): ");
                if (!int.TryParse(Console.ReadLine(), out int videoId) || videoId == 0) break;

                var video = videos.FirstOrDefault(v => v.Id == videoId);
                if (video == null)
                {
                    Console.WriteLine("[HATA] Geçersiz Video ID."); ConsoleHelper.WaitForKey(); continue;
                }

                Console.WriteLine($"\nSeçili Video: {video.Title}");
                Console.WriteLine("1) Yeniden İndir (Mevcut Ayarlar ile)");
                Console.WriteLine("2) Kaydı Veritabanından Sil");
                Console.WriteLine("0) Geri");
                Console.Write("\nSeçiminiz: ");

                string act = Console.ReadLine();

                if (act == "1")
                {
                    await RedownloadVideo(channel, video);
                }
                else if (act == "2")
                {
                    await DeleteVideoRecord(video);
                    videos.Remove(video);
                    if (!videos.Any()) break;
                }
                else if (act == "0") break;
            }
        }
        private static async Task RedownloadVideo(Core.Models.Channel channel, Video video)
        {
            using var db = new YAVDContext();

            var dirSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultDownloadDirectory");
            var actionSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultDownloadAction");
            var resSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultVideoResolution");
            var audioSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultAudioQuality");

            string folder = Path.GetFullPath(dirSetting?.Value ?? ".\\Downloads");

            DownloadAction action = Enum.TryParse(actionSetting?.Value, out DownloadAction a) ? a : DownloadAction.AudioOnly;
            VideoResolution res = Enum.TryParse(resSetting?.Value, out VideoResolution r) ? r : VideoResolution.P1080;
            AudioQuality audio = Enum.TryParse(audioSetting?.Value, out AudioQuality q) ? q : AudioQuality.Medium;

            Console.WriteLine("\n[BİLGİ] İndirme işlemi başlatılıyor...");
            try
            {
                await DownloadActions.ExecuteDownload(video.YoutubeId, video.Title, channel.Name, video.PublishedAt, folder, action, res, audio, 1, 1);

                video.IsDownloaded = true;
                db.Videos.Update(video);
                await db.SaveChangesAsync();
                
                Console.WriteLine("\n[BAŞARILI] Video yeniden indirildi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[HATA] {ex.Message}");
            }

            ConsoleHelper.WaitForKey();
        }
        private static async Task DeleteVideoRecord(Video video)
        {
            Console.Write($"\n'{video.Title}' kaydını silmek istediğinize emin misiniz? (E/H): ");
            if (Console.ReadLine()?.ToLower() == "e")
            {
                using var db = new YAVDContext();
                db.Videos.Remove(video);
                await db.SaveChangesAsync();

                Console.WriteLine("[BAŞARILI] Video kaydı veritabanından silindi.");
                ConsoleHelper.WaitForKey();
            }
        }
    }
}
