using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVDCore.Models
{
    public class VideoModel
    {
        public int VideoId { get; set; }
        public int ChannelId { get; set; }
        public string YouTubeVideoId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }
}