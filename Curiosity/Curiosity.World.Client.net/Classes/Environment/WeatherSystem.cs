using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using System;
using System.Threading.Tasks;

namespace Curiosity.GameWorld.Client.net.Classes.Environment
{
    class WeatherSystem
    {
        static Client client = Client.GetInstance();

        static private bool IsChristmas = false;
        static public bool IsHalloween = false;

        public static void Init()
        {
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
            client.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));
            client.RegisterEventHandler("curiosity:Client:Weather:Sync", new Action<string, bool, float, float, bool, bool>(WeatherSync));

            client.RegisterEventHandler("curiosity:Client:Weather:Check", new Action(WeatherCheckSync));

            Client.TriggerServerEvent("curiosity:Server:Weather:Sync");

            client.RegisterTickHandler(WeatherChecker);
        }

        static void WeatherCheckSync()
        {
            BaseScript.TriggerEvent("curiosity:Client:Weather:CheckReturn", IsChristmas, IsHalloween);
        }


        static async Task WeatherChecker()
        {
            while (true)
            {
                bool trails = CitizenFX.Core.World.Weather == Weather.Christmas;
                API.SetForceVehicleTrails(trails);
                API.SetForcePedFootstepsTracks(trails);
                await Client.Delay(0);

                if (trails
                    && (Game.PlayerPed.Weapons.Current.Hash == WeaponHash.Unarmed || Game.PlayerPed.Weapons.Current.Hash == WeaponHash.Snowball)
                    && Game.IsControlPressed(0, Control.ThrowGrenade)
                    && !Game.PlayerPed.IsInVehicle())
                {
                    API.RequestAnimDict("anim@mp_snowball");

                    if (!Game.PlayerPed.Weapons.HasWeapon(WeaponHash.Snowball))
                    {
                        Game.PlayerPed.Task.PlayAnimation("anim@mp_snowball", "pickup_snowball");
                        Game.PlayerPed.Weapons.Give(WeaponHash.Snowball, 1, true, true);
                    }
                    else if (Game.PlayerPed.Weapons[WeaponHash.Snowball].Ammo < 10)
                    {
                        Game.PlayerPed.Task.PlayAnimation("anim@mp_snowball", "pickup_snowball");
                        Game.PlayerPed.Weapons[WeaponHash.Snowball].Ammo++;
                        Game.PlayerPed.Weapons.Give(WeaponHash.Snowball, 1, true, true);
                    }
                    await Client.Delay(1000);
                }
            }
        }

        static void OnPlayerSpawned()
        {
            Client.TriggerServerEvent("curiosity:Server:Weather:Sync");
        }

        static async void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            await Client.Delay(2000);

            Client.TriggerServerEvent("curiosity:Server:Weather:Sync");
        }

        static async void WeatherSync(string weather, bool wind, float windSpeed, float windHeading, bool isChristmas, bool isHalloween)
        {
            await Client.Delay(0);

            API.ClearWeatherTypePersist();

            API.SetForceVehicleTrails(isChristmas);
            API.SetForcePedFootstepsTracks(isChristmas);

            IsHalloween = isHalloween;
            IsChristmas = isChristmas;

            if (isChristmas)
            {
                if (Player.PlayerInformation.IsDeveloper())
                {
                    Log.Verbose("WeatherSync -> Setting weather to XMAS");
                }

                API.ClearOverrideWeather();
                API.SetWeatherTypePersist(weather);
                API.SetWeatherTypeNowPersist(weather);
                API.SetWeatherTypeNow(weather);
                API.SetOverrideWeather(weather);
            }

            if (IsHalloween)
            {
                if (Player.PlayerInformation.IsDeveloper())
                {
                    Log.Verbose($"WeatherSync -> Setting weather to HALLOWEEN");
                }

                string weatherType = "HALLOWEEN";

                API.ClearOverrideWeather();
                API.SetWeatherTypePersist(weatherType);
                API.SetWeatherTypeNowPersist(weatherType);
                API.SetWeatherTypeNow(weatherType);
                API.SetOverrideWeather(weatherType);
            }

            if (!isChristmas && !isHalloween)
            {
                if (Player.PlayerInformation.IsDeveloper())
                {
                    Log.Verbose($"WeatherSync -> Setting weather to {weather}");
                }

                API.ClearOverrideWeather();
                API.SetWeatherTypeOverTime(weather, 60.00f);
            }
            

            await Client.Delay(0);

            if (wind)
            {
                API.SetWind(1.0f);
                API.SetWindSpeed(windSpeed);
                API.SetWindDirection(windHeading);
            }
            else
            {
                API.SetWind(0f);
                API.SetWindSpeed(0f);
            }
            await Client.Delay(0);
            WeatherCheckSync();

            if (Player.PlayerInformation.IsDeveloper())
            {
                Log.Verbose($"weather: {weather}, wind: {wind}, windSpeed: {windSpeed}, windHeading: {windHeading}, isChristmas: {isChristmas}, isHalloween: {isHalloween}");
            }
        }
    }
}
