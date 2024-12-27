using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Models
{
    public class MainSettingsModel
    {
        private string _YouTubeApiKey;
        private int _MaxResults;
        public string YouTubeApiKey
        {
            get { return _YouTubeApiKey; }
            set { _YouTubeApiKey = value; }
        }
        public int MaxResults
        {
            get { return _MaxResults; }
            set { _MaxResults = value; }
        }
    }
}