using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Models
{
    public class ChannelModel
    {
        public string ChannelId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public ChannelModel()
        {
            CreateDateTime = DateTime.Now;
        }
    }
}