using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVDCore.Models
{
    public class ChannelModel
    {
        public int ChannelId { get; set; }
        public int ApiKeyId { get; set; }
        public string YouTubeChannelId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public virtual void CheckVideos() { }
    }
}