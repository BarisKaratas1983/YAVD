using YAVD.Core.Data;
using YAVD.Core.Models;
using YAVD.Core.Services;
using YAVD.Core.Helpers;
using Microsoft.EntityFrameworkCore;

namespace YAVD.ConsoleApp.Actions
{
    public static class DownloadActions
    {
        private static readonly YoutubeManager _ytService = new YoutubeManager();
        public static async Task HandleDirectDownload(string url, bool isPlaylist)
        {
            using var db = new YAVDContext();

            var dirSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultDownloadDirectory");
            string finalPath = Path.GetFullPath(dirSetting?.Value ?? ".\\Downloads");

            var dirCheck = SystemValidator.ValidateDirectory(finalPath);
            if (!dirCheck.IsSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[BİLGİ] Hedef klasör bulunamadı: {finalPath}");
                Console.Write("Bu klasör otomatik olarak oluşturulsun mu? (E/H): ");
                Console.ResetColor();

                string answer = Console.ReadLine();
                if (answer?.Equals("e", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var createResult = SystemValidator.ValidateDirectory(finalPath, autoCreate: true);
                    if (!createResult.IsSuccess)
                    {
                        Console.WriteLine($"\n[HATA] Klasör oluşturulamadı: {createResult.Message}");
                        ChannelActions.WaitForKey();
                        return;
                    }
                    Console.WriteLine("[BAŞARILI] Klasör oluşturuldu.");
                }
                else
                {
                    Console.WriteLine("\n[İPTAL] Klasör mevcut olmadığı için işleme devam edilemiyor.");
                    ChannelActions.WaitForKey();
                    return;
                }
            }

            var actionSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultDownloadAction");
            DownloadAction defaultAction = Enum.TryParse(actionSetting?.Value, out DownloadAction a) ? a : DownloadAction.AudioOnly;

            var resSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultVideoResolution");
            VideoResolution defaultRes = Enum.TryParse(resSetting?.Value, out VideoResolution r) ? r : VideoResolution.P1080;

            var audioSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultAudioQuality");
            AudioQuality defaultAudio = Enum.TryParse(audioSetting?.Value, out AudioQuality q) ? q : AudioQuality.Medium;

            Console.WriteLine($"\n[BİLGİ] Varsayılan İndirme Ayarı: {defaultAction}");
            Console.WriteLine("1) Sadece Ses, 2) Sadece Video, 3) Ses + Video");
            Console.Write("Seçiminiz (Varsayılan için ENTER): ");
            string actionInput = Console.ReadLine();
            DownloadAction selectedAction = actionInput switch { "1" => DownloadAction.AudioOnly, "2" => DownloadAction.VideoOnly, "3" => DownloadAction.Both, _ => defaultAction };            

            VideoResolution selectedRes = defaultRes;
            AudioQuality selectedAudio = defaultAudio;

            if (selectedAction != DownloadAction.AudioOnly)
            {
                Console.WriteLine($"\n[BİLGİ] Varsayılan Video Çözünürlüğü: {(int)defaultRes}p");
                Console.WriteLine("1) 2160p, 2) 1440p, 3) 1080p, 4) 720p, 5) 480p");
                Console.Write("Seçiminiz (ENTER = Varsayılan): ");
                selectedRes = Console.ReadLine() switch { "1" => VideoResolution.P2160, "2" => VideoResolution.P1440, "3" => VideoResolution.P1080, "4" => VideoResolution.P720, "5" => VideoResolution.P480, _ => defaultRes };
            }

            if (selectedAction != DownloadAction.VideoOnly)
            {
                Console.WriteLine($"\n[BİLGİ] Varsayılan Ses Kalitesi: {(int)defaultAudio}kbps");
                Console.WriteLine("1) 320kbps, 2) 256kbps, 3) 192kbps, 4) 128kbps");
                Console.Write("Seçiminiz (ENTER = Varsayılan): ");
                selectedAudio = Console.ReadLine() switch { "1" => AudioQuality.Ultra, "2" => AudioQuality.High, "3" => AudioQuality.Medium, "4" => AudioQuality.Low, _ => defaultAudio };
            }

            if (!Directory.Exists(finalPath)) Directory.CreateDirectory(finalPath);

            try
            {
                if (isPlaylist)
                    await StartPlaylistDownload(url, finalPath, selectedAction, selectedRes, selectedAudio);
                else
                    await StartSingleVideoDownload(url, finalPath, selectedAction, selectedRes, selectedAudio);

                Console.WriteLine("\n\n[TAMAMLANDI] Tüm indirme işlemleri başarıyla bitti.");
                Console.WriteLine($"Dosyalar şurada: {finalPath}");
                ChannelActions.WaitForKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[HATA] Bir sorun oluştu: {ex.Message}");
                ChannelActions.WaitForKey();
            }
        }
        private static async Task StartSingleVideoDownload(string url, string folder, DownloadAction action, VideoResolution res, AudioQuality audio)
        {
            Console.WriteLine("Video bilgileri alınıyor...");
            var v = await _ytService.GetVideoMetadataExtendedAsync(url);
            await ExecuteDownload(v.Id, v.Title, v.Author, v.UploadDate, folder, action, res, audio, 1, 1);
        }
        private static async Task StartPlaylistDownload(string url, string folder, DownloadAction action, VideoResolution res, AudioQuality audio)
        {
            Console.WriteLine("Playlist taranıyor...");
            var videos = await _ytService.GetPlaylistVideosExtendedAsync(url);
            for (int i = 0; i < videos.Count; i++)
            {
                var v = videos[i];
                await ExecuteDownload(v.Id, v.Title, v.Author, v.UploadDate, folder, action, res, audio, i + 1, videos.Count);
            }
        }
        public static async Task ExecuteDownload(string id, string title, string author, DateTime publishedAt, string folder, DownloadAction action, VideoResolution res, AudioQuality audio, int current, int total)
        {
            using var db = new YAVDContext();
            var tags = new Dictionary<string, string>
            {
                { FileFormatTags.Title, title },
                { FileFormatTags.Channel, author },
                { FileFormatTags.Id, id },
                { FileFormatTags.Date, publishedAt.ToString("yyyy-MM-dd") },
                { FileFormatTags.Year, publishedAt.Year.ToString() },
                { FileFormatTags.DateTime, publishedAt.ToString("yyyy-MM-dd_HH-mm") },
                { FileFormatTags.Index, total > 1 ? current.ToString("D2") : "" }
            };

            if (action == DownloadAction.AudioOnly || action == DownloadAction.Both)
            {
                var formatSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "SaveFileFormatAudio");
                string pattern = formatSetting?.Value ?? "[TITLE]";
                tags[FileFormatTags.Kbps] = ((int)audio).ToString() + "kbps";

                string fileName = FileNameHelper.BuildFileName(pattern, tags, ".mp3");
                string savePath = FileNameHelper.GetUniqueFilePath(folder, fileName);

                var progress = new Progress<double>(p => DrawProgress(p, current, total));
                await _ytService.DownloadAudioAsync(id, savePath, title, author, audio, progress);

                Console.Write($"\r[{current}/{total}] SES: Dosya hazırlanıyor ve etiketleniyor...   ");
                Console.WriteLine($"\r[{current}/{total}] SES: {fileName} -> TAMAMLANDI.   ");
            }

            if (action == DownloadAction.VideoOnly || action == DownloadAction.Both)
            {
                var formatSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "SaveFileFormatVideo");
                string pattern = formatSetting?.Value ?? "[TITLE]";
                tags[FileFormatTags.Resolution] = (int)res + "p";

                string fileName = FileNameHelper.BuildFileName(pattern, tags, ".mp4");
                string savePath = FileNameHelper.GetUniqueFilePath(folder, fileName);

                var progress = new Progress<double>(p => DrawProgress(p, current, total));
                await _ytService.DownloadVideoWithFFmpegAsync(id, savePath, res, progress);

                Console.Write($"\r[{current}/{total}] VİDEO: Ses ve video birleştiriliyor...        ");
                Console.WriteLine($"\r[{current}/{total}] VİDEO: {fileName} -> TAMAMLANDI. ");
            }
        }
        private static void DrawProgress(double p, int current, int total)
        {
            Console.Write($"\r[{current}/{total}] İndiriliyor... %{(p * 100):0.0}      ");
        }
    }
}