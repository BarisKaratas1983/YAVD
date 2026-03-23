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
    Console.WriteLine("2. Kanallar"); // Yeni eklenen
    Console.WriteLine("0. Çıkış");
    Console.Write("\nSeçiminiz: ");
    string choice = Console.ReadLine();

    if (choice == "1") await DirectDownloadMenu();
    else if (choice == "2") await ChannelsMenu(); // Yeni metod
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
async Task ChannelsMenu()
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("=== Kanal Yönetimi ===");
        Console.WriteLine("1) Kanal Ekle");
        Console.WriteLine("2) Kanal Listesi");
        Console.WriteLine("3) Kanal Düzenle/Sil");
        Console.WriteLine("0) Ana Menüye Dön");
        Console.Write("\nSeçiminiz: ");
        string choice = Console.ReadLine();

        if (choice == "1") await AddChannelAction();
        else if (choice == "2") await ListChannelsAction();
        else if (choice == "3") await EditDeleteChannelAction();
        else if (choice == "0") break;
    }
}
async Task AddChannelAction()
{
    Console.Write("\nKanal veya Video Linki: ");
    string url = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(url)) return;

    try
    {
        Console.WriteLine("Kanal bilgileri alınıyor...");
        var channelMetadata = await ytService.GetChannelMetadataAsync(url);

        using var db = new YAVDContext();
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
    catch (Exception ex) { Console.WriteLine($"Hata: {ex.Message}"); }

    Console.WriteLine("\nDevam etmek için bir tuşa basın...");
    Console.ReadKey();
}
async Task ListChannelsAction()
{
    Console.Clear();
    Console.WriteLine("=== Kayıtlı Kanallar ===\n");
    using var db = new YAVDContext();
    var channels = await db.Channels.ToListAsync();

    if (!channels.Any()) Console.WriteLine("Henüz kayıtlı kanal yok.");

    foreach (var c in channels)
    {
        string status = c.Active ? "Aktif" : "Pasif";
        Console.WriteLine($"ID: {c.Id} | İsim: {c.Name} | Durum: {status}");
    }
    Console.WriteLine("Devam etmek için bir tuşa basın...");
    Console.ReadKey();
}
async Task EditDeleteChannelAction()
{
    Console.Write("\nİşlem yapmak istediğiniz Kanal ID: ");
    if (!int.TryParse(Console.ReadLine(), out int id)) return;

    using var db = new YAVDContext();
    var channel = await db.Channels.FindAsync(id);
    if (channel == null) { Console.WriteLine("Kanal bulunamadı."); Console.ReadKey(); return; }

    Console.WriteLine($"\nSeçili: {channel.Name}");
    Console.WriteLine("1) Aktif/Pasif Yap");
    Console.WriteLine("2) Kanalı Sil");
    Console.Write("Seçiminiz: ");
    string choice = Console.ReadLine();

    if (choice == "1")
    {
        channel.Active = !channel.Active;
        await db.SaveChangesAsync();
        Console.WriteLine($"Kanal durumu {(channel.Active ? "Aktif" : "Pasif")} olarak güncellendi.");
    }
    else if (choice == "2")
    {
        db.Channels.Remove(channel);
        await db.SaveChangesAsync();
        Console.WriteLine("Kanal başarıyla silindi.");
    }
    Console.ReadKey();
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

    if (action == DownloadAction.AudioOnly || action == DownloadAction.Both)
    {
        string savePath = GetUniqueFilePath(folder, cleanTitle, ".mp3");
        var progress = new Progress<double>(p => DrawProgress(p, current, total, cleanTitle, ".mp3"));
        await ytService.DownloadAudioAsync(id, savePath, title, author, audio, progress);
        Console.WriteLine();
    }
    
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
    Console.Write($"\r[{current}/{total}] {title}{ext} indiriliyor... %{(p * 100):0.0}   ");
}
string GetUniqueFilePath(string folder, string title, string extension)
{
    string fullPath = Path.Combine(folder, title + extension);
    int counter = 2;
    while (File.Exists(fullPath)) { fullPath = Path.Combine(folder, $"{title} ({counter}){extension}"); counter++; }
    return fullPath;
}