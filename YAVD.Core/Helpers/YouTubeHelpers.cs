using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Helpers
{
    public class YouTubeHelpers
    {
        public static YouTubeService GetYouTubeService(string YouTubeKeyApi)
        {
            return new YouTubeService(new BaseClientService.Initializer() { ApiKey = YouTubeKeyApi });
        }
    }
}