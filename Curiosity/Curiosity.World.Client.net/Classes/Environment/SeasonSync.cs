using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.net.Data;
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
        static Seasons _lastSeason;

        // population
        static float PED_MULTIPLIER = 1.0f;
        static float VEH_MULTIPLIER = 1.0f;
        static float VEH_PARKED_MULTIPLIER = 1.0f;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:server:seasons:sync:time", new Action<double, double, bool>(OnSeasonsTimeSync));
            client.RegisterEventHandler("curiosity:client:seasons:sync:season", new Action<int, int, int>(GetOnSeasonsTimeSync));
            client.RegisterEventHandler("curiosity:client:seasons:sync:weather", new Action<int, bool, int>(OnSeasonsWeatherSync));

            client.RegisterTickHandler(OnSeasonTimerTick);
            client.RegisterTickHandler(OnPopulationManagement);

            Log.Verbose($"[WORLD WEATHER] Init");
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

            switch((WeatherTypes)weather)
            {
                case WeatherTypes.XMAS_STORM:
                case WeatherTypes.XMAS:
                    SetForceVehicleTrails(true);
                    SetForcePedFootstepsTracks(true);
                    if ((WeatherTypes)weather == WeatherTypes.XMAS_STORM)
                    {
                        SetWeatherTypeTransition((uint)GetHashKey("XMAS"), (uint)GetHashKey("BLIZZARD"), 0.5f);
                    }
                    else
                    {
                        SetWeather(_lastWeather);
                    }
                    break;
                default:
                    SetWeather(_lastWeather);
                    SetForceVehicleTrails(false);
                    SetForcePedFootstepsTracks(false);
                    break;
            }

            switch((WeatherTypes)weather)
            {
                case WeatherTypes.XMAS_STORM:
                    PED_MULTIPLIER = 0f;
                    VEH_MULTIPLIER = .2f;
                    VEH_PARKED_MULTIPLIER = 1f;
                    break;
                case WeatherTypes.HALLOWEEN:
                case WeatherTypes.XMAS:
                    PED_MULTIPLIER = .2f;
                    VEH_MULTIPLIER = .5f;
                    VEH_PARKED_MULTIPLIER = 1f;
                    break;
                case WeatherTypes.CLEAR:
                case WeatherTypes.CLEARING:
                case WeatherTypes.EXTRASUNNY:
                case WeatherTypes.NEUTRAL:
                    PED_MULTIPLIER = 1f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = .5f;
                    break;
                case WeatherTypes.FOGGY:
                case WeatherTypes.SMOG:
                    PED_MULTIPLIER = .5f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = .1f;
                    break;
                case WeatherTypes.OVERCAST:
                    PED_MULTIPLIER = .8f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = .1f;
                    break;
                case WeatherTypes.RAIN:
                    PED_MULTIPLIER = .3f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = .1f;
                    break;
                case WeatherTypes.THUNDER:
                    PED_MULTIPLIER = .1f;
                    VEH_MULTIPLIER = 1f;
                    VEH_PARKED_MULTIPLIER = .1f;
                    break;
            }

            string weatherMessage = "";
            switch ((WeatherTypes)weather)
            {
                case WeatherTypes.XMAS_STORM:
                    weatherMessage = "Winter Storm";
                    break;
                case WeatherTypes.XMAS:
                    weatherMessage = "Winter";
                    break;
                case WeatherTypes.THUNDER:
                    weatherMessage = "Thunder Storm";
                    break;
                case WeatherTypes.SNOWLIGHT:
                    weatherMessage = "Light Snow";
                    break;
                case WeatherTypes.SNOW:
                    weatherMessage = "Snow";
                    break;
                case WeatherTypes.SMOG:
                    weatherMessage = "Smoggy";
                    break;
                case WeatherTypes.RAIN:
                    weatherMessage = "Raining cats and dogs";
                    break;
                case WeatherTypes.OVERCAST:
                    weatherMessage = "Overcast";
                    break;
                case WeatherTypes.NEUTRAL:
                    weatherMessage = "Nothing to say";
                    break;
                case WeatherTypes.HALLOWEEN:
                    weatherMessage = "SPOOKY!";
                    break;
                case WeatherTypes.FOGGY:
                    weatherMessage = "Foggy, can't see s***";
                    break;
                case WeatherTypes.EXTRASUNNY:
                    weatherMessage = "Hot and Sweaty";
                    break;
                case WeatherTypes.CLOUDS:
                    weatherMessage = "Clouds";
                    break;
                case WeatherTypes.CLEARING:
                    weatherMessage = "Its clearing up";
                    break;
                case WeatherTypes.CLEAR:
                    weatherMessage = "Clear day a'head";
                    break;
                case WeatherTypes.BLIZZARD:
                    weatherMessage = "Its a damn blizzard!";
                    break;
            }

            Client.TriggerEvent("curiosity:Client:Notification:Advanced", "CHAR_LS_TOURIST_BOARD", 1, "Weather Update", $"{weatherMessage}", $"", 2);
        }

        private static void SetWeather(WeatherTypes weather)
        {
            string w = $"{weather}";
            SetWeatherTypeNow(w);
            SetWeatherTypePersist(w);
            SetWeatherTypeNowPersist(w);
        }

        private static void GetOnSeasonsTimeSync(int season, int weather, int temp)
        {
            if (_lastSeason == (Seasons)season) return;
            _lastSeason = (Seasons)season;

            string message = "";

            switch ((Seasons)season)
            {
                case Seasons.SPRING:
                    message = "Ah, early spring morning. Listen to those birds.";
                    break;
                case Seasons.SUMMER:
                    message = "Its hot out side, look out for the lizard people!";
                    break;
                case Seasons.AUTUMN:
                    message = "Leaf falls down, thats why we call it Fall.";
                    break;
                case Seasons.WINTER:
                    message = "It's as cold a my sex life.";
                    break;
            }

            Client.TriggerEvent("curiosity:Client:Notification:Advanced", "CHAR_LS_TOURIST_BOARD", 1, "Seasonal Change", "", $"{message}", 2);
        }

        private static void OnSeasonsTimeSync(double serverTime, double serverOffset, bool freezeTime)
        {
            _clientBaseTime = serverTime;
            _clientTimeOffset = serverOffset;
            _clientFreezeTime = freezeTime;
        }
    }
}
