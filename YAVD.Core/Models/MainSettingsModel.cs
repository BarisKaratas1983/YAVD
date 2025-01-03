using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Models
{
    public class MainSettingsModel
    {
        public int MainSettingsId { get; set; }
        public string YouTubeApiKey { get; set; }
        public int MaxResults { get; set; }
    }
}