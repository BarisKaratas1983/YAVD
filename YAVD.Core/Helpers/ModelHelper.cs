using Google.Apis.Util;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVD.Core.Models;

namespace YAVD.Core.Helpers
{
    public class ModelHelper
    {
        public static YouTubeChannelModel ConvertChannelListResponseToYouTubeChannelModel(ChannelListResponse channelListResponse)
        {
            YouTubeChannelModel result = new YouTubeChannelModel();

            if (channelListResponse != null &&
                channelListResponse.Items != null &&
                channelListResponse.Items.Count == 1)
            {
                result.Id = channelListResponse.Items[0].Id;
                result.Description = channelListResponse.Items[0].Snippet.Description;
                result.Title = channelListResponse.Items[0].Snippet.Title;
            }

            return result;
        }
        public static YouTubeChannelModel ConvertVideoListResponseToYouTubeChannelModel(VideoListResponse videoListResponse)
        {
            YouTubeChannelModel result = new YouTubeChannelModel();

            if (videoListResponse != null &&
                videoListResponse.Items != null &&
                videoListResponse.Items.Count == 1 &&
                videoListResponse.Items[0].Snippet != null)
            {
                result.Id = videoListResponse.Items[0].Snippet.ChannelId;
                result.Title = videoListResponse.Items[0].Snippet.ChannelTitle;
            }

            return result;
        }
        public static List<YouTubeVideoModel> ConvertSearchListResponseToYouTubeVideoModelList(SearchListResponse searchListResponse)
        {
            List<YouTubeVideoModel> result = new List<YouTubeVideoModel>();

            if (searchListResponse != null &&
                searchListResponse.Items != null &&
                searchListResponse.Items.Count > 0)
            {
                foreach (var response in searchListResponse.Items)
                {
                    result.Add(new YouTubeVideoModel { Id = response.Id.VideoId, Title = response.Snippet.Title, Description = response.Snippet.Description, PublishedAt = response.Snippet.PublishedAt });
                }
            }

            return result;
        }
    }
}
