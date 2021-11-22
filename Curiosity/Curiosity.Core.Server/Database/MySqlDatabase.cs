using CitizenFX.Core;
using CitizenFX.Core.Native;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using GHMatti.Utilities;
using System.IO;
using System.Linq;
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
}
