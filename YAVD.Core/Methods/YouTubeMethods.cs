using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVD.Core.Helpers;
using YAVD.Core.Models;

namespace YAVD.Core.Methods
{
    public class YouTubeMethods
    {
        public static YouTubeChannelModel GetYouTubeChannel(string channelName)
        {           
            YouTubeService youTubeService = YouTubeHelpers.GetYouTubeService(MainSettingsMethods.LoadSettings().YouTubeApiKey);
            
            var channelsResource = youTubeService.Channels.List("snippet");
            channelsResource.ForUsername = channelName;
            
            var channelListResponse = channelsResource.Execute();

            return null;
        }
    }
}
