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

namespace YAVD.Core.Methods
{
    public class DatabaseMethods
    {
        public static bool InsertOrReplaceYouTubeChannel(YouTubeChannelModel youTubeChannel)
        {
            bool result = false;

            using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
            {
                try
                {
                    cnn.Execute("Insert or Replace Into YouTubeChannels(Id, Title, Description) Values(@Id, @Title, @Description);", youTubeChannel);
                    result = true;
                }
                catch (Exception)
                {
                }
            }

            return result;
        }
        public static bool InsertOrReplaceYouTubeVideo(List<YouTubeVideoModel> youTubeVideos)
        {
            bool result = true;

            if (youTubeVideos != null)
            {
                foreach (var youTubeVideo in youTubeVideos)
                {
                    try
                    {
                        using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
                        {
                            cnn.Execute("Insert or Replace Into YouTubeVideos(Id, YouTubeChannelId, Title, Description, PublishedAt) Values(@Id, @YouTubeChannelId, @Title, @Description, @PublishedAt);", youTubeVideo);
                        }
                    }
                    catch (Exception)
                    {
                        result = false;
                    }
                }
            }

            return result;
        }
        public static List<YouTubeChannelModel> GetYouTubeChannels(string channelId = null)
        {
            List<YouTubeChannelModel> result = null;

            try
            {
                using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
                {
                    result = cnn.Query<YouTubeChannelModel>("Select * From YouTubeChannels" + (channelId != null ? " Where Id = @Id" : ""), new { Id = channelId }).ToList();
                }
            }
            catch (Exception)
            {
            }

            return result ?? new List<YouTubeChannelModel>();
        }
        public static List<YouTubeVideoModel> GetYouTubeVideos(string channelId)
        {
            List<YouTubeVideoModel> result = null;

            using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
            {
                try
                {
                    result = cnn.Query<YouTubeVideoModel>("SELECT YouTubeVideosId, Id, YouTubeChannelId, Title, Description, PublishedAt FROM YouTubeVideos WHERE YouTubeChannelId = @YouTubeChannelId", new { YouTubeChannelId = channelId }).ToList();

                }
                catch (Exception ex)
                {

                    Console.WriteLine("Hata: " + ex.Message);
                }
            }

            return result ?? new List<YouTubeVideoModel>();
        }
    }
}