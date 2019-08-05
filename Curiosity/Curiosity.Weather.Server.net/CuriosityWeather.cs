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

        static bool weatherSetup = false;

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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            SetupWeather();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            EventHandlers["curiosity:Server:Weather:Sync"] += new Action<Player>(ClientSyncWeather);
            EventHandlers["curiosity:Server:Time:Sync"] += new Action<Player>(ClientSyncTime);

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
            if (isChristmas)
            {
                weathers.Add("SNOW", new List<string> { "BLIZZARD", "RAIN", "SNOWLIGHT" });
                weathers.Add("BLIZZARD", new List<string> { "SNOW", "SNOWLIGHT", "THUNDER" });
                weathers.Add("SNOWLIGHT", new List<string> { "SNOW", "RAIN", "CLEARING" });
            }
            else if (isHalloween)
            {
                weathers.Add("HALLOWEEN", new List<string> { "CLOUDS", "RAIN", "CLEARING", "CLEAR" });
            }
            else
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
            }
        }

        async void ClientSyncTime([FromSource]Player player)
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

                //double hourDouble = timeNow.TotalMinutes % 24;
                //double minuteDouble = (hourDouble % 1) * 60;

                double hourDouble = (timeNow.TotalSeconds / 180) % 24; 
                double minuteDouble = (hourDouble % 1) * 60;

                hour = (int)hourDouble;
                minute = (int)minuteDouble;

                if (hour > 23)
                {
                    Console.WriteLine($"[ERROR] ServerTime -> Hour greater than 23 ({hour})", ConsoleColor.Red);
                }

                TriggerClientEvent("curiosity:Client:Time:Sync", hour, minute);
            }
        }

        async void ClientSyncWeather([FromSource]Player player)
        {
            await Delay(1000);
            if (!weatherSetup)
            {
                await SetupWeather();
                player.TriggerEvent("curiosity:Client:Weather:Sync", weatherData.CurrentWeather, weatherData.Wind, weatherData.WindSpeed, weatherData.WindHeading);
                Debug.WriteLine($"{player.Name} - WEATHER SYNC -> {weatherData}");
            }
            else
            {
                player.TriggerEvent("curiosity:Client:Weather:Sync", weatherData.CurrentWeather, weatherData.Wind, weatherData.WindSpeed, weatherData.WindHeading);
                Debug.WriteLine($"{player.Name} - WEATHER SYNC -> {weatherData}");
            }
            ClientSyncTime(player);
            await Delay(0);
        }

        async Task SetupWeather()
        {
            Debug.WriteLine("WEATHER SYNC REQUEST");

            await Delay(0);
            Random random = new Random(API.GetGameTimer().GetHashCode());
            Random randomSelect = new Random();

            int countOfWeathers = weathers.Count;
            int countOfWeatherKeys = weathers.Keys.Count;
            float windSpeed = random.Next(0, 2);

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

            if (!weatherData.Wind)
            {
                windSpeed = 0f;
            }

            if (weatherData.CurrentWeather == "THUNDER")
            {
                windSpeed = randomSelect.Next(4, 18);
                weatherData.WindSpeed = windSpeed;
            }

            TriggerClientEvent("curiosity:Client:Weather:Sync", weatherData.CurrentWeather, weatherData.Wind, weatherData.WindSpeed, weatherData.WindHeading);

            Debug.WriteLine($"WEATHER CHANGE -> {weatherData}");
            weatherSetup = true;
        }

        async Task ChangeWeather()
        {
            await Delay(MINUTES_TO_WAIT);
            await SetupWeather();
        }
    }
}
