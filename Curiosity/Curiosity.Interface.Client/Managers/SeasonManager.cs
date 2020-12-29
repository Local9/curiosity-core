using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Interface.Client.Managers
{
    public class SeasonManager : Manager<SeasonManager>
    {
        private const string ANIM_DICT_SNOWBALL = "anim@mp_snowball";

        // TIME
        static double _clientBaseTime = 0;
        static double _clientTimeOffset = 0;
        static double _clientTimer = 0;
        static bool _clientFreezeTime = false;
        static bool _singleTimeSent = false;

        static int _hour = 0;
        static int _minute = 0;

        // WEATHER
        static WeatherTypes _lastWeather;
        static WeatherTypes _lastWeatherBeforeXmas;
        static Seasons _lastSeason;
        static string _seasonString;
        static int _temp;

        static bool _snowCheckActive = false;
        static bool _connected = false;
        static bool _startup = false;

        public override void Begin()
        {
            Instance.EventRegistry["onClientResourceStart"] += new Action<string>(OnResourceStart);
            // Legacy Events
            Instance.EventRegistry["curiosity:client:seasons:sync:time"] += new Action<double, double, bool>(OnSyncTime);
            Instance.EventRegistry["curiosity:client:seasons:sync:season"] += new Action<int, int, int>(OnSeasonsTimeSync);
            Instance.EventRegistry["curiosity:client:seasons:sync:weather"] += new Action<int, bool, int, float, float, float>(OnSeasonsWeatherSync);
            Instance.EventRegistry["curiosity:client:seasons:sync:clock"] += new Action<int, int>(OnSeasonsSyncClock);

            // NUI
            Instance.AttachNuiHandler("GetWeather", new EventCallback(metadata =>
            {
                WeatherNuiMessage(_lastWeather);

                if (!_connected)
                {
                    _connected = !_connected;

                    JsonBuilder jsonBuilder = new JsonBuilder()
                        .Add("operation", "TIME")
                        .Add("hour", $"{_hour:00}")
                        .Add("minute", $"{_minute:00}");
                    API.SendNuiMessage(jsonBuilder.Build());
                }

                return null;
            }));
        }

        private void OnResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            BaseScript.TriggerServerEvent("curiosity:server:seasons:sync:connection");
        }

        [TickHandler(SessionWait = true)]
        private static async Task OnSeasonTimerTick()
        {
            await BaseScript.Delay(100);

            double newBaseTime = _clientBaseTime;
            if ((GetGameTimer() - 500) > _clientTimer)
            {
                newBaseTime += 0.25;
                _clientTimer = GetGameTimer();
            }

            if (_clientFreezeTime)
                _clientTimeOffset = (_clientTimeOffset + _clientBaseTime) - newBaseTime;

            _clientBaseTime = newBaseTime;

            _hour = (int)Math.Floor(((_clientBaseTime + _clientTimeOffset) / 60) % 24);
            _minute = (int)Math.Floor((_clientBaseTime + _clientTimeOffset) % 60);

            NetworkOverrideClockTime(_hour, _minute, 0);
            SetClockTime(_hour, _minute, 0);
        }

        private static void OnSyncTime(double serverTime, double serverOffset, bool freezeTime)
        {
            _clientBaseTime = serverTime;
            _clientTimeOffset = serverOffset;
            _clientFreezeTime = freezeTime;
        }

        private void OnSeasonsTimeSync(int season, int weather, int temp)
        {
            if (_lastSeason == (Seasons)season && _startup) return;

            _startup = true;

            _lastSeason = (Seasons)season;
            _temp = temp;

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
                    _seasonString = "Spring";
                    break;
                case Seasons.SUMMER:
                    SetForceVehicleTrails(false);
                    SetForcePedFootstepsTracks(false);
                    _seasonString = "Summer";
                    break;
                case Seasons.AUTUMN:
                    SetForceVehicleTrails(false);
                    SetForcePedFootstepsTracks(false);
                    _seasonString = "Autumn";
                    break;
                case Seasons.WINTER: // OVERRIDE ALL THE THINGS!
                    _lastWeatherBeforeXmas = _lastWeather;
                    _lastWeather = WeatherTypes.XMAS;
                    SetForceVehicleTrails(true);
                    SetForcePedFootstepsTracks(true);
                    _seasonString = "Winter";
                    SetWeatherDelay(_lastWeather);
                    break;
            }

            WeatherNuiMessage(_lastWeather);
        }

        private async void OnSeasonsWeatherSync(int weather, bool blackout, int temp, float windSpeed, float windDirection, float rainIntensity)
        {
            WeatherTypes newWeather = (WeatherTypes)weather;
            if (_lastWeather == newWeather) return;

            _lastWeather = newWeather;

            SetWeatherTypeOverTime($"{weather}", 15.0f);

            await BaseScript.Delay(15000);

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

                    if (!_snowCheckActive)
                        Instance.AttachTickHandler(OnSnowCheck);
                    break;
                default:
                    SetForceVehicleTrails(false);
                    SetForcePedFootstepsTracks(false);
                    SetWeather(_lastWeather);

                    if (_snowCheckActive)
                    {
                        Instance.DetachTickHandler(OnSnowCheck);
                        RemoveAnimDict(ANIM_DICT_SNOWBALL);
                    }
                    break;
            }

            SetWind(windSpeed);
            SetWindDirection(windDirection); // 0-7.9
        }

        private void OnSeasonsSyncClock(int hour, int minute)
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

            SendNuiMessage(jsonBuilder.Build());
        }

        private static async void SetWeatherDelay(WeatherTypes weather)
        {
            SetWeatherTypeOverTime($"{_lastWeather}", 15.0f);

            await BaseScript.Delay(15000);

            ClearOverrideWeather();
            ClearWeatherTypePersist();

            SetWeather(weather);
        }

        private static void SetWeather(WeatherTypes weather)
        {
            string weatherString = $"{weather}";
            SetOverrideWeather(weatherString);
            SetWeatherTypeNow(weatherString);
            SetWeatherTypePersist(weatherString);
            SetWeatherTypeNowPersist(weatherString);

            WeatherNuiMessage(weather);
        }

        private async Task OnSnowCheck()
        {
            bool trails = World.Weather == Weather.Christmas;
            SetForceVehicleTrails(trails);
            SetForcePedFootstepsTracks(trails);

            RequestAnimDict(ANIM_DICT_SNOWBALL);

            int failCount = 0;

            while(!HasAnimDictLoaded(ANIM_DICT_SNOWBALL))
            {
                await BaseScript.Delay(100);
                failCount++;

                if (failCount == 5)
                    break;
            }

            _snowCheckActive = true;

            if (trails
                && (Game.PlayerPed.Weapons.Current.Hash == WeaponHash.Unarmed || Game.PlayerPed.Weapons.Current.Hash == WeaponHash.Snowball)
                && Game.IsControlPressed(0, Control.ThrowGrenade)
                && !Game.PlayerPed.IsInVehicle())
            {
                

                if (!Game.PlayerPed.Weapons.HasWeapon(WeaponHash.Snowball))
                {
                    Game.PlayerPed.Task.PlayAnimation(ANIM_DICT_SNOWBALL, "pickup_snowball");
                    Game.PlayerPed.Weapons.Give(WeaponHash.Snowball, 1, true, true);
                }
                else if (Game.PlayerPed.Weapons[WeaponHash.Snowball].Ammo < 10)
                {
                    Game.PlayerPed.Task.PlayAnimation(ANIM_DICT_SNOWBALL, "pickup_snowball");
                    Game.PlayerPed.Weapons[WeaponHash.Snowball].Ammo++;
                    Game.PlayerPed.Weapons.Give(WeaponHash.Snowball, 1, true, true);
                }
                await BaseScript.Delay(1500);
            }
        }
    }
}
