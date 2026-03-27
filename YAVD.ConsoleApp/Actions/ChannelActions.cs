using Microsoft.EntityFrameworkCore;
using YAVD.Core.Data;
using YAVD.Core.Helpers;
using YAVD.Core.Models;
using YAVD.Core.Services;

namespace YAVD.ConsoleApp.Actions
{
    public static class ChannelActions
    {
        private static readonly YoutubeManager _ytService = new YoutubeManager();
        public static void WaitForKey()
        {
            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }        
        public static async Task<List<Channel>?> GetChannelsOrWarn(string actionName)
        {
            using var db = new YAVDContext();
            var channels = await db.Channels.ToListAsync();

            if (!channels.Any())
            {
                Console.WriteLine($"\n[UYARI] {actionName} yapılacak kayıtlı bir kanal bulunamadı.");
                WaitForKey();
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
        public static async Task AddChannel()
        {
            Console.Clear();
            Console.WriteLine("=== Kanal Ekle ===");
            Console.Write("Kanal veya Video Linki: ");
            
            string url = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(url)) return;

            try
            {
                using var db = new YAVDContext();
                var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "IncludeShorts");
                bool includeShorts = setting?.Value?.ToLower() == "true" || setting?.Value == "1";

                Console.WriteLine("Kanal bilgileri ve son video tarihi analiz ediliyor...");
                var channelMetadata = await _ytService.GetChannelMetadataAsync(url, includeShorts);

                if (await db.Channels.AnyAsync(c => c.YoutubeId == channelMetadata.YoutubeId))
                {
                    Console.WriteLine("\n[UYARI] Bu kanal zaten kayıtlı!");
                }
                else
                {
                    db.Channels.Add(channelMetadata);
                    await db.SaveChangesAsync();
                    Console.WriteLine($"\n[BAŞARILI] {channelMetadata.Name} kanalı eklendi.");
                }
            }
            catch (Exception ex) { Console.WriteLine($"\n[HATA] {ex.Message}"); }
            WaitForKey();
        }        
        public static async Task DeleteChannel()
        {
            var channels = await GetChannelsOrWarn("Silinecek");
            if (channels == null) return;

            PrintChannels(channels);
            Console.Write("\nSilmek istediğiniz kanalın Id değeri (0 : İptal): ");

            if (!int.TryParse(Console.ReadLine(), out int id) || id == 0) return;

            using var db = new YAVDContext();
            var channel = await db.Channels.FindAsync(id);

            if (channel != null)
            {
                Console.Write($"\n'{channel.Name}' adlı kanalı ve video bilgilerini silmek istediğinize emin misiniz? (E/H): ");
                string confirm = Console.ReadLine();

                if (confirm?.Equals("e", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var videos = db.Videos.Where(v => v.ChannelId == id);
                    db.Videos.RemoveRange(videos);
                    db.Channels.Remove(channel);
                    await db.SaveChangesAsync();
                    Console.WriteLine($"\n[BAŞARILI] {channel.Name} kanalı ve tüm kayıtları silindi.");
                }
            }
            else Console.WriteLine("\n[HATA] Geçersiz ID.");
            WaitForKey();
        }        
        public static async Task ToggleChannelStatus()
        {
            var channels = await GetChannelsOrWarn("Düzenlenecek");
            if (channels == null) return;

            PrintChannels(channels);
            Console.Write("\nDurumunu değiştirmek istediğiniz kanalın Id değeri (0 : İptal): ");

            if (!int.TryParse(Console.ReadLine(), out int id) || id == 0) return;

            using var db = new YAVDContext();
            var channel = await db.Channels.FindAsync(id);

            if (channel != null)
            {
                channel.Active = !channel.Active;
                await db.SaveChangesAsync();
                string statusText = channel.Active ? "AKTİF" : "PASİF";
                Console.WriteLine($"\n[BİLGİ] {channel.Name} kanalı {statusText} yapıldı.");
            }
            else Console.WriteLine("\n[HATA] Geçersiz ID.");
            WaitForKey();
        }
        public static async Task ScanChannelsAction(int? specificChannelId = null)
        {
            using var db = new YAVDContext();
            
            var includeShortsSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "IncludeShorts");
            var actionSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultDownloadAction");
            var resSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultVideoResolution");
            var audioSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultAudioQuality");
            var dirSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultDownloadDirectory");
            var lastCheckedSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "LastCheckedDate");

            bool includeShorts = includeShortsSetting?.Value == "1" || includeShortsSetting?.Value?.ToLower() == "true";
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
                var newVideos = await _ytService.GetNewVideosFromChannelAsync(channel.YoutubeId, channel.LastVideoDate, includeShorts);

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
            WaitForKey();
        }
    }
}