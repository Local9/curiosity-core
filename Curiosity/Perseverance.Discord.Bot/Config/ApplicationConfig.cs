using Perseverance.Discord.Bot.Entities;

namespace Perseverance.Discord.Bot.Config
{
    public class ApplicationConfig
    {
        static Configuration _configuration;

        public static async Task<Configuration> GetConfig()
        {
            string json = string.Empty;
            using (FileStream fs = File.OpenRead("config.json"))
            using (StreamReader sr = new StreamReader(fs))
                json = await sr.ReadToEndAsync();

            if (string.IsNullOrEmpty(json))
                throw new Exception("Unable to load config.json");

            _configuration = JsonConvert.DeserializeObject<Configuration>(json);

            return _configuration;
        }

        public static List<ulong> GetDonatorRoles => _configuration.DonatorRoleList;
        public static Dictionary<string, ulong> DonatorRoles => _configuration.DonatorRoles;
        public static List<Server> Servers => _configuration.Servers;
    }

}
