﻿using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Server.Managers
{
    public class WorldManager : Manager<WorldManager>
    {
        public static WorldManager WorldInstance;

        long lastTimeWeatherUpdated = PluginManager.GetGameTime;

        PlayerList players => PluginManager.PlayersList;

        int numberOfWeatherCyclesProcessed = 0;

        public bool IsWeatherFrozen = false;

        Dictionary<Region, WeatherType> regionWeatherType = new Dictionary<Region, WeatherType>()
        {
            { Region.BeachCoastal, WeatherType.CLEAR },
            { Region.BeachLosSantos, WeatherType.CLEAR },
            { Region.CentralLosSantos, WeatherType.CLEAR },
            { Region.EasternValley, WeatherType.CLEAR },
            { Region.GrandSenoraDesert, WeatherType.CLEAR },
            { Region.NorthernMoutains, WeatherType.CLEAR },
            { Region.NorthLosSantos, WeatherType.CLEAR },
            { Region.NorthLosSantosHills, WeatherType.CLEAR },
            { Region.Paleto, WeatherType.CLEAR },
            { Region.SouthLosSantos, WeatherType.CLEAR },
            { Region.Zancudo, WeatherType.CLEAR },
            { Region.NorthYankton, WeatherType.SNOWLIGHT },
            { Region.CayoPericoIsland, WeatherType.CLEAR },
        };

        // TIME

        public override void Begin()
        {
            WorldInstance = this;

            EventSystem.Attach("weather:sync:regions", new EventCallback(metadata =>
            {
                return regionWeatherType;
            }));

            EventSystem.Attach("weather:sync", new EventCallback(metadata =>
            {
                SubRegion subRegion = (SubRegion)metadata.Find<int>(0);
                Region region = MapRegions.RegionBySubRegion[subRegion];

                CuriosityWeather weather = new CuriosityWeather();
                weather.WeatherType = regionWeatherType[region];
                weather.Season = WeatherData.GetCurrentSeason();

                return weather;
            }));

            EventSystem.Attach("weather:is:winter", new EventCallback(metadata =>
            {
                return WeatherData.GetCurrentSeason() == WeatherSeason.WINTER;
            }));

            EventSystem.Attach("weather:is:halloween", new EventCallback(metadata =>
            {
                return WeatherData.IsHalloween();
            }));

            EventSystem.Attach("world:routing:northYankton", new EventCallback(metadata =>
            {
                string playerId = $"{metadata.Sender}";
                SetPlayerRoutingBucket(playerId, 4);

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                curiosityUser.RoutingBucket = 4;
                return null;
            }));

            EventSystem.Attach("world:routing:island", new EventCallback(metadata =>
            {
                string playerId = $"{metadata.Sender}";
                SetPlayerRoutingBucket(playerId, 3);

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                curiosityUser.RoutingBucket = 3;
                return null;
            }));

            EventSystem.Attach("world:routing:city", new EventCallback(metadata =>
            {
                string playerId = $"{metadata.Sender}";
                SetPlayerRoutingBucket(playerId, 0);

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                curiosityUser.RoutingBucket = 0;
                return null;
            }));

            RandomiseWeather();
            WeatherDebugOutput();

            ToggleChristmasResources();
        }

        public void WeatherDebugOutput()
        {
            Logger.Debug($"Weather Region Init START");

            foreach (KeyValuePair<Region, WeatherType> kvp in regionWeatherType)
            {
                Logger.Debug($"Region: {kvp.Key} : {kvp.Value}");
            }

            Logger.Debug($"Weather Region Init END");
        }

        public void SetWeatherForAllRegions(WeatherType weatherType)
        {
            Dictionary<Region, WeatherType> regionWeatherTypeCopy = new Dictionary<Region, WeatherType>(regionWeatherType);
            foreach (KeyValuePair<Region, WeatherType> keyValuePair in regionWeatherTypeCopy)
            {
                if (keyValuePair.Key == Region.CayoPericoIsland || keyValuePair.Key == Region.NorthYankton) continue;
                regionWeatherType[keyValuePair.Key] = weatherType;
            }
            EventSystem.SendAll("world:server:weather:sync", regionWeatherType);
        }

        void RandomiseWeather()
        {
            bool isSnowDay = WeatherData.IsSnowDay();
            bool isHalloween = WeatherData.IsHalloween();

            Dictionary<Region, WeatherType> RegionWeatherTypeCopy = new Dictionary<Region, WeatherType>(regionWeatherType);

            foreach (KeyValuePair<Region, WeatherType> kvp in RegionWeatherTypeCopy)
            {
                if (kvp.Key == Region.CayoPericoIsland || kvp.Key == Region.NorthYankton)
                {
                    List<WeatherType> weatherTypes = WeatherData.RegionWeather[kvp.Key];
                    regionWeatherType[kvp.Key] = weatherTypes[Utility.RANDOM.Next(weatherTypes.Count)];
                }
                else if (isSnowDay)
                {
                    regionWeatherType[kvp.Key] = WeatherType.CHRISTMAS;
                }
                else if (isHalloween)
                {
                    regionWeatherType[kvp.Key] = WeatherType.HALLOWEEN;
                }
                else
                {
                    WeatherSeason season = WeatherData.GetCurrentSeason();
                    List<WeatherType> weatherTypes = new List<WeatherType>(WeatherData.SeasonalWeather[season]);

                    if (numberOfWeatherCyclesProcessed == 3 && season == WeatherSeason.SPRING)
                    {
                        if (Utility.RANDOM.Bool(.2f))
                            weatherTypes.Add(WeatherType.RAINING);
                        if (Utility.RANDOM.Bool(.1f))
                            weatherTypes.Add(WeatherType.CLEARING);
                    }

                    if (numberOfWeatherCyclesProcessed == 3 && season == WeatherSeason.SUMMER && Utility.RANDOM.Bool(.1f))
                    {
                        weatherTypes.Add(WeatherType.RAINING);
                    }

                    if (numberOfWeatherCyclesProcessed == 3 && season == WeatherSeason.AUTUMN && Utility.RANDOM.Bool(.1f))
                    {
                        if (Utility.RANDOM.Bool(.15f))
                            weatherTypes.Add(WeatherType.RAINING);

                        if (Utility.RANDOM.Bool(.05f))
                            weatherTypes.Add(WeatherType.THUNDERSTORM);
                    }

                    regionWeatherType[kvp.Key] = weatherTypes[Utility.RANDOM.Next(weatherTypes.Count)];

                    ToggleChristmasResources();
                }
            }

            numberOfWeatherCyclesProcessed++;

            if (numberOfWeatherCyclesProcessed > 7)
                numberOfWeatherCyclesProcessed = 0;

            EventSystem.SendAll("world:server:weather:sync", regionWeatherType);
        }

        void ToggleChristmasResources()
        {
            if (PluginManager.ActiveUsers.Count > 0) return;

            WeatherSeason season = WeatherData.GetCurrentSeason();
            bool isWinter = season == WeatherSeason.WINTER;

            string stateAlamo = GetResourceState("nve_iced_alamo");
            string stateXmas = GetResourceState("nve_xmas");

            if (stateAlamo == "started" && !isWinter)
                StopResource("nve_iced_alamo");

            if (stateXmas == "started" && !isWinter)
                StopResource("nve_xmas");

            if (stateAlamo == "stopped" && isWinter)
                StartResource("nve_iced_alamo");

            if (stateXmas == "stopped" && isWinter)
                StartResource("nve_xmas");
        }

        [TickHandler]
        private async Task OnWeatherTick()
        {
            if ((PluginManager.GetGameTime - (1000 * 60) * 48) > lastTimeWeatherUpdated)
            {
                if (IsWeatherFrozen) return;

                RandomiseWeather();

                lastTimeWeatherUpdated = PluginManager.GetGameTime;
            }

            await BaseScript.Delay(10000);
        }
    }
}
