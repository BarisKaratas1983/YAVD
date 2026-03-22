using YAVD.Core.Data;
using YAVD.Core.Models;
using YAVD.Core.Services;
using YAVD.Core.Helpers;
using Microsoft.EntityFrameworkCore;

var ytService = new YoutubeManager();

while (true)
{
    Console.Clear();
    Console.WriteLine("=== YAVD: Youtube Audio Video Downloader ===");
    Console.WriteLine("1. Direkt Video/Ses İndir");
    Console.WriteLine("0. Çıkış");
    Console.Write("\nSeçiminiz: ");
    string choice = Console.ReadLine();
    if (choice == "1") await DirectDownloadMenu();
    else if (choice == "0") return;
}

async Task DirectDownloadMenu()
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("=== Direkt Video/Ses İndirme ===");
        Console.WriteLine("1. Youtube Video Linkinden İndir");
        Console.WriteLine("2. Youtube Playlist Linkinden İndir");
        Console.WriteLine("0. Ana Menüye Dön");
        Console.Write("\nSeçiminiz: ");
        string choice = Console.ReadLine();
        if (choice == "1") await HandleDirectDownload(false);
        else if (choice == "2") await HandleDirectDownload(true);
        else if (choice == "0") break;
    }
}

async Task HandleDirectDownload(bool isPlaylist)
{
    using var db = new YAVDContext();
    var dirSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultDownloadDirectory");
    string finalPath = Path.GetFullPath(dirSetting?.Value ?? ".\\Downloads");

    var actionSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultDownloadAction");
    DownloadAction defaultAction = Enum.TryParse(actionSetting?.Value, out DownloadAction a) ? a : DownloadAction.None;

    var resSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultVideoResolution");
    VideoResolution defaultRes = Enum.TryParse(resSetting?.Value, out VideoResolution r) ? r : VideoResolution.P1080;

    var audioSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultAudioQuality");
    AudioQuality defaultAudio = Enum.TryParse(audioSetting?.Value, out AudioQuality q) ? q : AudioQuality.Medium;

    Console.Write(isPlaylist ? "\nYoutube Playlist Linki: " : "\nYoutube Video Linki: ");
    string url = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(url)) return;

    Console.WriteLine($"\n[BİLGİ] Varsayılan İndirme Ayarı: {defaultAction}");
    Console.WriteLine("1) Sadece Ses, 2) Sadece Video, 3) Ses + Video");
    Console.Write("Seçiminiz (Varsayılan için ENTER): ");
    string actionInput = Console.ReadLine();
    DownloadAction selectedAction = actionInput switch { "1" => DownloadAction.AudioOnly, "2" => DownloadAction.VideoOnly, "3" => DownloadAction.Both, _ => defaultAction };

    if (selectedAction == DownloadAction.None) return;

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

    Console.WriteLine($"\n[BİLGİ] Dosyalar şu klasöre kaydedilecek: {finalPath}");
    if (!Directory.Exists(finalPath)) Directory.CreateDirectory(finalPath);

    if (isPlaylist) await StartPlaylistDownload(url, finalPath, selectedAction, selectedRes, selectedAudio);
    else await StartSingleVideoDownload(url, finalPath, selectedAction, selectedRes, selectedAudio);

    Console.WriteLine("\nİşlem bitti. Devam etmek için bir tuşa basın...");
    Console.ReadKey();
}

async Task StartSingleVideoDownload(string url, string folder, DownloadAction action, VideoResolution res, AudioQuality audio)
{
    Console.WriteLine("Video bilgileri alınıyor...");
    var video = await ytService.GetVideoMetadataExtendedAsync(url);
    await ExecuteDownload(video.Id, video.Title, video.Author, folder, action, res, audio, 1, 1);
}

async Task StartPlaylistDownload(string url, string folder, DownloadAction action, VideoResolution res, AudioQuality audio)
{
    Console.WriteLine("Playlist taranıyor...");
    var videos = await ytService.GetPlaylistVideosExtendedAsync(url);
    for (int i = 0; i < videos.Count; i++)
    {
        await ExecuteDownload(videos[i].Id, videos[i].Title, videos[i].Author, folder, action, res, audio, i + 1, videos.Count);
    }
}

async Task ExecuteDownload(string id, string title, string author, string folder, DownloadAction action, VideoResolution res, AudioQuality audio, int current, int total)
{
    string cleanTitle = FileNameHelper.CleanFileName(title);

    // SES İNDİRME (AudioOnly veya Both ise)
    if (action == DownloadAction.AudioOnly || action == DownloadAction.Both)
    {
        string savePath = GetUniqueFilePath(folder, cleanTitle, ".mp3");
        var progress = new Progress<double>(p => DrawProgress(p, current, total, cleanTitle, ".mp3"));
        await ytService.DownloadAudioAsync(id, savePath, title, author, audio, progress);
        Console.WriteLine(); // Progress sonrası alt satıra geç
    }

    // VİDEO İNDİRME (VideoOnly veya Both ise)
    if (action == DownloadAction.VideoOnly || action == DownloadAction.Both)
    {
        string savePath = GetUniqueFilePath(folder, cleanTitle, ".mp4");
        var progress = new Progress<double>(p => DrawProgress(p, current, total, cleanTitle, ".mp4"));
        await ytService.DownloadVideoWithFFmpegAsync(id, savePath, res, progress);
        Console.WriteLine();
    }
}

void DrawProgress(double p, int current, int total, string title, string ext)
{
    // \r ile satır başına dön ve tüm satırı tekrar yaz
    Console.Write($"\r[{current}/{total}] {title}{ext} indiriliyor... %{(p * 100):0.0}   ");
}

string GetUniqueFilePath(string folder, string title, string extension)
{
    string fullPath = Path.Combine(folder, title + extension);
    int counter = 2;
    while (File.Exists(fullPath)) { fullPath = Path.Combine(folder, $"{title} ({counter}){extension}"); counter++; }
    return fullPath;
}