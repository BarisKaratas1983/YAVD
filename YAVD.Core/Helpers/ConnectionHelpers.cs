using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core
{
    public class ConnectionHelpers
    {
        public static string GetSQLiteConnectionString()
        {
            return $"Data Source=E:\\Projeler\\YAVD\\YAVD.db;Version=3;";
        }
    }
}
