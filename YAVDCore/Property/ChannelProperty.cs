using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVDCore.Methods;
using YAVDCore.Models;

namespace YAVDCore.Property
{
    public class Channel : ChannelModel
    {
        public override void CheckVideos()
        {
            List<VideoModel> vids = YouTubeMethods.GetYouTubeVideos(this);

            if (vids.Count > 0)
            {
                DatabaseMethods.InsertYouTubeVideos(vids);
                DatabaseMethods.UpdateChannelLastCheckDateTime(this.ChannelId, vids.Max(v => v.PublishedAt));
            }
        }
    }
    public class ChannelMethods
    {
        public ChannelModel SaveChannelFromVideoLink(string videoLink, ApiKeyModel apiKey)
        {
            ChannelModel channel = YouTubeMethods.GetYouTubeChannelFromVideoUrl(videoLink, apiKey);
            DatabaseMethods.InsertYouTubeChannel(channel);
            return channel;
        }
        public ChannelModel SaveChannelFromChannelLink(string channelLink, ApiKeyModel apiKey)
        {
            ChannelModel channel = YouTubeMethods.GetYouTubeChannelFromHandle(channelLink, apiKey);
            DatabaseMethods.InsertYouTubeChannel(channel);
            return channel;
        }
    }
    public class ChannelProperty
    {
        private List<Channel> items;
        private ChannelMethods methods;
        public List<Channel> Items { get { return items; } }
        public ChannelMethods Methods { get { return methods; } }
        public void RefreshChannels()
        {
            items = DatabaseMethods.GetChannels().ConvertAll(x => new Channel
            {
                Active = x.Active,
                ChannelId = x.ChannelId,
                CreateDateTime = x.CreateDateTime,
                Description = x.Description,
                Title = x.Title,
                LastCheckDateTime = x.LastCheckDateTime,
                YouTubeChannelId = x.YouTubeChannelId
            });
        }
        public ChannelProperty()
        {
            RefreshChannels();
            methods = new ChannelMethods();
        }
    }
}