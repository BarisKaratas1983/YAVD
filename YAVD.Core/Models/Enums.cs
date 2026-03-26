using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Models
{
    public enum DownloadAction { AudioOnly = 1, VideoOnly = 2, Both = 3 }
    public enum VideoResolution { P144 = 144, P240 = 240, P360 = 360, P480 = 480, P720 = 720, P1080 = 1080, P1440 = 1440, P2160 = 2160 }
    public enum AudioQuality { Low = 128, Medium = 192, High = 256, Ultra = 320 }
    public static class FileFormatTags
    {
        public const string Title = "[TITLE]";
        public const string Channel = "[CHANNEL]";
        public const string Id = "[ID]";
        public const string Date = "[DATE]";
        public const string Year = "[YEAR]";
        public const string DateTime = "[DATETIME]";
        public const string Resolution = "[RES]";
        public const string Kbps = "[KBPS]";
        public const string Index = "[INDEX]";
    }
}