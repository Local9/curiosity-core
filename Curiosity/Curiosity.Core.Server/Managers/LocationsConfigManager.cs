using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Server.Managers
{
    public class LocationsConfigManager : Manager<LocationsConfigManager>
    {
        public LocationConfig configCache = new();
        // public Dictionary<string, List<Position>> eventCache = new();
        public Dictionary<SpawnType, List<Position>> spawnCache = new();

        public override void Begin()
        {
            EventSystem.GetModule().Attach("config:locations", new EventCallback(metadata =>
            {
                return GetLocations();
            }));

            Instance.ExportDictionary.Add("IsCloseToLocation", new Func<string, float, float, float, float, bool>(
                (eventName, posX, posY, posZ, dist) => {
                    Vector3 vector = new Vector3(posX, posY, posZ);
                    return IsNearEventLocation(vector, eventName, dist);
                }));

            GetLocations();
        }

        private LocationConfig GetLocationConfig()
        {
            LocationConfig config = new();

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

                    if (config.Locations.Count > 0)
                    {
                        foreach(Location location in config.Locations)
                        {
                            if (location.SpawnType == SpawnType.Unknown
                                || location.SpawnType == SpawnType.Hospital)
                                continue;

                            if (!spawnCache.ContainsKey(location.SpawnType))
                            {
                                if (location.Spawns.Count > 0)
                                    spawnCache.Add(location.SpawnType, location.Spawns);
                            }
                            else
                            {
                                foreach(Position position in location.Spawns)
                                {
                                    spawnCache[location.SpawnType].Add(position);
                                }
                            }
                        }
                    }
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

        public bool IsNearEventLocation(Vector3 position, string eventName, float distance = 0f)
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

            Logger.Debug($"Possible server config does not match the client config");

            return false;
        }

        public Location NearestEventLocation(Vector3 position, string eventName, float distance = 0f)
        {
            foreach (Location location in configCache.Locations)
            {
                if (location.Markers.Count == 0)
                    continue;

                foreach (Marker marker in location.Markers)
                {
                    if (marker.Event == eventName)
                    {
                        Logger.Debug($"Event found");

                        foreach (Position pos in marker.Positions)
                        {
                            Vector3 posV = pos.AsVector();
                            float dist = Vector3.Distance(position, posV);
                            float distanceToCheck = (distance > 0f) ? distance : marker.ContextAoe;
                            bool distanceValid = dist <= distanceToCheck;

                            Logger.Debug($"Position {posV}, Close: {distanceValid}");

                            if (distanceValid)
                            {
                                return location;
                            }
                        };
                    }
                }
            }

            Logger.Debug($"Possible server config does not match the client config");

            return null;
        }

        public Position NearestSpawnPosition(Vector3 position, SpawnType spawnType)
        {
            try
            {
                if (!spawnCache.ContainsKey(spawnType))
                {
                    Logger.Error($"Key '{spawnType}' not found");

                    string keys = string.Empty;
                    spawnCache.Keys.ToList().ForEach(t => { keys += $"{t},"; });
                    Logger.Error($"Keys: {keys}");

                    return null;
                }


                List<Position> positions = spawnCache[spawnType];

                if (positions.Count == 0)
                {
                    return null;
                }

                return positions.OrderBy(x => Vector3.Distance(position, x.AsVector())).First();
            }
            catch (Exception ex)
            {
                Logger.Error($"NearestSpawnPosition: {ex.Message}");
                return null;
            }
        }

        public List<Position> NearestSpawnPositions(Vector3 position, SpawnType spawnType, float distance = 50f)
        {
            try
            {
                if (!spawnCache.ContainsKey(spawnType))
                {
                    Logger.Error($"Key '{spawnType}' not found");

                    string keys = string.Empty;
                    spawnCache.Keys.ToList().ForEach(t => { keys += $"{t},"; });
                    Logger.Error($"Keys: {keys}");

                    return null;
                }


                List<Position> positions = spawnCache[spawnType];

                if (positions.Count == 0)
                {
                    return null;
                }

                return positions
                    .Where(x => Vector3.Distance(position, x.AsVector()) < distance)
                    .OrderBy(x => Vector3.Distance(position, x.AsVector())).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error($"NearestSpawnPosition: {ex.Message}");
                return null;
            }
        }
    }
}
