using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core
{
    public class MainSettingsModel
    {
        private string _YouTubeApiKey;
        public string YouTubeApiKey
        {
            get { return _YouTubeApiKey; }
            set { _YouTubeApiKey = value; }
        }
        public MainSettingsModel()
        {
            _YouTubeApiKey = "";
        }
    }
}