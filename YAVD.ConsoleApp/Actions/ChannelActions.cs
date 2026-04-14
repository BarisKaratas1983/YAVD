using Microsoft.EntityFrameworkCore;
using System;
using YAVD.ConsoleApp.Helpers;
using YAVD.Core.Data;
using YAVD.Core.Helpers;
using YAVD.Core.Models;
using YAVD.Core.Services;

namespace YAVD.ConsoleApp.Actions
{
    public static class ChannelActions
    {
        private static readonly YoutubeManager _ytService = new YoutubeManager();
        public static async Task<List<Channel>?> GetChannels(string actionName)
        {
            using var db = new YAVDContext();
            var channels = await db.Channels.ToListAsync();

            if (!channels.Any())
            {
                Console.WriteLine($"\n[UYARI] {actionName} kayıtlı bir kanal bulunamadı.");
                ConsoleHelper.WaitForKey();
                return null;
            }
            return channels;
        }
        public static void PrintChannels(List<Channel> channels)
        {
            Console.Clear();
            Console.WriteLine("=== Kayıtlı Kanallar ===");
            Console.WriteLine("{0,-5} {1,-30} {2,-10} {3,-20}", "ID", "Kanal Adı", "Durum", "Son Video Tarihi");
            Console.WriteLine(new string('-', 65));

            foreach (var c in channels)
            {
                string status = c.Active ? "AKTİF" : "PASİF";
                Console.WriteLine("{0,-5} {1,-30} {2,-10} {3,-20}", c.Id, c.Name, status, c.LastVideoDate);
            }
            Console.WriteLine(new string('-', 65));
        }
        public static async Task ScanChannels(int? specificChannelId = null)
        {
            using var db = new YAVDContext();

            var includeShortsSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "IncludeShorts");
            var includeLiveSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "IncludeLiveStreams");
            var actionSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultDownloadAction");
            var resSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultVideoResolution");
            var audioSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultAudioQuality");
            var dirSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultDownloadDirectory");
            var lastCheckedSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "LastCheckedDate");

            bool includeShorts = includeShortsSetting?.Value == "1" || includeShortsSetting?.Value?.ToLower() == "true";
            bool includeLive = includeLiveSetting?.Value == "1" || includeLiveSetting?.Value?.ToLower() == "true";

            DownloadAction defaultAction = Enum.TryParse(actionSetting?.Value, out DownloadAction a) ? a : DownloadAction.AudioOnly;
            VideoResolution defaultRes = Enum.TryParse(resSetting?.Value, out VideoResolution r) ? r : VideoResolution.P1080;
            AudioQuality defaultAudio = Enum.TryParse(audioSetting?.Value, out AudioQuality q) ? q : AudioQuality.Medium;
            string downloadFolder = Path.GetFullPath(dirSetting?.Value ?? ".\\Downloads");

            Console.Clear();
            Console.WriteLine("=== Kanal Tarama Konfigürasyonu ===");
            Console.WriteLine($"Hedef Klasör  : {downloadFolder}");
            Console.WriteLine($"İşlem Tipi    : {defaultAction}");
            if (defaultAction != DownloadAction.AudioOnly) Console.WriteLine($"Video Kalite  : {defaultRes}p");
            if (defaultAction != DownloadAction.VideoOnly) Console.WriteLine($"Ses Kalite    : {defaultAudio}kbps");
            Console.WriteLine($"Shorts Dahil  : {(includeShorts ? "Evet" : "Hayır")}");
            Console.WriteLine($"Son Tarama    : {lastCheckedSetting?.Value ?? "Null"}");
            Console.WriteLine(new string('-', 60));

            if (!Directory.Exists(downloadFolder))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\n[UYARI] Klasör bulunamadı. Oluşturulsun mu? (E/H): ");
                Console.ResetColor();
                if (Console.ReadLine()?.ToLower() == "e") Directory.CreateDirectory(downloadFolder);
                else return;
            }

            var query = db.Channels.Where(c => c.Active);
            if (specificChannelId.HasValue) query = query.Where(c => c.Id == specificChannelId.Value);
            var channels = await query.ToListAsync();

            int totalProcessedVideos = 0;

            foreach (var channel in channels)
            {
                Console.WriteLine($"\r[*] {channel.Name} taranıyor...");
                var newVideos = await _ytService.GetNewVideosFromChannelAsync(channel.YoutubeId, channel.LastVideoDate, includeShorts, includeLive);

                if (newVideos.Any())
                {
                    for (int j = 0; j < newVideos.Count; j++)
                    {
                        var v = newVideos[j];

                        var existingVideo = await db.Videos.FirstOrDefaultAsync(x => x.YoutubeId == v.Id);
                        bool shouldDownload = true;

                        if (existingVideo != null && existingVideo.IsDownloaded)
                        {
                            string checkExt = defaultAction == DownloadAction.AudioOnly ? ".mp3" : ".mp4";
                            string expectedFileName = FileNameHelper.CleanFileName(v.Title) + checkExt; // Basit bir kontrol
                            string checkPath = Path.Combine(downloadFolder, expectedFileName);

                            if (File.Exists(checkPath))
                            {
                                shouldDownload = false;
                                Console.WriteLine($"\r[{j + 1}/{newVideos.Count}] Atlanıyor: {v.Title} (Zaten mevcut)");
                            }
                        }

                        if (shouldDownload)
                        {
                            try
                            {
                                var videoRecord = existingVideo ?? new Video { YoutubeId = v.Id, ChannelId = channel.Id };
                                videoRecord.Title = v.Title;
                                videoRecord.PublishedAt = v.UploadDate;
                                videoRecord.IsDownloaded = false;

                                if (existingVideo == null) db.Videos.Add(videoRecord);
                                await db.SaveChangesAsync();

                                await DownloadActions.ExecuteDownload(v.Id, v.Title, channel.Name, v.UploadDate, downloadFolder, defaultAction, defaultRes, defaultAudio, j + 1, newVideos.Count);

                                videoRecord.IsDownloaded = true;
                                channel.LastVideoDate = v.UploadDate;
                                await db.SaveChangesAsync();
                                totalProcessedVideos++;
                            }
                            catch (Exception ex) { Console.WriteLine($"\n[HATA] {v.Title}: {ex.Message}"); }
                        }
                    }
                }
            }

            if (lastCheckedSetting == null)
                db.AppSettings.Add(new AppSetting { Key = "LastCheckedDate", Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
            else
                lastCheckedSetting.Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            await db.SaveChangesAsync();

            Console.WriteLine(new string('-', 60));
            Console.WriteLine($"[TAMAMLANDI]");
            ConsoleHelper.WaitForKey();
        }
        public static void PrintChannelsWithCount(List<(Core.Models.Channel channel, int videoCount)> channelData)
        {
            Console.Clear();
            Console.WriteLine("=== Kanallar ve Video Sayıları ===");

            Console.WriteLine("{0,-5} {1,-30} {2,-15} {3,-12}", "ID", "Kanal Adı", "Son Video", "Video Sayısı");

            Console.WriteLine(new string('-', 70));

            foreach (var item in channelData)
            {
                string dateText = item.channel.LastVideoDate?.ToString("yyyy-MM-dd") ?? "Null";

                Console.WriteLine("{0,-5} {1,-30} {2,-15} {3,-12}",

                    item.channel.Id,
                    item.channel.Name,
                    dateText,
                    $"({item.videoCount} Video)");
            }
            Console.WriteLine(new string('-', 70));
        }
        public static async Task CheckVideosMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Kanalların Videolarını Kontrol Et ===");
                Console.WriteLine("1) Bütün Kanalları Kontrol Et");
                Console.WriteLine("2) Belirli Bir Kanalı Kontrol Et");
                Console.WriteLine("0) Bir Önceki Menüye Dön");
                Console.Write("\nSeçiminiz: ");
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    await ChannelActions.ScanChannels();
                }
                else if (choice == "2")
                {
                    var channels = await ChannelActions.GetChannels("Tarama");
                    if (channels != null)
                    {
                        ChannelActions.PrintChannels(channels);
                        Console.Write("\nKontrol etmek istediğiniz kanalın Id değeri (0 : İptal): ");

                        string input = Console.ReadLine();
                        if (int.TryParse(input, out int id) && id != 0)
                        {
                            await ChannelActions.ScanChannels(id);
                        }
                    }
                }
                else if (choice == "0") break;
            }
        }
    }
}