using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

            Instance.ExportDictionary.Add("IsCloseToLocation", new Func<string, float, float, float, float, bool>(
                (eventName, posX, posY, posZ, dist) => {
                    Vector3 vector = new Vector3(posX, posY, posZ);
                    return IsNearLocation(vector, eventName, dist);
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

        public bool IsNearLocation(Vector3 position, string eventName, float distance = 0f)
        {
            foreach(Location location in configCache.Locations)
            {
                if (location.Markers.Count == 0)
                    continue;

                foreach(Marker marker in location.Markers)
                {
                    if (marker.Event == eventName)
                    {
                        foreach(Position pos in marker.Positions)
                        {
                            Vector3 posV = pos.AsVector();
                            float dist = Vector3.Distance(position, posV);
                            float distanceToCheck = (distance > 0f) ? distance : marker.ContextAoe;
                            bool distanceValid = dist <= distanceToCheck;

                            Logger.Debug($"Position {posV} Close: {distanceValid}");

                            if (distanceValid)
                            {
                                return true;
                            }
                        };
                    }
                }
            }

            return false;
        }
    }
}
