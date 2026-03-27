using YAVD.Core.Models;

namespace YAVD.Core.Helpers
{
    public static class SystemValidator
    {        
        public static ValidationResult CheckDatabase()
        {     
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "YAVD.db");

            if (!File.Exists(dbPath))
            {
                return ValidationResult.Fail("Kritik Hata: 'YAVD.db' veritabanı dosyası bulunamadı! Uygulama çalışmaya devam edemez.");
            }

            return ValidationResult.Success();
        }        
        public static ValidationResult CheckFFmpeg()
        {
            string[] requiredFiles = { "ffmpeg.exe", "ffprobe.exe" };
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            foreach (var file in requiredFiles)
            {
                if (!File.Exists(Path.Combine(baseDir, file)))
                {
                    return ValidationResult.Fail($"Kritik Hata: '{file}' dosyası eksik! Video birleştirme ve ses dönüştürme işlemleri yapılamaz.");
                }
            }

            return ValidationResult.Success();
        }        
        public static ValidationResult ValidateDirectory(string path, bool autoCreate = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return ValidationResult.Fail("Geçersiz klasör yolu.");

                if (!Directory.Exists(path))
                {
                    if (autoCreate)
                    {
                        Directory.CreateDirectory(path);
                        return ValidationResult.Success();
                    }
                    return ValidationResult.Fail($"Klasör bulunamadı: {path}");
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Fail($"Klasör erişim hatası: {ex.Message}");
            }
        }
    }
}