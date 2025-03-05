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
            DatabaseMethods.InsertYouTubeVideos(vids);
        }
    }

    public class ChannelMethods
    {
        public bool SaveChannelFromVideoLink(string videoLink, ApiKeyModel apiKey)
        {
            bool result = false;
            ChannelModel channel = YouTubeMethods.GetYouTubeChannelFromVideoUrl(videoLink, apiKey);

            if (channel != null)
            {
                DatabaseMethods.InsertYouTubeChannel(channel);
                result = true;
            }

            return result;
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
                ApiKeyId = x.ApiKeyId,
                ChannelId = x.ChannelId,
                CreateDateTime = x.CreateDateTime,
                Description = x.Description,
                Title = x.Title,
                UpdateDateTime = x.UpdateDateTime,
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