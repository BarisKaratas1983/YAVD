using Microsoft.EntityFrameworkCore;
using YAVD.Core.Data;
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
            Console.WriteLine("{0,-5} {1,-30} {2,-10}", "ID", "Kanal Adı", "Durum");
            Console.WriteLine(new string('-', 50));

            foreach (var c in channels)
            {
                string status = c.Active ? "AKTİF" : "PASİF";
                Console.WriteLine("{0,-5} {1,-30} {2,-10}", c.Id, c.Name, status);
            }
            Console.WriteLine(new string('-', 50));
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
    }
}