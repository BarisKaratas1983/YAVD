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
    Console.WriteLine("1. Yeni Kanal Ekle");
    Console.WriteLine("2. Kanalları Tara (Toplu/Tekil)");
    Console.WriteLine("3. Kayıtlı Kanalları Listele");
    Console.WriteLine("4. Ayarlar");
    Console.WriteLine("0. Çıkış");
    Console.Write("\nSeçiminiz: ");

    string choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await AddChannelAction();
            break;
        case "2":
            await ScanChannelsAction();
            break;
        case "3":
            await ListChannelsAction();
            break;
        case "4":
            await SettingsAction();
            break;
        case "0":
            return;
        default:
            Console.WriteLine("Geçersiz seçim! Devam etmek için bir tuşa basın...");
            Console.ReadKey();
            break;
    }
}
async Task AddChannelAction()
{
    Console.Write("\nYoutube Video Linki: ");
    string url = Console.ReadLine();

    try
    {
        var channel = await ytService.GetChannelMetadataAsync(url);
        using var db = new YAVDContext();

        if (!db.Channels.Any(c => c.YoutubeId == channel.YoutubeId))
        {
            db.Channels.Add(channel);
            await db.SaveChangesAsync();
            Console.WriteLine($"\n[BAŞARILI] {channel.Name} kaydedildi.");
        }
        else
        {
            Console.WriteLine("\n[UYARI] Bu kanal zaten kayıtlı.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nHata: {ex.Message}");
    }
    Console.ReadKey();
}
async Task ScanChannelsAction()
{
    Console.Clear();
    Console.WriteLine("=== Tarama Seçenekleri ===");
    Console.WriteLine("1. Tüm Kayıtlı Kanalları Tara");
    Console.WriteLine("2. Belirli Bir Kanalı Tara (ID ile)");
    Console.Write("\nSeçiminiz: ");
    string scanChoice = Console.ReadLine();

    using var db = new YAVDContext();

    if (scanChoice == "1")
    {
        var channels = await db.Channels.ToListAsync();
        foreach (var channel in channels)
        {
            await ProcessChannelScan(channel, db);
        }
    }
    else if (scanChoice == "2")
    {
        await ListChannelsAction();
        Console.Write("\nTarama yapmak istediğiniz Kanal ID: ");
        if (int.TryParse(Console.ReadLine(), out int channelId))
        {
            var channel = await db.Channels.FindAsync(channelId);
            if (channel != null)
                await ProcessChannelScan(channel, db);
            else
                Console.WriteLine("Kanal bulunamadı.");
        }
    }

    Console.WriteLine("\nİşlem tamamlandı. Devam etmek için bir tuşa basın...");
    Console.ReadKey();
}
async Task ProcessChannelScan(Channel channel, YAVDContext db)
{
    Console.WriteLine($"\n--> {channel.Name} kontrol ediliyor...");

    var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultDownloadAction");
    var currentAction = (DownloadAction)int.Parse(setting?.Value ?? "0");
    var newVideos = await ytService.GetNewVideosFromChannelAsync(channel);

    if (newVideos.Any())
    {
        foreach (var v in newVideos)
        {
            if (!db.Videos.Any(vid => vid.YoutubeId == v.YoutubeId))
            {
                db.Videos.Add(v);
                
                if (currentAction != DownloadAction.None)
                {
                    await HandleDownloads(v, currentAction, channel.Name);
                    v.IsDownloaded = true;
                }

                Console.WriteLine($"   [KAYDEDİLDİ] {v.Title}");
            }
        }

        channel.LastCheckedDate = DateTime.Now;
        await db.SaveChangesAsync();
    }
}
// HandleDownloads metodunu bu parametrelerle güncelle:
async Task HandleDownloads(Video v, DownloadAction action, string channelName)
{
    using var db = new YAVDContext();
    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    string safeChannelName = FileNameHelper.CleanFileName(channelName);
    string safeTitle = FileNameHelper.CleanFileName(v.Title);

    // Ayarları oku
    var resSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultVideoResolution");
    var targetRes = (VideoResolution)int.Parse(resSetting?.Value ?? "1080");

    // İndirme klasörü: Downloads\<Kanal Adı>
    string channelFolder = Path.Combine(baseDir, "Downloads", safeChannelName);
    if (!Directory.Exists(channelFolder)) Directory.CreateDirectory(channelFolder);

    Console.WriteLine($"\n   >> İşleniyor: {v.Title}");

    // SES İNDİRME
    if (action == DownloadAction.AudioOnly || action == DownloadAction.Both)
    {
        string fullPath = GetUniqueFilePath(channelFolder, safeTitle, ".m4a");

        var progress = new Progress<double>(p =>
            Console.Write($"\r      [SES] %{(p * 100):0.0} indiriliyor...   "));

        await ytService.DownloadAudioAsync(v.YoutubeId, fullPath, progress);
        Console.WriteLine("\n      [SES] Tamamlandı.");
    }

    // VİDEO İNDİRME
    if (action == DownloadAction.VideoOnly || action == DownloadAction.Both)
    {
        string fullPath = GetUniqueFilePath(channelFolder, safeTitle, ".mp4");

        var progress = new Progress<double>(p =>
            Console.Write($"\r      [VİDEO] %{(p * 100):0.0} indiriliyor...   "));

        await ytService.DownloadVideoWithFFmpegAsync(v.YoutubeId, fullPath, targetRes, progress);
        Console.WriteLine("\n      [VİDEO] Tamamlandı.");
    }
}
async Task ListChannelsAction()
{
    using var db = new YAVDContext();
    var channels = await db.Channels.ToListAsync();

    Console.WriteLine("\n{0,-5} {1,-30} {2,-20}", "ID", "Kanal Adı", "Son Kontrol");
    Console.WriteLine(new string('-', 60));

    foreach (var channel in channels)
    {
        Console.WriteLine("{0,-5} {1,-30} {2,-20:dd.MM.yyyy HH:mm}",
            channel.Id,
            channel.Name.Length > 28 ? channel.Name.Substring(0, 27) + ".." : channel.Name,
            channel.LastCheckedDate);
    }
    Console.WriteLine("\nDevam etmek için bir tuşa basın...");
    Console.ReadKey();
}
async Task UpdateDownloadAction(YAVDContext db, AppSetting setting)
{
    Console.WriteLine("\nYeni İndirme Modunu Seçin:");
    Console.WriteLine("0. None (Sadece Kayıt)");
    Console.WriteLine("1. AudioOnly (Sadece Ses)");
    Console.WriteLine("2. VideoOnly (Sadece Video)");
    Console.WriteLine("3. Both (Ses + Video)");
    Console.Write("Seçiminiz: ");

    string newVal = Console.ReadLine();
    if ("0123".Contains(newVal) && newVal.Length == 1)
    {
        setting.Value = newVal;
        await db.SaveChangesAsync();
        Console.WriteLine("\n[BAŞARILI] Ayar güncellendi.");
    }
    else { Console.WriteLine("\n[HATA] Geçersiz seçim."); }
    Console.ReadKey();
}
async Task UpdateResolutionAction(YAVDContext db, AppSetting setting)
{
    Console.WriteLine("\nVarsayılan Video Çözünürlüğünü Seçin:");
    var resolutions = Enum.GetValues(typeof(VideoResolution));
    foreach (var res in resolutions)
    {
        Console.WriteLine($"{(int)res}");
    }
    Console.Write("Değer Girin (Örn: 1080): ");

    string newVal = Console.ReadLine();
    
    if (Enum.IsDefined(typeof(VideoResolution), int.Parse(newVal)))
    {
        setting.Value = newVal;
        await db.SaveChangesAsync();
        Console.WriteLine("\n[BAŞARILI] Çözünürlük güncellendi.");
    }
    else { Console.WriteLine("\n[HATA] Geçersiz çözünürlük."); }
    Console.ReadKey();
}
async Task SettingsAction()
{
    using var db = new YAVDContext();

    while (true)
    {
        Console.Clear();
        Console.WriteLine("=== PROGRAM AYARLARI ===");
                
        var downloadActionSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultDownloadAction");
        var resolutionSetting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "DefaultVideoResolution");
        
        var currentAction = (DownloadAction)int.Parse(downloadActionSetting?.Value ?? "0");
        var currentRes = resolutionSetting?.Value ?? "1080";

        Console.WriteLine($"1. Varsayılan İndirme Modu: [{currentAction}]");
        Console.WriteLine($"2. Varsayılan Video Kalitesi: [{currentRes}p]");
        Console.WriteLine("0. Ana Menüye Dön");
        Console.Write("\nDeğiştirmek istediğiniz ayar (veya 0): ");

        string choice = Console.ReadLine();

        if (choice == "1")
        {
            await UpdateDownloadAction(db, downloadActionSetting);
        }
        else if (choice == "2")
        {
            await UpdateResolutionAction(db, resolutionSetting);
        }
        else if (choice == "0")
        {
            break;
        }
    }
}
void DrawProgressBar(double progress)
{
    int barLength = 30;
    int filledLength = (int)(progress * barLength);

    Console.CursorLeft = 3; // Barın biraz içeriden başlaması için
    Console.Write("[");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write(new string('#', filledLength));
    Console.Write(new string('-', barLength - filledLength));
    Console.ResetColor();
    Console.Write($"] {(progress * 100):0.0}%");
}
string GetUniqueFilePath(string folder, string title, string extension)
{
    string fileName = $"{title}{extension}";
    string fullPath = Path.Combine(folder, fileName);
    int counter = 2;

    while (File.Exists(fullPath))
    {
        // Örn: Deneme (2).m4a
        fileName = $"{title} ({counter}){extension}";
        fullPath = Path.Combine(folder, fileName);
        counter++;
    }
    return fullPath;
}