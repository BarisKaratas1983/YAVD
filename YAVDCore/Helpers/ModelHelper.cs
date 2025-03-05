using Google.Apis.Util;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVDCore.Methods;
using YAVDCore.Models;
using YAVDCore.Property;

namespace YAVDCore.Helpers
{
    public class ModelHelper
    {
        public static ChannelModel ConvertChannelListResponseToChannelModel(ChannelListResponse channelListResponse)
        {
            ChannelModel result = new ChannelModel();

            if (channelListResponse != null &&
                channelListResponse.Items != null &&
                channelListResponse.Items.Count == 1)
            {
                result.YouTubeChannelId = channelListResponse.Items[0].Id;
                result.Description = channelListResponse.Items[0].Snippet.Description;
                result.Title = channelListResponse.Items[0].Snippet.Title;
            }

            return result;
        }
        public static ChannelModel ConvertVideoListResponseToChannelModel(VideoListResponse videoListResponse)
        {
            ChannelModel result = new ChannelModel();

            if (videoListResponse != null &&
                videoListResponse.Items != null &&
                videoListResponse.Items.Count == 1 &&
                videoListResponse.Items[0].Snippet != null)
            {
                result.YouTubeChannelId = videoListResponse.Items[0].Snippet.ChannelId;
                result.Title = videoListResponse.Items[0].Snippet.ChannelTitle;
            }

            return result;
        }
        public static List<VideoModel> ConvertSearchListResponseToVideoModelList(SearchListResponse searchListResponse)
        {
            List<VideoModel> result = new List<VideoModel>();

            if (searchListResponse != null &&
                searchListResponse.Items != null &&
                searchListResponse.Items.Count > 0)
            {
                foreach (var response in searchListResponse.Items)
                {

                    result.Add(new VideoModel
                    {
                        ChannelId = DatabaseMethods.GetChannels(response.Snippet.ChannelId).First().ChannelId,
                        YouTubeVideoId = response.Id.VideoId,
                        Title = response.Snippet.Title,
                        Description = response.Snippet.Description,
                        PublishedAt = response.Snippet.PublishedAt ?? DateTime.Now
                    });
                }
            }

            return result;
        }
    }
}
