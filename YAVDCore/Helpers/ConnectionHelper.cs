using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVDCore.Helpers
{
    public class ConnectionHelper
    {
        public static string GetSQLiteConnectionString()
        {
            return $"Data Source=.\\Database\\YAVD.db;Version=3;";
        }
    }
}
