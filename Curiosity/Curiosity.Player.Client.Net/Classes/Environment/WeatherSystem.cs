using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment
{
    class WeatherSystem
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
            client.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));
            client.RegisterEventHandler("curiosity:Client:Weather:Sync", new Action<string, bool, float, float, bool, bool>(WeatherSync));

            client.RegisterTickHandler(TimeNetworkSync);
            client.RegisterTickHandler(WeatherChecker);
        }


        static async Task WeatherChecker()
        {
            while (true)
            {
                bool trails = World.Weather == Weather.Christmas;
                API.SetForceVehicleTrails(trails);
                API.SetForcePedFootstepsTracks(trails);
                await Client.Delay(0);
            }
        }

        static void OnPlayerSpawned()
        {
            Client.TriggerServerEvent("curiosity:Server:Weather:Sync");
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;
            Client.TriggerServerEvent("curiosity:Server:Weather:Sync");
        }

        static async Task TimeNetworkSync()
        {
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            API.NetworkGetServerTime(ref hours, ref minutes, ref seconds);
            API.NetworkOverrideClockTime(hours, minutes, 0);
            await Task.FromResult(0);
        }

        static async void WeatherSync(string weather, bool wind, float windSpeed, float windHeading, bool isChristmas, bool isHalloween)
        {
            if (Player.PlayerInformation.IsDeveloper())
            {
                Log.Verbose($"weather: {weather}, wind: {wind}, windSpeed: {windSpeed}, windHeading: {windHeading}, isChristmas: {isChristmas}");
            }

            await Client.Delay(0);

            API.ClearWeatherTypePersist();

            if (isChristmas)
            {
                API.SetForceVehicleTrails(isChristmas);
                API.SetForcePedFootstepsTracks(isChristmas);
                API.SetWeatherTypeOverTime(weather, 60.00f);

                uint weatherStruct = 0x27EA2814; // blizzard

                switch (weather)
                {
                    case "CLEAR":
                        weatherStruct = 0x36A83D84;
                        break;
                    case "EXTRASUNNY":
                        weatherStruct = 0x97AA0A79;
                        break;
                    case "CLOUDS":
                        weatherStruct = 0x36A83D84;
                        break;
                    case "OVERCAST":
                        weatherStruct = 0xBB898D2D;
                        break;
                    case "RAIN":
                        weatherStruct = 0x54A69840;
                        break;
                    case "CLEARING":
                        weatherStruct = 0x6DB1A50D;
                        break;
                    case "THUNDER":
                        weatherStruct = 0xB677829F;
                        break;
                    case "SMOG":
                        weatherStruct = 0x10DCF4B5;
                        break;
                    case "FOGGY":
                        weatherStruct = 0xAE737644;
                        break;
                    case "XMAS":
                        weatherStruct = 0xAAC9C895;
                        break;
                    case "SNOWLIGHT":
                        weatherStruct = 0x23FB812B;
                        break;
                }

                API.SetWeatherTypeOverTime("XMAS", 60.00f);

                API.SetWeatherTypeTransition(0xAAC9C895, weatherStruct, 0.5f);
            }
            else if (isHalloween)
            {
                API.SetWeatherTypeOverTime("HALLOWEEN", 60.00f);
            }
            else
            {
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
        }
    }
}
