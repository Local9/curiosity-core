using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;

namespace Curiosity.Client.Net
{
    public class CuriosityWeather : BaseScript
    {
        public CuriosityWeather()
        {
            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
            EventHandlers["curiosity:Client:Weather:Sync"] += new Action<string, bool, float>(WeatherSync);
        }

        async void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;
            await Delay(0);
            TriggerServerEvent("curiosity:Server:Weather:Sync");
        }

        async void WeatherSync(string weather, bool wind, float windHeading)
        {
            await Delay(0);

            API.ClearWeatherTypePersist();
            API.SetWeatherTypeOverTime(weather, 60.00f);

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
