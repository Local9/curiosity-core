using CitizenFX.Core;
using CitizenFX.Core.Native;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using GHMatti.Utilities;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Curiosity.Server.net.Database
{
    public class DatabaseSettings : BaseScript
    {
        internal static string resourceName = API.GetCurrentResourceName();
        internal static string resourcePath = $"resources/{API.GetResourcePath(resourceName).Substring(API.GetResourcePath(resourceName).LastIndexOf("//") + 2)}";

        public MySQL mySQL;
        private GHMattiTaskScheduler taskScheduler;
        private Settings settings;

        private static DatabaseSettings _database;

        public static DatabaseSettings GetInstance()
        {
            return _database;
        }

        public DatabaseSettings()
        {
            taskScheduler = new GHMattiTaskScheduler();

            if (settings == null)
            {
                settings = new Settings();
                settings.ConvarConnectionString = API.GetConvar("mysql_connection_string", "");
                settings.ConvarDebug = API.GetConvar("mysql_debug", "false");
                string path = Path.Combine($@"{resourcePath}/server/settings.xml");
                XDocument xDocument = XDocument.Load(path);
                settings.XMLConfiguration = xDocument.Descendants("setting").ToDictionary(
                    setting => setting.Attribute("key").Value,
                    setting => setting.Value
                );
            }
            mySQL = new MySQL(settings, taskScheduler);
            _database = this;
        }

        public async Task<int> GetServerId(int serverKey)
        {
            string query = "select serverId from servers where servers.key = @key;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@key", serverKey);

            using (var result = mySQL.QueryResult(query, myParams))
            {
                ResultSet keyValuePairs = await result;

                await Delay(0);

                if (keyValuePairs.Count == 0)
                {
                    Debug.WriteLine("SERVER ID NOT FOUND!");
                    return 0;
                }

                int serverId = 0;

                foreach (Dictionary<string, object> keyValues in keyValuePairs)
                {
                    serverId = int.Parse($"{keyValues["serverId"]}");
                }
                return serverId;
            }
        }
    }
}
