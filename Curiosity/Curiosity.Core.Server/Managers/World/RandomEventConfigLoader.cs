using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Environment;
using Newtonsoft.Json;

namespace Curiosity.Core.Server.Managers.World
{
    public static class RandomEventConfigLoader
    {
        const string CONFIG_LOCATION = $"/config/random-event.json";
        static RandomEventConfig _configCache;

        private static RandomEventConfig Get()
        {
            RandomEventConfig config = new();

            try
            {
                if (_configCache is not null)
                    return _configCache;

                string file = API.LoadResourceFile(API.GetCurrentResourceName(), CONFIG_LOCATION);
                _configCache = JsonConvert.DeserializeObject<RandomEventConfig>(file);
                return _configCache;
            }
            catch (Exception ex)
            {
                Logger.Error($"Config JSON File Exception\nFailed to load: {CONFIG_LOCATION}\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }

        public static List<RandomEvent> GetEvents => Get().Events;
    }
}
