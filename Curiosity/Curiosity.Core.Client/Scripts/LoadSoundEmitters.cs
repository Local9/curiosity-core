using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Extensions;
using Newtonsoft.Json;

namespace Curiosity.Core.Client.Scripts
{
    internal static class LoadSoundEmitters
    {
        static List<SoundEmitter> _configCache;
        const string CONFIG_LOCATION = $"/data/durty/soundEmitters.json";

        private static List<SoundEmitter> Get()
        {
            List<SoundEmitter> config = new();

            try
            {
                if (_configCache is not null)
                    return _configCache;

                string file = API.LoadResourceFile(API.GetCurrentResourceName(), CONFIG_LOCATION);

                if (string.IsNullOrEmpty(file))
                {
                    Logger.Error($"Failed to load file soundEmitters.json");
                    return config;
                }

                _configCache = JsonConvert.DeserializeObject<List<SoundEmitter>>(file);
                Logger.Info($"----> {_configCache.Count} emitters loaded.");

                return _configCache;
            }
            catch (Exception ex)
            {
                Logger.Error($"Config JSON File Exception\nFailed to load: {CONFIG_LOCATION}\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }

        public static List<SoundEmitter> GetEmitters => Get();

        public static List<SoundEmitter> GetClosestEmitters(this Ped ped)
        {
            Vector3 pos = ped.Position;
            List<SoundEmitter> lst = new();

            foreach (SoundEmitter soundEmitter in GetEmitters)
            {
                if (pos.Distance(soundEmitter.Position.AsVector()) < 10f && !lst.Contains(soundEmitter))
                    lst.Add(soundEmitter);
            }
            return lst;
        }
    }
}
