using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Models
{
    public class AppSetting
    {
        public int Id { get; set; }
        public string Key { get; set; } // Örn: "DefaultDownloadAction"
        public string Value { get; set; } // Örn: "1" (AudioOnly)
    }
}
