using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Models
{
    public class Channel
    {
        public int Id { get; set; }
        public string YoutubeId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime LastCheckedDate { get; set; }
    }
}
