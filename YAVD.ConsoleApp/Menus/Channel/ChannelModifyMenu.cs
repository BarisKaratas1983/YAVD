using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVD.ConsoleApp.Actions;
using YAVD.ConsoleApp.Helpers;
using YAVD.Core.Data;
using YAVD.Core.Services;

namespace YAVD.ConsoleApp.Menus
{
    public static class ChannelModifyMenu
    {
        private static readonly YoutubeManager _ytService = new YoutubeManager();
        public static async Task Show()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Kanal Ekle / Düzenle / Sil ===");
                Console.WriteLine("1) Kanal Ekle");
                Console.WriteLine("2) Kanal Düzenle (Aktif/Pasif)");
                Console.WriteLine("3) Kanal Sil");
                Console.WriteLine("0) Önceki Menüye Dön");
                Console.Write("\nSeçiminiz: ");
                string choice = Console.ReadLine();

                if (choice == "1") await AddChannel();
                else if (choice == "2") await ToggleChannelStatus();
                else if (choice == "3") await DeleteChannel();
                else if (choice == "0") break;
            }
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
            ConsoleHelper.WaitForKey();
        }
        public static async Task ToggleChannelStatus()
        {
            var channels = await ChannelActions.GetChannels("Düzenlenecek");
            if (channels == null) return;

            ChannelActions.PrintChannels(channels);
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

            ConsoleHelper.WaitForKey();
        }
        public static async Task DeleteChannel()
        {
            var channels = await ChannelActions.GetChannels("Silinecek");
            if (channels == null) return;

            ChannelActions.PrintChannels(channels);
            Console.Write("\nSilmek istediğiniz kanalın Id değeri (0 : İptal): ");

            if (!int.TryParse(Console.ReadLine(), out int id) || id == 0) return;

            using var db = new YAVDContext();
            var channel = await db.Channels.FindAsync(id);

            if (channel != null)
            {
                Console.Write($"\n'{channel.Name}' adlı kanalı ve video bilgilerini de silmek istediğinize emin misiniz? (E/H): ");
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
            ConsoleHelper.WaitForKey();
        }
    }
}
