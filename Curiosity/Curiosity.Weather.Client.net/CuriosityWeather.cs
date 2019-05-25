using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.Net
{
    public class CuriosityWeather : BaseScript
    {
        public CuriosityWeather()
        {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["curiosity:Client:Weather:Sync"] += new Action<string, bool, float>(WeatherSync);
            EventHandlers["curiosity:Client:Time:Sync"] += new Action<int, int>(TimeSync);

            Tick += WeatherChecker;
        }

        async Task WeatherChecker()
        {
            while (true)
            {
                bool trails = World.Weather == Weather.Christmas;
                API.SetForceVehicleTrails(trails);
                API.SetForcePedFootstepsTracks(trails);
                await Delay(0);
            }
        }

        async void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;
            await Delay(0);
            TriggerServerEvent("curiosity:Server:Weather:Sync"); // Also syncs the time
        }

        async void TimeSync(int hour, int minute)
        {
            if (hour > 23)
            {
                hour = 0;
            }

            API.NetworkOverrideClockTime(hour, minute, 0);
            await Delay(0);
        }

        async void WeatherSync(string weather, bool wind, float windHeading)
        {
            await Delay(0);

            API.ClearWeatherTypePersist();
            API.SetWeatherTypeOverTime(weather, 60.00f);

            bool trails = (weather == "XMAS");
            API.SetForceVehicleTrails(trails);
            API.SetForcePedFootstepsTracks(trails);

            await Delay(0);

            if (wind)
            {
                API.SetWind(1.0f);
                API.SetWindSpeed(4.0f);
                API.SetWindDirection(windHeading);
            } else
            {
                API.SetWind(0f);
                API.SetWindSpeed(0f);
            }
            await Delay(0);
        }
    }
}
