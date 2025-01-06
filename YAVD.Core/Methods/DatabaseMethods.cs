using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVD.Core.Helpers;
using YAVD.Core.Models;
using Google.Apis.YouTube.v3.Data;

namespace YAVD.Core.Methods
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
                    result = cnn.QuerySingleOrDefault("Select MaxResults From MainSettings");
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
        public static void InsertOrReplaceYouTubeChannel(ChannelModel youTubeChannel)
        {
            using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
            {
                try
                {
                    cnn.Execute("Insert or Replace Into Channels(ChannelId, Title, Description, Active, CreateDateTime, UpdateDateTime) " +
                                "Values(@ChannelId, @Title, @Description, @Active, @CreateDateTime, @UpdateDateTime);", youTubeChannel);
                }
                catch (Exception)
                {
                }
            }
        }
        public static void InsertOrReplaceYouTubeVideo(List<VideoModel> youTubeVideos)
        {
            if (youTubeVideos != null)
            {
                foreach (var youTubeVideo in youTubeVideos)
                {
                    try
                    {
                        using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
                        {
                            cnn.Execute("Insert or Replace Into Videos(VideosId, ChannelId, YouTubeVideoId, Title, Description, PublishedAt, CreateDateTime, UpdateDateTime) " +
                                        "Values(@VideosId, @ChannelId, @YouTubeVideoId, @Title, @Description, @PublishedAt, @CreateDateTime, @UpdateDateTime);", youTubeVideo);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        public static List<ChannelModel> GetYouTubeChannels(string channelId = null)
        {
            List<ChannelModel> result = null;

            try
            {
                using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
                {
                    result = cnn.Query<ChannelModel>("Select * From Channels " +
                                                    (channelId != null ? " Where ChannelId = @ChannelId" : ""), new { ChannelId = channelId }).ToList();
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