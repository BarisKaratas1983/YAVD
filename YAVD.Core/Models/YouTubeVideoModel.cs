using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Models
{
    public class YouTubeVideoModel
    {
        private string Id_;
        private string Title_;
        private string Description_;
        private DateTime? PublishedAt_;

        public string Id
        {
            get { return Id_; }
            set { Id_ = value; }
        }
        public string Title
        {
            get { return Title_; }
            set { Title_ = value; }
        }
        public string Description
        {
            get { return Description_; }
            set { Description_ = value; }
        }
        public DateTime? PublishedAt
        {
            get { return PublishedAt_; }
            set { PublishedAt_ = value; }
        }
    }
}
