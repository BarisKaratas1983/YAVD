using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Models
{
    public class Video
    {
        public int Id { get; set; }
        public string YoutubeId { get; set; }
        public string Title { get; set; }
        public DateTime PublishedAt { get; set; }
        public int ChannelId { get; set; }
        public bool IsDownloaded { get; set; }
    }
}
