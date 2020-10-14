using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Data;
using Curiosity.Server.net.Extensions;
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
        static bool _dynamicWeatherEnabled = true;

        static List<WeatherTypes> _springWeather = SeasonData.WeatherSpringList();
        static List<WeatherTypes> _summerWeather = SeasonData.WeatherSummerList();
        static List<WeatherTypes> _autumnWeather = SeasonData.WeatherAutumnList();
        static WeatherTypes _serverWeather = _summerWeather[Server.random.Next(_summerWeather.Count)];

        // SEASONS
        static int _baseSeasonTimer = 60;
        static int _newSeasonTimer = 60;
        static List<Seasons> _seasonsList = SeasonData.SeasonList();
        static int _season = Server.random.Next(_seasonsList.Count);
        static Seasons _serverSeason = _seasonsList[_season];
        static int _serverTemp = Server.random.Next(60, 88);
        static bool _hasChangedSeason = false;

        static public void Init()
        {
            // must run while loading in the air
            server.RegisterEventHandler("curiosity:server:seasons:sync:connection", new Action<CitizenFX.Core.Player>(OnSeasonConnectionSync));

            // server.RegisterTickHandler(OnSeasonTick);
            server.RegisterTickHandler(OnWorldTimeTick);
            server.RegisterTickHandler(OnSeasonTimerSyncTick);
            server.RegisterTickHandler(OnSeasonWeatherTimerTick);

            _baseSeasonTimer = API.GetConvarInt("weather_season_change_minutes", 60);
            _newSeasonTimer = _baseSeasonTimer;

            _baseWeatherTimer = API.GetConvarInt("weather_change_minutes", 10);
            _newWeatherTimer = _baseWeatherTimer;

            Log.Verbose($"[WORLD WEATHER] Init");

            API.RegisterCommand("time", new Action<int, List<object>, string>(OnTimeCommand), false);
            API.RegisterCommand("weather", new Action<int, List<object>, string>(OnWeatherCommand), false);
            API.RegisterCommand("blackout", new Action<int, List<object>, string>(OnBlackoutCommand), false);
            API.RegisterCommand("morning", new Action<int, List<object>, string>(OnMorningCommand), false);
            API.RegisterCommand("noon", new Action<int, List<object>, string>(OnNoonCommand), false);
            API.RegisterCommand("evening", new Action<int, List<object>, string>(OnEveningCommand), false);
            API.RegisterCommand("night", new Action<int, List<object>, string>(OnNightCommand), false);
            API.RegisterCommand("freezeweather", new Action<int, List<object>, string>(OnFreezeWeatherCommand), false);
            API.RegisterCommand("freezetime", new Action<int, List<object>, string>(OnFreezeTimeCommand), false);
        }

        private static void OnFreezeTimeCommand(int playerHandle, List<object> args, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (!session.IsDeveloper) return;

            _freezeTime = !_freezeTime;

            session.Player.NotificationCuriosity("Time", string.Format("Time: {0}", _freezeTime ? "~g~Enabled" : "~r~Disabled"));

            SyncAllUsers();
        }

        private static void OnFreezeWeatherCommand(int playerHandle, List<object> args, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (!session.IsDeveloper) return;

            _dynamicWeatherEnabled = !_dynamicWeatherEnabled;

            session.Player.NotificationCuriosity("Weather", string.Format("Weather: {0}", _dynamicWeatherEnabled ? "~g~Enabled" : "~r~Disabled"));

            SyncAllUsers();
        }

        private static void OnEveningCommand(int playerHandle, List<object> args, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (!session.IsDeveloper) return;

            ShiftTimeToHour(18);
            ShiftTimeToMinute(0);
            session.Player.NotificationCuriosity("Time Shift", "Time set to ~y~evening~s~.");

            SyncAllUsers();
        }

        private static void OnNightCommand(int playerHandle, List<object> args, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (!session.IsDeveloper) return;

            ShiftTimeToHour(23);
            ShiftTimeToMinute(0);
            session.Player.NotificationCuriosity("Time Shift", "Time set to ~y~night~s~.");

            SyncAllUsers();
        }

        private static void OnNoonCommand(int playerHandle, List<object> args, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (!session.IsDeveloper) return;

            ShiftTimeToHour(12);
            ShiftTimeToMinute(0);
            session.Player.NotificationCuriosity("Time Shift", "Time set to ~y~noon~s~.");

            SyncAllUsers();
        }

        private static void OnMorningCommand(int playerHandle, List<object> args, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (!session.IsDeveloper) return;

            ShiftTimeToHour(9);
            ShiftTimeToMinute(0);
            session.Player.NotificationCuriosity("Time Shift", "Time set to ~y~morning~s~.");

            SyncAllUsers();
        }

        private static void OnBlackoutCommand(int playerHandle, List<object> args, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (!session.IsDeveloper) return;

            _blackout = !_blackout;

            session.Player.NotificationCuriosity("Blackout", string.Format("Blackout: {0}", _blackout ? "~g~Enabled" : "~r~Disabled"));

            SyncAllUsers();
        }

        private static void OnTimeCommand(int playerHandle, List<object> args, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (!session.IsDeveloper) return;

            if (args.Count < 2)
            {
                session.Player.NotificationCuriosity("Error", "Missing arguments~n~/time HH MM");
                return;
            }

            int inHour;
            int inMins;

            if (!int.TryParse($"{args[0]}", out inHour))
                inHour = 0;

            if (!int.TryParse($"{args[1]}", out inMins))
                inMins = 0;

            inHour = inHour > 24 ? 0 : inHour;
            inMins = inMins > 60 ? 0 : inMins;

            ShiftTimeToHour(inHour);
            ShiftTimeToMinute(inMins);

            SyncAllUsers();
        }

        private static void ShiftTimeToHour(int inHour)
        {
            _timeOffset = _timeOffset - (((((_baseTime + _timeOffset) / 60) % 24) - inHour) * 60);
        }

        private static void ShiftTimeToMinute(int inMins)
        {
            _timeOffset = _timeOffset - (((_baseTime + _timeOffset) % 60) - (inMins * 1000 ));
        }

        private static void OnWeatherCommand(int playerHandle, List<object> args, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (!session.IsDeveloper) return;

            if (args.Count < 1)
            {
                session.Player.NotificationCuriosity("Error", "Missing weather argument");
                return;
            }

            string weather = $"{args[0]}".ToUpper();
            WeatherTypes weatherType;

            if (!Enum.TryParse(weather, out weatherType))
            {
                session.Player.NotificationCuriosity("Error", "Invalid weather argument");
                return;
            }

            _serverWeather = weatherType;
            _newWeatherTimer = _baseWeatherTimer;

            session.Player.NotificationCuriosity($"Weather", $"Weather changed: {_serverWeather}");

            SyncAllUsers();
        }

        private static async Task OnSeasonTimerSyncTick()
        {
            await Server.Delay(5000); // wait every 5 seconds
            Server.TriggerClientEvent("curiosity:client:seasons:sync:time", _baseTime, _timeOffset, _freezeTime);
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

        private static async void SetNextWeather()
        {
            List<WeatherTypes> weathers = new List<WeatherTypes>(); // create a new list

            switch(_serverSeason) // self explanitory
            {
                case Seasons.SPRING:
                    weathers = SeasonData.WeatherSpringList();
                    _serverTemp = Server.random.Next(60, 88);
                    break;
                case Seasons.SUMMER:
                    weathers = SeasonData.WeatherSummerList();
                    _serverTemp = Server.random.Next(73, 102);
                    break;
                case Seasons.AUTUMN:
                    weathers = SeasonData.WeatherAutumnList();
                    _serverTemp = Server.random.Next(53, 77);
                    break;
                case Seasons.WINTER:
                    _serverWeather = WeatherTypes.XMAS;
                    _serverTemp = Server.random.Next(29, 43);
                    break;
            }

            if (_serverSeason == Seasons.WINTER) // if its winter, its always xmas, blizzard and light snow are for north yankton
            {
                _serverWeather = WeatherTypes.XMAS;
            }
            else
            {
                WeatherTypes newType = weathers[Server.random.Next(weathers.Count - 1)];

                while (_serverWeather == newType)
                {
                    await Server.Delay(10);
                    newType = weathers[Server.random.Next(weathers.Count - 1)];
                }

                _serverWeather = newType;
            }

            weathers.Clear(); // clear the list

            SyncAllUsers(); // sync all players
        }

        private static async Task OnWorldTimeTick()
        {
            await Server.Delay(0);
            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)timeSpan.TotalSeconds;
            
            double newBaseTime = (secondsSinceEpoch / 2) + 360;
            
            if (_freezeTime)
                _timeOffset = (_timeOffset + _baseTime) - newBaseTime;

            _baseTime = newBaseTime;

            int hour = (int)Math.Floor(((_baseTime + _timeOffset) / 60) % 24);
            if (hour == 5 && !_hasChangedSeason)
            {
                _hasChangedSeason = true;

                _season++; // increase the season by 1

                if (_season >= 4) // cannot have more seasons than whats in the list, 4 seasons known and arrays start at zero
                    _season = 0; // reset to base season

                _serverSeason = _seasonsList[_season]; // set new season based on the increase

                Server.TriggerClientEvent("curiosity:client:seasons:sync:season", (int)_serverSeason, (int)_serverWeather, _serverTemp); // inform clients

                Log.Success($"[SEASONS] Changed Season {_serverSeason}:{_season}");
            }
            else if (hour != 5 && _hasChangedSeason)
            {
                _hasChangedSeason = false;
            }
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
