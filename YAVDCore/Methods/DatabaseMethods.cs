using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVDCore.Helpers;
using YAVDCore.Models;
using Google.Apis.YouTube.v3.Data;

namespace YAVDCore.Methods
{
    public class DatabaseMethods
    {
        public static MainSettingsModel GetMainSettings()
        {
            MainSettingsModel result = null;
            
            using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
            {
                try
                {
                    result = cnn.Query<MainSettingsModel>("Select * From MainSettings").First();
                }
                catch (Exception)
                {
                }
            }

            return result ?? new MainSettingsModel();
        }
        public static List<ApiKeyModel> GetApiKeys()
        {
            List<ApiKeyModel> result = null;

            try
            {
                using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
                {
                    result = cnn.Query<ApiKeyModel>("Select * From ApiKeys").ToList();
                }
            }
            catch (Exception)
            {
                result = null;
            }

            return result;
        }
        public static void InsertYouTubeChannel(ChannelModel youTubeChannel)
        {
            using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
            {
                try
                {
                    cnn.Execute("Insert Into Channels(ApiKeyId, YouTubeChannelId, Title, Description, Active, CreateDateTime) " +
                                "Values(@ApiKeyId, @YouTubeChannelId, @Title, @Description, 1, datetime('now', 'localtime'));", youTubeChannel);
                }
                catch (Exception)
                {
                }
            }
        }
        public static void InsertYouTubeVideos(List<VideoModel> youTubeVideos)
        {
            if (youTubeVideos != null)
            {
                foreach (var youTubeVideo in youTubeVideos)
                {
                    try
                    {
                        using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
                        {
                            cnn.Execute("Insert Into Videos(ChannelId, YouTubeVideoId, Title, Description, PublishedAt, CreateDateTime) " +
                                        "Values(@ChannelId, @YouTubeVideoId, @Title, @Description, @PublishedAt, datetime('now', 'localtime'));", youTubeVideo);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        public static List<ChannelModel> GetChannels(string youTubeChannelId = null)
        {
            List<ChannelModel> result = null;

            try
            {
                using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
                {
                    result = cnn.Query<ChannelModel>("Select * From Channels " +
                                                    (youTubeChannelId != null ? " Where YouTubeChannelId = @YouTubeChannelId" : ""), new { YouTubeChannelId = youTubeChannelId }).ToList();
                }
            }
            catch (Exception)
            {
            }

            return result ?? new List<ChannelModel>();
        }
        public static List<VideoModel> GetYouTubeVideos(string channelId)
        {
            List<VideoModel> result = null;

            using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
            {
                try
                {
                    result = cnn.Query<VideoModel>("Select * From Videos Where ChannelId = @ChannelId", new { ChannelId = channelId }).ToList();

                }
                catch (Exception)
                {
                }
            }

            return result ?? new List<VideoModel>();
        }
    }
}