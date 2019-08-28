using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using System;
using System.Threading.Tasks;

namespace Curiosity.World.Client.net.Classes.Environment
{
    class WeatherSystem
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
            client.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));
            client.RegisterEventHandler("curiosity:Client:Weather:Sync", new Action<string, bool, float, float, bool, bool>(WeatherSync));
            
            client.RegisterTickHandler(WeatherChecker);
        }
        
        static async Task WeatherChecker()
        {
            while (true)
            {
                bool trails = CitizenFX.Core.World.Weather == Weather.Christmas;
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
                API.SetWeatherTypeOverTime("XMAS", 60.00f);
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
