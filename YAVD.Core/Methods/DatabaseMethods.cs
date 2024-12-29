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
        public static bool InsertOrReplaceYouTubeVideo(YouTubeVideoModel youTubeVideo)
        {
            bool result = false;

            using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
            {
                try
                {
                    cnn.Execute("Insert or Replace Into YouTubeVideos(Id, YouTubeChannelId, Title, Description, PublishedAt) Values(@Id, @YouTubeChannelId, @Title, @Description, @PublishedAt);", youTubeVideo);
                    result = true;
                }
                catch (Exception)
                {
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

            try
            {
                using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
                {
                    var videos = cnn.Query("SELECT YouTubeVideosId, Id, YouTubeChannelId, Title, Description, PublishedAt FROM YouTubeVideos WHERE YouTubeChannelId = @YouTubeChannelId",
                                           new { YouTubeChannelId = channelId }).ToList();

                    // Dönüşüm işlemi: PublishedAt alanını long olarak alıp, DateTime'a çevirme
                    result = videos.Select(video => new YouTubeVideoModel
                    {
                        YouTubeVideosId = video.YouTubeVideosId,
                        Id = video.Id,
                        YouTubeChannelId = video.YouTubeChannelId,
                        Title = video.Title,
                        Description = video.Description,
                        PublishedAt = video.PublishedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(video.PublishedAt.Value).DateTime : (DateTime?)null
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
            }

            return result;
        }

    }
}