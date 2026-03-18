using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Models
{
    public enum DownloadAction
    {
        None = 0,
        AudioOnly = 1,
        VideoOnly = 2,
        Both = 3
    }
    public enum VideoResolution
    {
        P144 = 144,
        P240 = 240,
        P360 = 360,
        P480 = 480,
        P720 = 720,
        P1080 = 1080,
        P1440 = 1440,
        P2160 = 2160
    }
}
