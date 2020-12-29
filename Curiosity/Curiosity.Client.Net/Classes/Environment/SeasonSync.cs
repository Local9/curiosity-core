using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Client.net.Classes.PlayerClasses;
using Curiosity.Client.net.Helpers;
using Curiosity.Global.Shared.Data;
using Curiosity.Shared.Client.net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.net.Classes.Environment
{
    class SeasonSync
    {
        static Client client = Client.GetInstance();

        // TIME
        static double _clientBaseTime = 0;
        static double _clientTimeOffset = 0;
        static double _clientTimer = 0;
        static bool _clientFreezeTime = false;
        static bool _singleTimeSent = false;

        static int hour = 0;
        static int minute = 0;

        // WEATHER
        static WeatherTypes _lastWeather;
        static WeatherTypes _lastWeatherBeforeXmas;
        static Seasons _lastSeason;
        static string _seasonString;
        static int _temp;

        // population
        static float PED_MULTIPLIER;
        static float VEH_MULTIPLIER;
        static float VEH_PARKED_MULTIPLIER;

        static bool weatherDebug = false;
        static bool _connected = false;
        static bool _startup = false;

        public static void Init()
        {
            RegisterCommand("wd", new Action<int, List<object>, string>(OnWeatherDebug), false);

            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnResourceStart));

            client.RegisterEventHandler("curiosity:client:seasons:sync:time", new Action<double, double, bool>(OnSyncTime));
            client.RegisterEventHandler("curiosity:client:seasons:sync:season", new Action<int, int, int>(GetOnSeasonsTimeSync));
            client.RegisterEventHandler("curiosity:client:seasons:sync:weather", new Action<int, bool, int, float, float, float>(OnSeasonsWeatherSync));
            client.RegisterEventHandler("curiosity:client:seasons:sync:clock", new Action<int, int>(OnSeasonsSyncClock));

            // client.RegisterNuiEventHandler("GetWeather", new Action<IDictionary<string, object>, CallbackDelegate>(OnGetWeather));

            client.RegisterTickHandler(OnSeasonTimerTick);
            // client.RegisterTickHandler(OnPopulationManagement);
            // client.RegisterTickHandler(OnSnowCheck);

            Log.Verbose($"[WORLD WEATHER] Init");
        }

        private static void OnSeasonsSyncClock(int hour, int minute)
        {
            if (minute % 10 == 0 && _lastWeather != WeatherTypes.HALLOWEEN)
            {
                JsonBuilder jsonBuilder = new JsonBuilder()
                    .Add("operation", "TIME")
                    .Add("hour", $"{hour:00}")
                    .Add("minute", $"{minute:00}");
                API.SendNuiMessage(jsonBuilder.Build());
            }
            else if (_lastWeather == WeatherTypes.HALLOWEEN && !_singleTimeSent)
            {
                _singleTimeSent = true;

                JsonBuilder jsonBuilder = new JsonBuilder()
                    .Add("operation", "TIME")
                    .Add("hour", $"{0:00}")
                    .Add("minute", $"{0:00}");
                API.SendNuiMessage(jsonBuilder.Build());
            }
        }

        private static void OnGetWeather(IDictionary<string, object> arg1, CallbackDelegate cb)
        {
            WeatherNuiMessage(_lastWeather);

            if (!_connected)
            {
                _connected = !_connected;

                JsonBuilder jsonBuilder = new JsonBuilder()
                    .Add("operation", "TIME")
                    .Add("hour", $"{hour:00}")
                    .Add("minute", $"{minute:00}");
                API.SendNuiMessage(jsonBuilder.Build());
            }

            cb(new { ok = true });
        }

        private static void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            Client.TriggerServerEvent("curiosity:server:seasons:sync:connection");
        }

        private static void OnWeatherDebug(int arg1, List<object> arg2, string arg3)
        {
            if (!PlayerInformation.IsTrustedAdmin()) return;

            weatherDebug = !weatherDebug;
            if (weatherDebug)
            {
                client.RegisterTickHandler(OnWeatherDebugTick);
                return;
            }
            client.DeregisterTickHandler(OnWeatherDebugTick);
        }

        private static async Task OnWeatherDebugTick()
        {
            Screen.ShowSubtitle($"WT: {_lastWeather}, S: {_lastSeason}, T: {hour:00}:{minute:00}~n~P: V {VEH_MULTIPLIER:f}, PV {VEH_PARKED_MULTIPLIER:f}, P: {PED_MULTIPLIER:f}");
        }

        private static async Task OnPopulationManagement()
        {
            API.SetVehicleDensityMultiplierThisFrame(VEH_MULTIPLIER);
            API.SetPedDensityMultiplierThisFrame(PED_MULTIPLIER);
            API.SetRandomVehicleDensityMultiplierThisFrame(VEH_MULTIPLIER);
            API.SetParkedVehicleDensityMultiplierThisFrame(VEH_PARKED_MULTIPLIER);
            API.SetScenarioPedDensityMultiplierThisFrame(PED_MULTIPLIER, PED_MULTIPLIER);
        }

        private static async Task OnSeasonTimerTick()
        {
            await Client.Delay(0);

            double newBaseTime = _clientBaseTime;
            if ((GetGameTimer() - 500) > _clientTimer)
            {
                newBaseTime += 0.25;
                _clientTimer = GetGameTimer();
            }

            if (_clientFreezeTime)
                _clientTimeOffset = (_clientTimeOffset + _clientBaseTime) - newBaseTime;

            _clientBaseTime = newBaseTime;

            hour = (int)Math.Floor(((_clientBaseTime + _clientTimeOffset) / 60) % 24);
            minute = (int)Math.Floor((_clientBaseTime + _clientTimeOffset) % 60);

            NetworkOverrideClockTime(hour, minute, 0);
            SetClockTime(hour, minute, 0);

            if (minute % 10 == 0 && _lastWeather != WeatherTypes.HALLOWEEN)
            {
                JsonBuilder jsonBuilder = new JsonBuilder()
                    .Add("operation", "TIME")
                    .Add("hour", $"{hour:00}")
                    .Add("minute", $"{minute:00}");
                // API.SendNuiMessage(jsonBuilder.Build());
            }
            else if (_lastWeather == WeatherTypes.HALLOWEEN && !_singleTimeSent)
            {
                _singleTimeSent = true;

                JsonBuilder jsonBuilder = new JsonBuilder()
                    .Add("operation", "TIME")
                    .Add("hour", $"{0:00}")
                    .Add("minute", $"{0:00}");
                // API.SendNuiMessage(jsonBuilder.Build());
            }
        }

        private static void ShowTimeAndWeather()
        {

        }

        private static async void OnSeasonsWeatherSync(int weather, bool blackout, int temp, float windSpeed, float windDirection, float rainIntensity)
        {
            WeatherTypes newWeather = (WeatherTypes)weather;
            if (_lastWeather == newWeather) return;

            _lastWeather = newWeather;

            SetWeatherTypeOverTime($"{weather}", 15.0f);

            await Client.Delay(15000);

            ClearOverrideWeather();
            ClearWeatherTypePersist();
            SetBlackout(blackout);

            if (_lastSeason == Seasons.WINTER) // override to be sure 
            {
                _lastWeather = WeatherTypes.XMAS;
                _lastWeatherBeforeXmas = (WeatherTypes)weather;
            }

            switch ((WeatherTypes)weather)
            {
                case WeatherTypes.XMAS_STORM:
                case WeatherTypes.XMAS:
                    SetForceVehicleTrails(true);
                    SetForcePedFootstepsTracks(true);
                    SetWeather(WeatherTypes.XMAS);
                    break;
                default:
                    SetForceVehicleTrails(false);
                    SetForcePedFootstepsTracks(false);
                    SetWeather(_lastWeather);
                    break;
            }

            API.SetWind(windSpeed);
            API.SetWindDirection(windDirection); // 0-7.9
        }

        private static async void SetWeatherDelay(WeatherTypes weather)
        {
            SetWeatherTypeOverTime($"{_lastWeather}", 15.0f);

            await Client.Delay(15000);

            ClearOverrideWeather();
            ClearWeatherTypePersist();

            SetWeather(weather);
        }

        private static void SetWeather(WeatherTypes weather)
        {
            string w = $"{weather}";
            SetOverrideWeather(w);
            SetWeatherTypeNow(w);
            SetWeatherTypePersist(w);
            SetWeatherTypeNowPersist(w);

            // SetPopulationValues(weather);

            WeatherNuiMessage(weather);
        }

        private static async void GetOnSeasonsTimeSync(int season, int weather, int temp)
        {
            if (_lastSeason == (Seasons)season && _startup) return;

            _startup = true;

            _lastSeason = (Seasons)season;
            _temp = temp;
            // SetPopulationValues((WeatherTypes)weather);

            string message = "";

            switch ((Seasons)season)
            {
                case Seasons.SPRING:
                    if (_lastWeather.Equals(WeatherTypes.XMAS) || _lastWeather.Equals(WeatherTypes.XMAS_STORM))
                    {
                        _lastWeather = _lastWeatherBeforeXmas;
                        SetWeatherDelay(_lastWeather);
                    }
                    SetForceVehicleTrails(false);
                    SetForcePedFootstepsTracks(false);
                    message = "Ah, early spring morning. Listen to those birds.";
                    _seasonString = "Spring";
                    break;
                case Seasons.SUMMER:
                    SetForceVehicleTrails(false);
                    SetForcePedFootstepsTracks(false);
                    message = "Its hot out side, look out for the lizard people!";
                    _seasonString = "Summer";
                    break;
                case Seasons.AUTUMN:
                    SetForceVehicleTrails(false);
                    SetForcePedFootstepsTracks(false);
                    message = "Leaf falls down, thats why we call it Fall.";
                    _seasonString = "Autumn";
                    break;
                case Seasons.WINTER: // OVERRIDE ALL THE THINGS!
                    message = "It's as cold a my sex life.";
                    _lastWeatherBeforeXmas = _lastWeather;
                    _lastWeather = WeatherTypes.XMAS;
                    SetForceVehicleTrails(true);
                    SetForcePedFootstepsTracks(true);
                    _seasonString = "Winter";
                    SetWeatherDelay(_lastWeather);
                    break;
            }

            WeatherNuiMessage(_lastWeather);

            // Client.TriggerEvent("curiosity:Client:Notification:Advanced", "CHAR_LS_TOURIST_BOARD", 1, "Seasonal Change", "", $"{message}", 2);
        }

        private static void OnSyncTime(double serverTime, double serverOffset, bool freezeTime)
        {
            _clientBaseTime = serverTime;
            _clientTimeOffset = serverOffset;
            _clientFreezeTime = freezeTime;
        }

        private static void SetPopulationValues(WeatherTypes weather)
        {
            PED_MULTIPLIER = 1f;
            VEH_MULTIPLIER = 1f;

            switch (weather)
            {
                case WeatherTypes.XMAS_STORM:
                case WeatherTypes.XMAS:
                    PED_MULTIPLIER = 0.2f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = 1f;
                    break;
                case WeatherTypes.HALLOWEEN:
                    PED_MULTIPLIER = 0.2f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = 1f;
                    break;
                case WeatherTypes.CLEAR:
                case WeatherTypes.CLEARING:
                case WeatherTypes.EXTRASUNNY:
                case WeatherTypes.NEUTRAL:
                    VEH_PARKED_MULTIPLIER = 0.5f;
                    break;
                case WeatherTypes.FOGGY:
                case WeatherTypes.SMOG:
                    VEH_PARKED_MULTIPLIER = 0.1f;
                    break;
                case WeatherTypes.OVERCAST:
                case WeatherTypes.CLOUDS:
                    VEH_PARKED_MULTIPLIER = 0.1f;
                    break;
                case WeatherTypes.RAIN:
                    PED_MULTIPLIER = 0.5f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = 0.1f;
                    break;
                case WeatherTypes.THUNDER:
                    PED_MULTIPLIER = 0.1f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = 0.1f;
                    break;
            }


            WeatherNuiMessage(weather);
        }

        private static void WeatherNuiMessage(WeatherTypes weather)
        {
            JsonBuilder jsonBuilder = new JsonBuilder();
            jsonBuilder.Add("operation", "WEATHER");
            jsonBuilder.Add("temp", _temp);
            jsonBuilder.Add("season", _seasonString);

            switch (weather)
            {
                case WeatherTypes.XMAS_STORM:
                case WeatherTypes.XMAS:
                    jsonBuilder.Add("type", "Snow");
                    break;
                case WeatherTypes.HALLOWEEN:
                    jsonBuilder.Add("type", "Halloween");
                    break;
                case WeatherTypes.CLEAR:
                case WeatherTypes.CLEARING:
                case WeatherTypes.NEUTRAL:
                    jsonBuilder.Add("type", "Clear");
                    break;
                case WeatherTypes.EXTRASUNNY:
                    jsonBuilder.Add("type", "Sunny");
                    break;
                case WeatherTypes.FOGGY:
                case WeatherTypes.SMOG:
                    jsonBuilder.Add("type", "Foggy");
                    break;
                case WeatherTypes.OVERCAST:
                case WeatherTypes.CLOUDS:
                    jsonBuilder.Add("type", "Cloudy");
                    break;
                case WeatherTypes.RAIN:
                    jsonBuilder.Add("type", "Raining");
                    break;
                case WeatherTypes.THUNDER:
                    jsonBuilder.Add("type", "Thunder Storm");
                    break;
            }
            // API.SendNuiMessage(jsonBuilder.Build());
        }
    }
}
