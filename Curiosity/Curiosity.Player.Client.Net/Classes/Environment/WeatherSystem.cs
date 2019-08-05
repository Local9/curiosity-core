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
            client.RegisterEventHandler("curiosity:Client:Weather:Sync", new Action<string, bool, float, float>(WeatherSync));

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

        static async void WeatherSync(string weather, bool wind, float windSpeed, float windHeading)
        {
            if (Player.PlayerInformation.IsDeveloper())
            {
                Log.Verbose($"weather: {weather}, wind: {wind}, windSpeed: {windSpeed}, windHeading: {windHeading}");
            }

            await Client.Delay(0);

            API.ClearWeatherTypePersist();
            API.SetWeatherTypeOverTime(weather, 60.00f);

            bool trails = (weather == "XMAS");

            API.SetForceVehicleTrails(trails);
            API.SetForcePedFootstepsTracks(trails);

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
