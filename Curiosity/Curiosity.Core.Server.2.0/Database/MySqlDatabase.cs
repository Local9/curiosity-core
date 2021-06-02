using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Dapper;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using GHMatti.Utilities;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Curiosity.Core.Server.Database
{
    public class MySqlDatabase : BaseScript
    {
        internal static string resourceName = API.GetCurrentResourceName();
        internal static string resourcePath = $"resources/{API.GetResourcePath(resourceName).Substring(API.GetResourcePath(resourceName).LastIndexOf("//") + 2)}";

        public static MySQL mySQL;
        private static GHMattiTaskScheduler taskScheduler;
        private static Settings settings;

        public MySqlDatabase()
        {
            taskScheduler = new GHMattiTaskScheduler();

            if (settings == null)
            {
                settings = new Settings();
                settings.ConvarConnectionString = API.GetConvar("mysql_connection_string", "");
                settings.ConvarDebug = API.GetConvar("mysql_debug", "false");
                string path = Path.Combine($@"{resourcePath}/settings.xml");
                XDocument xDocument = XDocument.Load(path);
                settings.XMLConfiguration = xDocument.Descendants("setting").ToDictionary(
                    setting => setting.Attribute("key").Value,
                    setting => setting.Value
                );
            }
            mySQL = new MySQL(settings, taskScheduler);
        }
    }

    public class MySqlRefactor : BaseScript
    {
        private static string _connectionString = API.GetConvar("mysql_connection_string", "");

        public MySqlRefactor()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                MySqlStringCheckError();
            }
        }

        private async void MySqlStringCheckError()
        {
            while(true)
            {
                Logger.Error($"mysql_connection_string is empty or missing");
                await Delay(1000);
            }
        }

        public async Task<dynamic> QueryAsync(string query, object parameters = null)
        {
            try
            {
                using (MySqlConnection _conn = new MySqlConnection(_connectionString))
                {
                    CommandDefinition def = new CommandDefinition(query, parameters);
                    IEnumerable<dynamic> result = await _conn.QueryAsync<dynamic>(def);
                    await _conn.CloseAsync();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return null;
            }
        }

        public async Task ExecuteAsync(string query, object parameters)
        {
            try
            {
                using (MySqlConnection _conn = new MySqlConnection(_connectionString))
                {
                    CommandDefinition def = new CommandDefinition(query, parameters);
                    await _conn.ExecuteAsync(def);
                    await _conn.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }
    }
}
