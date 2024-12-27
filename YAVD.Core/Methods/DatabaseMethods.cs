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
                    cnn.Execute("Insert or Replace Into YouTubeChannel(Id, Title, Description) Values(@Id, @Title, @Description);", youTubeChannel);
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
                    result = cnn.Query<YouTubeChannelModel>("Select * From YouTubeChannel " + (channelId != null ? "Where Id = @Id" : ""), new { Id = channelId }).ToList();
                }
            }
            catch (Exception)
            {
            }

            return result;
        }
    }
}