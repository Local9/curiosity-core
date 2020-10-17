using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.GameWorld.Client.net.Classes.Player;
using Curiosity.Global.Shared.Data;
using Curiosity.Shared.Client.net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.GameWorld.Client.net.Classes.Environment
{
    class SeasonSync
    {
        static Client client = Client.GetInstance();

        // TIME
        static double _clientBaseTime = 0;
        static double _clientTimeOffset = 0;
        static double _clientTimer = 0;
        static bool _clientFreezeTime = false;

        static int hour = 0;
        static int minute = 0;

        // WEATHER
        static WeatherTypes _lastWeather;
        static WeatherTypes _lastWeatherBeforeXmas;
        static Seasons _lastSeason;

        // population
        static float PED_MULTIPLIER;
        static float VEH_MULTIPLIER;
        static float VEH_PARKED_MULTIPLIER;

        static bool weatherDebug = false;

        public static void Init()
        {
            RegisterCommand("wd", new Action<int, List<object>, string>(OnWeatherDebug), false);

            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnResourceStart));

            client.RegisterEventHandler("curiosity:client:seasons:sync:time", new Action<double, double, bool>(OnSyncTime));
            client.RegisterEventHandler("curiosity:client:seasons:sync:season", new Action<int, int, int>(GetOnSeasonsTimeSync));
            client.RegisterEventHandler("curiosity:client:seasons:sync:weather", new Action<int, bool, int>(OnSeasonsWeatherSync));

            client.RegisterTickHandler(OnSeasonTimerTick);
            client.RegisterTickHandler(OnPopulationManagement);

            Log.Verbose($"[WORLD WEATHER] Init");
        }

        private static void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            Client.TriggerServerEvent("curiosity:server:seasons:sync:connection");
            Client.TriggerEvent("curiosity:Client:Player:Information");
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
        }

        private static async void OnSeasonsWeatherSync(int weather, bool blackout, int temp)
        {
            if (_lastWeather == (WeatherTypes)weather) return;

            _lastWeather = (WeatherTypes)weather;

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

            switch((WeatherTypes)weather)
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
        }

        private static async void SetWeatherDelay(WeatherTypes weather)
        {
            SetWeatherTypeOverTime($"{_lastWeather}", 15.0f);

            await Client.Delay(15000);

            ClearOverrideWeather();
            ClearWeatherTypePersist();

            string w = $"{weather}";
            SetWeatherTypeNow(w);
            SetWeatherTypePersist(w);
            SetWeatherTypeNowPersist(w);

            SetPopulationValues(weather);
        }

        private static void SetWeather(WeatherTypes weather)
        {
            string w = $"{weather}";
            SetWeatherTypeNow(w);
            SetWeatherTypePersist(w);
            SetWeatherTypeNowPersist(w);

            SetPopulationValues(weather);
        }

        private static async void GetOnSeasonsTimeSync(int season, int weather, int temp)
        {
            if (_lastSeason == (Seasons)season) return;
            _lastSeason = (Seasons)season;

            SetPopulationValues((WeatherTypes)weather);

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
                    break;
                case Seasons.SUMMER:
                    SetForceVehicleTrails(false);
                    SetForcePedFootstepsTracks(false);
                    message = "Its hot out side, look out for the lizard people!";
                    break;
                case Seasons.AUTUMN:
                    SetForceVehicleTrails(false);
                    SetForcePedFootstepsTracks(false);
                    message = "Leaf falls down, thats why we call it Fall.";
                    break;
                case Seasons.WINTER: // OVERRIDE ALL THE THINGS!
                    message = "It's as cold a my sex life.";
                    _lastWeatherBeforeXmas = _lastWeather;
                    _lastWeather = WeatherTypes.XMAS;
                    SetForceVehicleTrails(true);
                    SetForcePedFootstepsTracks(true);
                    SetWeatherDelay(_lastWeather);
                    break;
            }

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
            switch (weather)
            {
                case WeatherTypes.XMAS_STORM:
                case WeatherTypes.HALLOWEEN:
                case WeatherTypes.XMAS:
                    PED_MULTIPLIER = 0.2f;
                    VEH_MULTIPLIER = 0.5f;
                    VEH_PARKED_MULTIPLIER = 1f;
                    break;
                case WeatherTypes.CLEAR:
                case WeatherTypes.CLEARING:
                case WeatherTypes.EXTRASUNNY:
                case WeatherTypes.NEUTRAL:
                    PED_MULTIPLIER = 1f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = 0.5f;
                    break;
                case WeatherTypes.FOGGY:
                case WeatherTypes.SMOG:
                    PED_MULTIPLIER = 0.5f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = 0.1f;
                    break;
                case WeatherTypes.OVERCAST:
                case WeatherTypes.CLOUDS:
                    PED_MULTIPLIER = 0.8f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = 0.1f;
                    break;
                case WeatherTypes.RAIN:
                    PED_MULTIPLIER = 0.3f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = 0.1f;
                    break;
                case WeatherTypes.THUNDER:
                    PED_MULTIPLIER = 0.1f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = 0.1f;
                    break;
            }
        }
    }
}
