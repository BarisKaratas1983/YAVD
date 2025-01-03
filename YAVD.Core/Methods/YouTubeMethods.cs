﻿using Google.Apis.Services;
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
        public static ChannelModel GetYouTubeChannelFromHandle(string handle)
        {
            if (string.IsNullOrWhiteSpace(handle))
                return null;

            YouTubeService youTubeService = YouTubeHelper.GetYouTubeService(MainSettingsMethods.LoadSettings().YouTubeApiKey);

            var channelsRequest = youTubeService.Channels.List("snippet");
            channelsRequest.ForHandle = "@" + handle;

            var channelListResponse = channelsRequest.Execute();
            return ModelHelper.ConvertChannelListResponseToChannelModel(channelListResponse) ?? null;
        }
        public static ChannelModel GetYouTubeChannelFromVideoUrl(string videoUrl)
        {
            string videoId = YouTubeHelper.GetVideoId(videoUrl);

            if (string.IsNullOrWhiteSpace(videoUrl))
                return null;

            YouTubeService youTubeService = YouTubeHelper.GetYouTubeService(MainSettingsMethods.LoadSettings().YouTubeApiKey);

            var videosRequest = youTubeService.Videos.List("snippet");
            videosRequest.Id = videoId;

            var videoListResponse = videosRequest.Execute();
            return ModelHelper.ConvertVideoListResponseToChannelModel(videoListResponse) ?? null;
        }
        public static List<VideoModel> GetYouTubeVideos(string youTubeChannelId, DateTime? lastVideoDate)
        {
            if (string.IsNullOrWhiteSpace(youTubeChannelId))
                return null;

            MainSettingsModel mainSettings = MainSettingsMethods.LoadSettings();
            YouTubeService youTubeService = YouTubeHelper.GetYouTubeService(mainSettings.YouTubeApiKey);

            var searchRequest = youTubeService.Search.List("snippet");
            searchRequest.ChannelId = youTubeChannelId;
            searchRequest.Q = "";
            searchRequest.Type = "video";
            searchRequest.MaxResults = mainSettings.MaxResults;
            searchRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
            searchRequest.PublishedAfter = lastVideoDate;

            var searchListResponse = searchRequest.Execute();
            return ModelHelper.ConvertSearchListResponseToVideoModelList(searchListResponse) ?? null;
        }
    }
}