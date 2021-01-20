using CitizenFX.Core.Native;
using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Curiosity.Core.Server.Extensions;
using System.Linq;

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

        public bool IsNearLocation(Vector3 position, string eventName, float distance)
        {
            foreach(Location location in configCache.Locations)
            {
                if (location.Markers.Count == 0)
                    return false;

                foreach(Marker marker in location.Markers)
                {
                    if (marker.Event == eventName)
                    {
                        foreach(Position pos in marker.Positions)
                        {
                            float dist = Vector3.Distance(position, pos.AsVector());
                            bool distanceValid = dist <= distance;

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

        private Vector3 FindClosestPoint(Vector3 startingPoint, IEnumerable<Vector3> points)
        {
            if (points.Count() == 0) return Vector3.Zero;

            return points.OrderBy(x => Vector3.Distance(startingPoint, x)).First();
        }
    }
}
