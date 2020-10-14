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

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:server:seasons:sync:time", new Action<double, double, bool>(OnSeasonsTimeSync));
            client.RegisterEventHandler("curiosity:client:seasons:sync:season", new Action<int, int, int>(GetOnSeasonsTimeSync));
            client.RegisterEventHandler("curiosity:client:seasons:sync:weather", new Action<int, bool, int>(OnSeasonsWeatherSync));

            client.RegisterTickHandler(OnSeasonTimerTick);

            Log.Verbose($"[WORLD WEATHER] Init");
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

            string ws = $"{(WeatherTypes)weather}".ToLowerInvariant();
            Screen.ShowNotification($"~b~Weather: ~s~{ws}");
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
            // Need to find a point for this...

            string ws = $"{(Seasons)season}".ToLowerInvariant();
            Screen.ShowNotification($"~b~Season: ~s~{ws}");
        }

        private static void OnSeasonsTimeSync(double serverTime, double serverOffset, bool freezeTime)
        {
            _clientBaseTime = serverTime;
            _clientTimeOffset = serverOffset;
            _clientFreezeTime = freezeTime;
        }
    }
}
