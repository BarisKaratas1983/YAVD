using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVDCore.Helpers;
using YAVDCore.Models;

namespace YAVDCore.Methods
{
    public class MainSettingsMethods
    {
        public static MainSettingsModel LoadSettings()
        {
            MainSettingsModel result = null;
            
            using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
            {
                result = cnn.QuerySingle<MainSettingsModel>("Select * From MainSettings Limit 1");
            }
            return result ?? new MainSettingsModel();
        }
    }
}