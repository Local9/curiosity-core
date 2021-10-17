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
        public static WorldManager WorldInstance;
        DateTime lastTimeWeatherUpdated = DateTime.UtcNow;

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
            { Region.NorthYankton, WeatherType.XMAS },
            { Region.CayoPericoIsland, WeatherType.CLEAR },
        };

        // TIME
        double _baseTime = 0;
        double _timeOffset = 0;
        DateTime lastTimeTick = DateTime.UtcNow;
        DateTime lastTimeSyncTick = DateTime.UtcNow;

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

            EventSystem.GetModule().Attach("weather:is:halloween", new EventCallback(metadata =>
            {
                return WeatherData.IsHalloween();
            }));

            RandomiseWeather();
            WeatherDebugOutput();
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
                regionWeatherType[keyValuePair.Key] = weatherType;
            }
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
                    regionWeatherType[kvp.Key] = WeatherType.XMAS;
                }
                else if (isHalloween)
                {
                    regionWeatherType[kvp.Key] = WeatherType.HALLOWEEN;
                }
                else
                {
                    WeatherSeason season = WeatherData.GetCurrentSeason();
                    List<WeatherType> weatherTypes = WeatherData.SeasonalWeather[season];

                    if (numberOfWeatherCyclesProcessed % 3 == 0 && season == WeatherSeason.SUMMER && Utility.RANDOM.Bool(.1f))
                    {
                        weatherTypes.Add(WeatherType.RAIN);
                    }

                    if (numberOfWeatherCyclesProcessed % 3 == 0 && season == WeatherSeason.AUTUMN && Utility.RANDOM.Bool(.1f))
                    {
                        if (Utility.RANDOM.Bool(.25f))
                            weatherTypes.Add(WeatherType.RAIN);

                        if(Utility.RANDOM.Bool(.1f))
                            weatherTypes.Add(WeatherType.THUNDER);
                    }

                    regionWeatherType[kvp.Key] = weatherTypes[Utility.RANDOM.Next(weatherTypes.Count)];
                }
            }
            numberOfWeatherCyclesProcessed++;

            if (numberOfWeatherCyclesProcessed > 7)
                numberOfWeatherCyclesProcessed = 0;
        }

        [TickHandler]
        private async Task OnWeatherTick()
        {
            if (DateTime.UtcNow > lastTimeWeatherUpdated)
            {
                if (IsWeatherFrozen) return;

                RandomiseWeather();

                lastTimeWeatherUpdated = DateTime.UtcNow.AddMinutes(60);
            }

            await BaseScript.Delay(1000);
        }

        [TickHandler]
        private async Task OnWorldTimeTick()
        {
            if (DateTime.UtcNow > lastTimeTick)
            {
                TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1);
                int secondsSinceEpoch = (int)timeSpan.TotalSeconds;

                double newBaseTime = (secondsSinceEpoch / 2) + 360;

                _baseTime = IsTimeFrozen ? _baseTime : newBaseTime;

                lastTimeTick = DateTime.UtcNow.AddMilliseconds(1000);
            }

            await BaseScript.Delay(250);
        }

        [TickHandler]
        private async Task OnWorldTimeSyncTick()
        {
            if (DateTime.UtcNow > lastTimeSyncTick) {
                EventSystem.GetModule().SendAll("world:time:sync", _baseTime, _timeOffset);
                lastTimeSyncTick = DateTime.UtcNow.AddSeconds(5);
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
