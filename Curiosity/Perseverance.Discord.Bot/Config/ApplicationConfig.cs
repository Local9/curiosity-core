using Newtonsoft.Json;
using Perseverance.Discord.Bot.Entities;

namespace Perseverance.Discord.Bot.Config
{
    public class ApplicationConfig
    {
        static Configuration _configuration;

        public static async Task<Configuration> GetConfig()
        {
            if (_configuration is not null) return _configuration;

            string json = string.Empty;
            using (FileStream fs = File.OpenRead("config.json"))
            using (StreamReader sr = new StreamReader(fs))
                json = await sr.ReadToEndAsync();

            if (string.IsNullOrEmpty(json))
                throw new Exception("Unable to load config.json");

            // This warning is FUCKING STUPID
#pragma warning disable CS8601 // Possible null reference assignment.
            _configuration = JsonConvert.DeserializeObject<Configuration>(json);
#pragma warning restore CS8601 // Possible null reference assignment.

            // AND HOW WILL THIS EVER BE NULL?! I'M THROWING AN ERROR IF NOTHING RETURNS!
            // Fucking stupid, really fucking stupid
            return _configuration ?? new();
        }
    }

}
