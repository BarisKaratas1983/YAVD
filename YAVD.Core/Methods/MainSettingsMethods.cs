using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVD.Core.Helpers;
using YAVD.Core.Models;

namespace YAVD.Core.Methods
{
    public class MainSettingsMethods
    {
        public static MainSettingsModel LoadSettings()
        {
            MainSettingsModel result = null;
            
            using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
            {
                result = cnn.QuerySingle<MainSettingsModel>("Select * From MainSettings Where MainSettingsId = 1", new DynamicParameters());
            }
            return result ?? new MainSettingsModel();
        }
    }
}