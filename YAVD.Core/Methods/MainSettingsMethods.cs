﻿using Dapper;
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
            using (IDbConnection cnn = new SQLiteConnection(ConnectionHelper.GetSQLiteConnectionString()))
            {
                var mainSettings = cnn.QuerySingle<MainSettingsModel>("Select * From MainSettings Where Id = 1", new DynamicParameters());
                return mainSettings;
            }
        }
    }
}
