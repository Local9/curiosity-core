using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Server.Net
{
    public class CuriosityWeather : BaseScript
    {
        const int MINUTES_TO_WAIT = (1000*60)*20;

        Dictionary<string, List<string>> weathers = new Dictionary<string, List<string>>();
        Dictionary<string, bool> windWeathers = new Dictionary<string, bool>();

        bool isChristmas = true;
        bool isHalloween = true;
        bool isLive = false;

        DateTime now;
        TimeSpan timeNow;
        int hour = 0;
        int minute = 0;

        Entity.WeatherData weatherData = new Entity.WeatherData();

        public CuriosityWeather()
        {
            isChristmas = API.GetConvar("christmas_weather", "false") == "true";
            isHalloween = API.GetConvar("halloween_weather", "false") == "true";
            isLive = API.GetConvar("server_live", "false") == "true";

            SetupWindWeather();
            SetupWeathers();

            EventHandlers["curiosity:Server:Weather:Sync"] += new Action<Player>(SyncWeather);
            EventHandlers["curiosity:Server:Time:Sync"] += new Action<Player>(SyncTime);

            Tick += ChangeWeather;
            Tick += ServerTime;
        }

        void SetupWindWeather()
        {
            windWeathers.Add("OVERCAST", true);
            windWeathers.Add("RAIN", true);
            windWeathers.Add("THUNDER", true);
            windWeathers.Add("CLOUDS", true);
            windWeathers.Add("EXTRASUNNY", false);
            windWeathers.Add("SMOG", false);
            windWeathers.Add("CLEAR", false);
            windWeathers.Add("FOGGY", false);
            windWeathers.Add("CLEARING", false);
            windWeathers.Add("BLIZZARD", isChristmas);
            windWeathers.Add("XMAS", isChristmas);
            windWeathers.Add("SNOW", isChristmas);
        }

        void SetupWeathers()
        {
            weathers.Add("EXTRASUNNY", new List<string> { "CLEAR", "SMOG" });
            weathers.Add("SMOG", new List<string> { "FOGGY", "CLEAR", "CLEARING", "OVERCAST", "CLOUDS", "EXTRASUNNY" });
            weathers.Add("CLEAR", new List<string> { "CLOUDS", "EXTRASUNNY", "CLEARING", "SMOG", "FOGGY", "OVERCAST" });
            weathers.Add("CLOUDS", new List<string> { "CLEAR", "SMOG", "FOGGY", "CLEARING", "OVERCAST" });
            weathers.Add("FOGGY", new List<string> { "CLEAR", "CLOUDS", "SMOG", "OVERCAST" });
            weathers.Add("OVERCAST", new List<string> { "CLEAR", "CLOUDS", "SMOG", "FOGGY", "RAIN", "CLEARING" });
            weathers.Add("RAIN", new List<string> { "THUNDER", "CLEARING", "OVERCAST" });
            weathers.Add("THUNDER", new List<string> { "RAIN", "CLEARING" });
            weathers.Add("CLEARING", new List<string> { "CLEAR", "CLOUDS", "OVERCAST", "FOGGY", "SMOG" });

            if (isChristmas)
            {
                weathers.Add("SNOW", new List<string> { "BLIZZARD", "RAIN", "SNOWLIGHT" });
                weathers.Add("BLIZZARD", new List<string> { "SNOW", "SNOWLIGHT", "THUNDER" });
                weathers.Add("SNOWLIGHT", new List<string> { "SNOW", "RAIN", "CLEARING" });
            }

            if (isHalloween)
            {
                weathers.Add("HALLOWEEN", new List<string> { "CLOUDS", "RAIN", "CLEARING", "CLEAR" });
            }
        }

        async void SyncTime([FromSource]Player player)
        {
            player.TriggerEvent("curiosity:Client:Time:Sync", hour, minute);
            await Delay(0);
        }

        async Task ServerTime()
        {
            hour = 0;
            minute = 0;
            while (true)
            {
                await Delay(5000);

                now = DateTime.Now;
                timeNow = now.TimeOfDay;

                double hourDouble = timeNow.TotalMinutes % 24;
                double minuteDouble = (hourDouble % 1) * 60;

                hour = (int)hourDouble;
                minute = (int)minuteDouble;

                TriggerClientEvent("curiosity:Client:Time:Sync", hour, minute);
            }
        }

        async void SyncWeather([FromSource]Player player)
        {
            if (string.IsNullOrEmpty(weatherData.CurrentWeather))
            {
                await Delay(1000);
                await SetupWeather();
            } else
            {
                player.TriggerEvent("curiosity:Client:Weather:Sync", weatherData.CurrentWeather, weatherData.Wind, weatherData.WindHeading);

                if (!isLive)
                    Debug.WriteLine($"WEATHER SYNC -> {weatherData}");
            }
            player.TriggerEvent("curiosity:Client:Time:Sync", hour, minute);
            await Delay(0);
        }

        async Task SetupWeather()
        {
            await Delay(0);
            Random random = new Random(API.GetGameTimer().GetHashCode());
            Random randomSelect = new Random();

            int countOfWeathers = weathers.Count;
            int countOfWeatherKeys = weathers.Keys.Count;

            if (string.IsNullOrEmpty(weatherData.CurrentWeather))
            {
                weatherData.CurrentWeather = weathers.Keys.OrderBy(s => Guid.NewGuid()).First();
            }
            else
            {
                weatherData.CurrentWeather = weathers[weatherData.CurrentWeather].OrderBy(s => Guid.NewGuid()).First();
            }

            if (randomSelect.Next(0, 2) == 0)
            {
                weatherData.Wind = windWeathers[weatherData.CurrentWeather];
                weatherData.WindHeading = randomSelect.Next(0, 360);
            }

            TriggerClientEvent("curiosity:Client:Weather:Sync", weatherData.CurrentWeather, weatherData.Wind, weatherData.WindHeading);

            if (!isLive)
                Debug.WriteLine($"WEATHER CHANGE -> {weatherData}");
        }

        async Task ChangeWeather()
        {
            await Delay(MINUTES_TO_WAIT);
            await SetupWeather();
        }
    }
}
