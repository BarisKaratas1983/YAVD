using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Models
{
    public class YouTubeChannelModel
    {
        private string _Id;
        private string _Title;
        private string _Description;
        private DateTime _LastVideoDate;
        public string Id
        {
            get { return _Id; }
            set { _Id = value; }
        }
        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        public DateTime LastVideoDate
        {
            get { return _LastVideoDate; }
            set { _LastVideoDate = value; }
        }
    }
}
