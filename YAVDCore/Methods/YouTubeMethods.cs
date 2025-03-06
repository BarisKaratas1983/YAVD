using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVDCore.Helpers;
using YAVDCore.Models;

namespace YAVDCore.Methods
{
    public class YouTubeMethods
    {
        public static ChannelModel GetYouTubeChannelFromHandle(string handle, ApiKeyModel apiKey)
        {
            ChannelModel result = null;

            if (string.IsNullOrWhiteSpace(handle))
                return null;

            YouTubeService youTubeService = YouTubeHelper.GetYouTubeService(apiKey.ApiKey);

            var channelsRequest = youTubeService.Channels.List("snippet");
            channelsRequest.ForHandle = "@" + handle;

            var channelListResponse = channelsRequest.Execute();
            if (channelListResponse != null)
            {
                result = ModelHelper.ConvertChannelListResponseToChannelModel(channelListResponse);
                result.ApiKeyId = apiKey.ApiKeyId;
            }
            return result;
        }
        public static ChannelModel GetYouTubeChannelFromVideoUrl(string videoUrl, ApiKeyModel apiKey)
        {
            ChannelModel result = null;

            string videoId = YouTubeHelper.GetVideoId(videoUrl);

            if (string.IsNullOrWhiteSpace(videoUrl))
                return null;

            YouTubeService youTubeService = YouTubeHelper.GetYouTubeService(apiKey.ApiKey);

            var videosRequest = youTubeService.Videos.List("snippet");
            videosRequest.Id = videoId;

            var videoListResponse = videosRequest.Execute();
            if (videoListResponse != null)
            {
                result = ModelHelper.ConvertVideoListResponseToChannelModel(videoListResponse);
                result.ApiKeyId = apiKey.ApiKeyId;
            }
            return result;
        }
        public static List<VideoModel> GetYouTubeVideos(ChannelModel channel)
        {
            List<VideoModel> result;

            MainSettingsModel mainSettings = MainSettingsMethods.LoadSettings();
            YouTubeService youTubeService = YouTubeHelper.GetYouTubeService(DatabaseMethods.GetApiKeys().First(x => x.ApiKeyId == channel.ApiKeyId).ApiKey);

            var searchRequest = youTubeService.Search.List("snippet");
            searchRequest.ChannelId = channel.YouTubeChannelId;
            searchRequest.Q = "";
            searchRequest.Type = "video";
            searchRequest.MaxResults = mainSettings.MaxResults;
            searchRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
            searchRequest.PublishedAfter = channel.LastCheckDateTime;

            var searchListResponse = searchRequest.Execute();
            result = ModelHelper.ConvertSearchListResponseToVideoModelList(searchListResponse);

            foreach (var r in result)
            {
                r.ChannelId = channel.ChannelId;
            }
            
            return result;
        }
    }
}