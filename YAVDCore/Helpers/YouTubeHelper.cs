using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace YAVDCore.Helpers
{
    public class YouTubeHelper
    {
        public static YouTubeService GetYouTubeService(string YouTubeKeyApi)
        {
            return new YouTubeService(new BaseClientService.Initializer() { ApiKey = YouTubeKeyApi });
        }
        public static string GetVideoId(string videoUrl)
        {
            string result = null;
            
            if (string.IsNullOrWhiteSpace(videoUrl))
                return string.Empty;

            try
            {
                if (!videoUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !videoUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    videoUrl = "https://" + videoUrl;
                }

                Uri uri = new Uri(videoUrl);

                if (uri.Host.IndexOf("youtu.be", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = uri.AbsolutePath.Trim('/');
                }

                if (uri.Host.IndexOf("youtube.com", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var query = HttpUtility.ParseQueryString(uri.Query);
                    result = query["v"];
                }                
            }
            catch (Exception)
            {
                result = null;
            }

            return result;
        }
        public static string GetChannelHandle(string channelUrl)
        {
            if (string.IsNullOrEmpty(channelUrl))
            {
                return string.Empty;
            }

            int atIndex = channelUrl.IndexOf('@');

            if (atIndex != -1 && atIndex < channelUrl.Length - 1)
            {
                return channelUrl.Substring(atIndex + 1);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}