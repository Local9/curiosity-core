using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers
{
    public class WorldManager : Manager<WorldManager>
    {
        private const int WEATHER_UPDATE_MS = 60000;
        DateTime lastTimeWeatherUpdated = DateTime.Now;

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
        };

        // TIME
        double _baseTime = 0;
        double _timeOffset = 0;
        DateTime lastTimeTick = DateTime.Now;
        DateTime lastTimeSyncTick = DateTime.Now;

        public override void Begin()
        {
            EventSystem.GetModule().Attach("weather:sync", new EventCallback(metadata =>
            {
                SubRegion subRegion = (SubRegion)metadata.Find<int>(0);
                Region region = MapRegions.RegionBySubRegion[subRegion];

                CuriosityWeather weather = new CuriosityWeather();
                weather.WeatherType = regionWeatherType[region];
                weather.Season = WeatherData.GetCurrentSeason();

                return weather;
            }));

            // Need to add a time sync

            RandomiseWeather();

            Logger.Debug($"Weather Region Init START");

            foreach(KeyValuePair<Region, WeatherType> kvp in regionWeatherType)
            {
                Logger.Debug($"Region: {kvp.Key} : {kvp.Value}");
            }

            Logger.Debug($"Weather Region Init END");
        }

        void RandomiseWeather()
        {
            bool isSnowDay = WeatherData.IsSnowDay();

            Dictionary<Region, WeatherType> RegionWeatherTypeCopy = new Dictionary<Region, WeatherType>(regionWeatherType);

            foreach (KeyValuePair<Region, WeatherType> kvp in RegionWeatherTypeCopy)
            {
                if (isSnowDay)
                {
                    regionWeatherType[kvp.Key] = WeatherType.XMAS;
                }
                else
                {
                    WeatherSeason season = WeatherData.GetCurrentSeason();
                    List<WeatherType> weatherTypes = WeatherData.SeasonalWeather[season];
                    regionWeatherType[kvp.Key] = weatherTypes[Utility.RANDOM.Next(weatherTypes.Count)];
                }
            }
        }

        [TickHandler]
        private async Task OnWeatherTick()
        {
            if (DateTime.Now.Subtract(lastTimeWeatherUpdated).TotalMinutes >= 60)
            {
                RandomiseWeather();
            }

            await BaseScript.Delay(WEATHER_UPDATE_MS);
        }

        [TickHandler]
        private async Task OnWorldTimeTick()
        {
            if (DateTime.Now.Subtract(lastTimeTick).TotalMilliseconds >= 500)
            {

                TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1);
                int secondsSinceEpoch = (int)timeSpan.TotalSeconds;

                double newBaseTime = (secondsSinceEpoch / 2) + 360;

                _baseTime = newBaseTime;

                lastTimeTick = DateTime.Now;
            }

            await BaseScript.Delay(100);
        }

        [TickHandler]
        private async Task OnWorldTimeSyncTick()
        {
            if (DateTime.Now.Subtract(lastTimeSyncTick).TotalSeconds >= 5) {
                EventSystem.GetModule().SendAll("world:time", _baseTime, _timeOffset);
                lastTimeSyncTick = DateTime.Now;
            }

            await BaseScript.Delay(1000);
        }
    }
}
