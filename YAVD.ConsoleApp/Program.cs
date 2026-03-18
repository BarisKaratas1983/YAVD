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

    var newVideos = await ytService.GetNewVideosFromChannelAsync(channel);

    if (newVideos.Any())
    {      
        string downloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
        if (!Directory.Exists(downloadPath)) Directory.CreateDirectory(downloadPath);

        foreach (var v in newVideos)
        {
            if (!db.Videos.Any(vid => vid.YoutubeId == v.YoutubeId))
            {
                string safeTitle = FileNameHelper.CleanFileName(v.Title);
                string fullFilePath = Path.Combine(downloadPath, $"{safeTitle}.m4a");

                Console.WriteLine($"   [INDIRILIYOR] {v.Title}...");

                try
                {
                    await ytService.DownloadAudioAsync(v.YoutubeId, fullFilePath);

                    v.IsDownloaded = true;
                    db.Videos.Add(v);
                    Console.WriteLine($"   [TAMAMLANDI] {v.Title}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   [HATA] İndirilemedi: {ex.Message}");
                }
            }
        }

        channel.LastCheckedDate = DateTime.Now;
        await db.SaveChangesAsync();
    }
    else
    {
        Console.WriteLine("   Yeni video bulunamadı.");
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