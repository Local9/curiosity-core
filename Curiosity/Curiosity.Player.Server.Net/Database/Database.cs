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
    public class Database : BaseScript
    {
        internal static string resourceName = API.GetCurrentResourceName();
        internal static string resourcePath = $"resources/{API.GetResourcePath(resourceName).Substring(API.GetResourcePath(resourceName).LastIndexOf("//") + 2)}";

        public static MySQL mySQL;
        private static GHMattiTaskScheduler taskScheduler;
        private static Settings settings;

        public static void Init()
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
        }

        public static async Task<int> ServerIdExists(int serverId)
        {
            string query = "select serverId from curiosity.server where server.serverId = @serverId;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@serverId", serverId);

            using (var result = mySQL.QueryResult(query, myParams))
            {
                ResultSet keyValuePairs = await result;

                await Delay(0);

                if (keyValuePairs.Count == 0)
                {
                    Log.Error("SERVER ID NOT FOUND!");
                    return 0;
                }

                foreach (Dictionary<string, object> keyValues in keyValuePairs)
                {
                    serverId = int.Parse($"{keyValues["serverId"]}");
                }
                return serverId;
            }
        }
    }
}
