using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Models
{
    public class YouTubeVideoModel
    {
        public int YouTubeVideosId { get; set; }
        public string Id { get; set; }
        public string YouTubeChannelId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}