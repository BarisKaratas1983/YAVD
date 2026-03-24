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
    Console.WriteLine("2. Kanallar");
    Console.WriteLine("0. Çıkış");
    Console.Write("\nSeçiminiz: ");
    string choice = Console.ReadLine();

    if (choice == "1") await DirectDownloadMenu();
    else if (choice == "2") await ChannelsMenu();
    else if (choice == "0") return;
}
async Task DirectDownloadMenu()
{
    Console.Clear();
    Console.WriteLine("=== Direkt Video/Ses İndirme ===");
    Console.Write("\nYouTube videosunun veya playlistin linkini girin: ");
    string url = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(url)) return;

    // Link Analizi: Playlist mi yoksa Tekil Video mu?
    var playlistId = YoutubeExplode.Playlists.PlaylistId.TryParse(url);
    var videoId = YoutubeExplode.Videos.VideoId.TryParse(url);

    bool isPlaylist = playlistId != null;
    bool isVideo = videoId != null;

    // Eğer linkte hem video hem playlist ID'si varsa (örneğin bir playlist içinden video açılmışsa),
    // önceliği playliste mi yoksa videoya mı vereceğimizi seçebiliriz. 
    // Genelde kullanıcı playlist içindeyken o linki atıyorsa playlisti indirmek ister.
    if (isPlaylist)
    {
        Console.WriteLine("[TESPİT] Çalma listesi (Playlist) algılandı.");
        await HandleDirectDownload(url, true);
    }
    else if (isVideo)
    {
        Console.WriteLine("[TESPİT] Tekil video algılandı.");
        await HandleDirectDownload(url, false);
    }
    else
    {
        Console.WriteLine("\n[HATA] Geçersiz veya desteklenmeyen bir YouTube linki girdiniz.");
        Console.WriteLine("Devam etmek için bir tuşa basın...");
        Console.ReadKey();
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
        using var db = new YAVDContext();

        var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "IncludeShorts");
        bool includeShorts = setting?.Value?.ToLower() == "true" || setting?.Value == "1";

        Console.WriteLine("Kanal bilgileri ve son video tarihi analiz ediliyor...");
        
        var channelMetadata = await ytService.GetChannelMetadataAsync(url, includeShorts);

        if (await db.Channels.AnyAsync(c => c.YoutubeId == channelMetadata.YoutubeId))
        {
            Console.WriteLine("\n[UYARI] Bu kanal zaten kayıtlı!");
        }
        else
        {
            db.Channels.Add(channelMetadata);
            await db.SaveChangesAsync();
            Console.WriteLine($"\n[BAŞARILI] {channelMetadata.Name} kanalı eklendi.");
            Console.WriteLine($"[BİLGİ] Takip başlangıç tarihi: {channelMetadata.LastVideoDate}");
        }
    }
    catch (Exception ex) { Console.WriteLine($"\n[HATA] {ex.Message}"); }

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
async Task HandleDirectDownload(string url, bool isPlaylist)
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
    
    Console.WriteLine($"\n[BİLGİ] Varsayılan İndirme Ayarı: {defaultAction}");
    Console.WriteLine("1) Sadece Ses, 2) Sadece Video, 3) Ses + Video");
    Console.Write("Seçiminiz (Varsayılan için ENTER): ");
    string actionInput = Console.ReadLine();
    DownloadAction selectedAction = actionInput switch
    {
        "1" => DownloadAction.AudioOnly,
        "2" => DownloadAction.VideoOnly,
        "3" => DownloadAction.Both,
        _ => defaultAction
    };

    if (selectedAction == DownloadAction.None)
    {
        Console.WriteLine("\n[UYARI] İndirme modu 'None' olarak ayarlı. İşlem iptal edildi.");
        Console.ReadKey();
        return;
    }
    
    VideoResolution selectedRes = defaultRes;
    AudioQuality selectedAudio = defaultAudio;

    if (selectedAction != DownloadAction.AudioOnly)
    {
        Console.WriteLine($"\n[BİLGİ] Varsayılan Video Çözünürlüğü: {(int)defaultRes}p");
        Console.WriteLine("1) 2160p, 2) 1440p, 3) 1080p, 4) 720p, 5) 480p");
        Console.Write("Seçiminiz (ENTER = Varsayılan): ");
        selectedRes = Console.ReadLine() switch
        {
            "1" => VideoResolution.P2160,
            "2" => VideoResolution.P1440,
            "3" => VideoResolution.P1080,
            "4" => VideoResolution.P720,
            "5" => VideoResolution.P480,
            _ => defaultRes
        };
    }

    if (selectedAction != DownloadAction.VideoOnly)
    {
        Console.WriteLine($"\n[BİLGİ] Varsayılan Ses Kalitesi: {(int)defaultAudio}kbps");
        Console.WriteLine("1) 320kbps, 2) 256kbps, 3) 192kbps, 4) 128kbps");
        Console.Write("Seçiminiz (ENTER = Varsayılan): ");
        selectedAudio = Console.ReadLine() switch
        {
            "1" => AudioQuality.Ultra,
            "2" => AudioQuality.High,
            "3" => AudioQuality.Medium,
            "4" => AudioQuality.Low,
            _ => defaultAudio
        };
    }
    
    Console.WriteLine($"\n[BİLGİ] Kayıt Klasörü: {finalPath}");
    if (!Directory.Exists(finalPath)) Directory.CreateDirectory(finalPath);

    try
    {
        if (isPlaylist)
            await StartPlaylistDownload(url, finalPath, selectedAction, selectedRes, selectedAudio);
        else
            await StartSingleVideoDownload(url, finalPath, selectedAction, selectedRes, selectedAudio);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[HATA] Bir sorun oluştu: {ex.Message}");
    }

    Console.WriteLine("\nİşlem tamamlandı. Devam etmek için bir tuşa basın...");
    Console.ReadKey();
}
async Task StartSingleVideoDownload(string url, string folder, DownloadAction action, VideoResolution res, AudioQuality audio)
{
    Console.WriteLine("Video bilgileri alınıyor...");
    var video = await ytService.GetVideoMetadataExtendedAsync(url);
    await ExecuteDownload(video.Id, video.Title, video.Author, video.UploadDate, folder, action, res, audio, 1, 1);
}
async Task StartPlaylistDownload(string url, string folder, DownloadAction action, VideoResolution res, AudioQuality audio)
{
    Console.WriteLine("Playlist taranıyor...");
    var videos = await ytService.GetPlaylistVideosExtendedAsync(url);
    for (int i = 0; i < videos.Count; i++)
    {
        await ExecuteDownload(videos[i].Id, videos[i].Title, videos[i].Author, videos[i].UploadDate, folder, action, res, audio, i + 1, videos.Count);
    }
}
async Task ExecuteDownload(string id, string title, string author, DateTime publishedAt, string folder, DownloadAction action, VideoResolution res, AudioQuality audio, int current, int total)
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
        string savePath = GetUniqueFilePath(folder, fileName);

        var progress = new Progress<double>(p => DrawProgress(p, current, total, fileName));
        await ytService.DownloadAudioAsync(id, savePath, title, author, audio, progress);
        Console.WriteLine();
    }

    if (action == DownloadAction.VideoOnly || action == DownloadAction.Both)
    {
        var formatSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "SaveFileFormatVideo");
        string pattern = formatSetting?.Value ?? "[TITLE]";

        tags[FileFormatTags.Resolution] = (int)res + "p";

        string fileName = FileNameHelper.BuildFileName(pattern, tags, ".mp4");
        string savePath = GetUniqueFilePath(folder, fileName);

        var progress = new Progress<double>(p => DrawProgress(p, current, total, fileName));
        await ytService.DownloadVideoWithFFmpegAsync(id, savePath, res, progress);
        Console.WriteLine();
    }
}
void DrawProgress(double p, int current, int total, string fileName)
{
    Console.Write($"\r[{current}/{total}] {fileName} indiriliyor... %{(p * 100):0.0}   ");
}
string GetUniqueFilePath(string folder, string fileNameWithExtension)
{
    string fullPath = Path.Combine(folder, fileNameWithExtension);

    if (!File.Exists(fullPath)) return fullPath;

    string fileNameOnly = Path.GetFileNameWithoutExtension(fileNameWithExtension);
    string extension = Path.GetExtension(fileNameWithExtension);
    int counter = 2;

    while (File.Exists(fullPath))
    {
        string newName = $"{fileNameOnly} ({counter}){extension}";
        fullPath = Path.Combine(folder, newName);
        counter++;
    }
    return fullPath;
}