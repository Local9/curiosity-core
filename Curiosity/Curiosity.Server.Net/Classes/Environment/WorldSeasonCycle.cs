using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Data;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Classes.Environment
{
    class WorldSeasonCycle
    {
        static Server server = Server.GetInstance();
        const int ONE_MINUTE = 60000;
        
        // Settings

        // TIME
        static double _baseTime = 0;
        static double _timeOffset = 0;
        static bool _freezeTime = false;

        // OPTIONAL
        static bool _blackout = false;

        // WEATHER
        static int _baseWeatherTimer = 10;
        static int _newWeatherTimer = 10;
        static WeatherTypes _serverWeather = WeatherTypes.CLEAR;
        static bool _dynamicWeatherEnabled = true;

        static List<WeatherTypes> _springWeather = SeasonData.WeatherSpringList();
        static List<WeatherTypes> _summerWeather = SeasonData.WeatherSummerList();
        static List<WeatherTypes> _autumnWeather = SeasonData.WeatherAutumnList();

        // SEASONS
        static int _baseSeasonTimer = 60;
        static int _newSeasonTimer = 60;
        static List<Seasons> _seasonsList = SeasonData.SeasonList();
        static int _season = Server.random.Next(_seasonsList.Count);
        static Seasons _serverSeason = _seasonsList[_season];
        static int _serverTemp = Server.random.Next(60, 88);

        // TODO:
        /*
         * Client Sync
         * Server Time
         * 
         */

        static public void Init()
        {
            // must run while loading in the air
            server.RegisterEventHandler("curiosity:server:seasons:sync:connection", new Action<CitizenFX.Core.Player>(OnSeasonConnectionSync));

            server.RegisterTickHandler(OnSeasonTick);
            server.RegisterTickHandler(OnSeasonTimerTick);
            server.RegisterTickHandler(OnSeasonTimerSyncTick);
            server.RegisterTickHandler(OnSeasonWeatherTimerTick);

            _baseSeasonTimer = API.GetConvarInt("weather_season_change_minutes", 60);
            _newSeasonTimer = _baseSeasonTimer;

            _baseWeatherTimer = API.GetConvarInt("weather_change_minutes", 10);
            _newWeatherTimer = _baseWeatherTimer;

            Log.Verbose($"[WORLD WEATHER] Init");
        }

        private static async Task OnSeasonTimerSyncTick()
        {
            await Server.Delay(5000); // wait every 5 seconds
            Server.TriggerClientEvent("curiosity:server:seasons:sync:time", _baseTime, _timeOffset, _freezeTime);
        }

        private static async Task OnSeasonWeatherTimerTick()
        {
            _newWeatherTimer--; // count down

            await Server.Delay(ONE_MINUTE); // delay for one minute

            if (_newWeatherTimer > 0) return; // do nothing

            if (_dynamicWeatherEnabled)
                SetNextWeather(); // change the weather

            _newWeatherTimer = _baseWeatherTimer; // set back to base timer
        }

        private static void SetNextWeather()
        {
            List<WeatherTypes> weathers = new List<WeatherTypes>(); // create a new list

            switch(_serverSeason) // self explanitory
            {
                case Seasons.SPRING:
                    weathers = _springWeather;
                    _serverTemp = Server.random.Next(60, 88);
                    break;
                case Seasons.SUMMER:
                    weathers = _summerWeather;
                    _serverTemp = Server.random.Next(73, 102);
                    break;
                case Seasons.AUTUMN:
                    weathers = _autumnWeather;
                    _serverTemp = Server.random.Next(53, 77);
                    break;
                case Seasons.WINTER:
                    _serverWeather = WeatherTypes.XMAS;
                    _serverTemp = Server.random.Next(29, 43);
                    break;
            }

            if (_serverSeason != Seasons.WINTER) // if its winter, its always xmas, blizzard and light snow are for north yankton
                _serverWeather = Server.random.NextDouble() > 0.8 ? WeatherTypes.XMAS_STORM : WeatherTypes.XMAS;

            weathers.Clear(); // clear the list

            SyncAllUsers(); // sync all players
        }

        private static async Task OnSeasonTimerTick()
        {
            await Server.Delay(0); // awaits one CPU Cycle

            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)timeSpan.TotalSeconds;
            
            double newBaseTime = (secondsSinceEpoch / 2) + 360;
            
            if (_freezeTime)
                _timeOffset = (_timeOffset + _baseTime) - newBaseTime;

            _baseTime = newBaseTime;
        }

        private static async Task OnSeasonTick()
        {
            _newSeasonTimer--; // count down
            
            await Server.Delay(ONE_MINUTE); // delay for one minute

            if (_newSeasonTimer > 0) return; // if the timer is greater than zero, do nothing

            _season++; // increase the season by 1

            if (_season >= 4) // cannot have more seasons than whats in the list, 4 seasons known and arrays start at zero
                _season = 0; // reset to base season

            _serverSeason = _seasonsList[_season]; // set new season based on the increase

            Server.TriggerClientEvent("curiosity:client:seasons:sync:season", _serverSeason, _serverWeather, _serverTemp); // inform clients

            _newSeasonTimer = _baseSeasonTimer; // reset to server config base value
        }

        private static void SyncAllUsers()
        {
            Server.TriggerClientEvent("curiosity:client:seasons:sync:weather", (int)_serverWeather, _blackout, _serverTemp); // inform clients
            Server.TriggerClientEvent("curiosity:client:seasons:sync:season", (int)_serverSeason, (int)_serverWeather, _serverTemp); // inform clients
        }

        private static void OnSeasonConnectionSync([FromSource]CitizenFX.Core.Player player)
        {
            player.TriggerEvent("curiosity:client:seasons:sync:weather", (int)_serverWeather, _blackout, _serverTemp); // inform clients
            player.TriggerEvent("curiosity:client:seasons:sync:season", (int)_serverSeason, (int)_serverWeather, _serverTemp); // inform clients   
        }
    }
}
