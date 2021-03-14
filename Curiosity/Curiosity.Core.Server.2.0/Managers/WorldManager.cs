using CitizenFX.Core;
using CitizenFX.Core.Native;
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
        public static WorldManager WorldInstance;
        private const int WEATHER_UPDATE_MS = 60000;
        DateTime lastTimeWeatherUpdated = DateTime.Now;

        public bool IsWeatherFrozen = false;
        bool _debugWeather
        {
            get
            {
                return API.GetConvarInt("debug_weather", 0) == 1;
            }
        }

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

        public bool IsTimeFrozen { get; internal set; }

        public override void Begin()
        {
            WorldInstance = this;

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
            WeatherDebugOutput();
        }

        private void WeatherDebugOutput()
        {
            if (_debugWeather)
            {
                Logger.Debug($"Weather Region Init START");

                foreach (KeyValuePair<Region, WeatherType> kvp in regionWeatherType)
                {
                    Logger.Debug($"Region: {kvp.Key} : {kvp.Value}");
                }

                Logger.Debug($"Weather Region Init END");
            }
        }

        public void SetWeatherForAllRegions(WeatherType weatherType)
        {
            Dictionary<Region, WeatherType> regionWeatherTypeCopy = new Dictionary<Region, WeatherType>(regionWeatherType);
            foreach (KeyValuePair<Region, WeatherType> keyValuePair in regionWeatherTypeCopy)
            {
                regionWeatherType[keyValuePair.Key] = weatherType;
            }
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
                if (IsWeatherFrozen) return;

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

                _baseTime = IsTimeFrozen ? _baseTime : newBaseTime;

                lastTimeTick = DateTime.Now;
            }

            await BaseScript.Delay(250);
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

        public void ShiftTimeToHour(int inHour)
        {
            inHour = inHour >= 24 ? 0 : inHour;
            _timeOffset = _timeOffset - (((((_baseTime + _timeOffset) / 60) % 24) - inHour) * 60);
        }

        public void ShiftTimeToMinute(int inMins)
        {
            inMins = inMins >= 60 ? 0 : inMins;
            _timeOffset = _timeOffset - (((_baseTime + _timeOffset) % 60) - (inMins * 1000));
        }
    }
}
