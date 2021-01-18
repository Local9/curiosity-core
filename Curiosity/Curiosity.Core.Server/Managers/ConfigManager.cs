using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Curiosity.Core.Server.Managers
{
    public class ConfigManager : Manager<ConfigManager>
    {
        public static ConfigManager ConfigInstance;
        public LocationConfig configCache = new LocationConfig();

        public override void Begin()
        {
            ConfigInstance = this;

            EventSystem.GetModule().Attach("config:locations", new EventCallback(metadata =>
            {
                return GetLocations();
            }));
        }

        private LocationConfig GetLocationConfig()
        {
            LocationConfig config = new LocationConfig();

            string jsonFile = API.LoadResourceFile(API.GetCurrentResourceName(), "config/locations.json"); // Fuck you VS2019 UTF8 BOM

            try
            {
                if (string.IsNullOrEmpty(jsonFile))
                {
                    Logger.Error($"locations.json file is empty or does not exist, please fix this");
                }
                else
                {
                    config = JsonConvert.DeserializeObject<LocationConfig>(jsonFile);
                    configCache = config;
                }
            }
            catch(Exception ex)
            {
                Logger.Error($"Location JSON File Exception\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }

        private List<Location> GetLocations()
        {
            return GetLocationConfig().Locations;
        }
    }
}
