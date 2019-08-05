using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core.Native;
using CitizenFX.Core;

namespace Curiosity.Server.net.Classes.Environment
{
    class WeatherSystems
    {
        static Server server = Server.GetInstance();

        static Dictionary<string, List<string>> weathers = new Dictionary<string, List<string>>();
        static Dictionary<string, bool> windWeathers = new Dictionary<string, bool>();

        static bool isChristmas = true;
        static bool isHalloween = true;
        static bool isLive = false;
        static bool weatherSetup = false;

        const int MINUTES_TO_WAIT = (1000 * 60) * 20;

        static Entity.WeatherData weatherData = new Entity.WeatherData();

        public static void Init()
        {
            isChristmas = API.GetConvar("christmas_weather", "false") == "true";
            isHalloween = API.GetConvar("halloween_weather", "false") == "true";
            isLive = API.GetConvar("server_live", "false") == "true";

            SetupWindWeather();
            SetupWeathers();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            SetupWeather();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            server.RegisterEventHandler("curiosity:Server:Weather:Sync", new Action<CitizenFX.Core.Player>(ClientSyncWeather));

            server.RegisterTickHandler(ChangeWeather);
        }

        static void SetupWindWeather()
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

        static void SetupWeathers()
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

        static async void ClientSyncWeather([FromSource]CitizenFX.Core.Player player)
        {
            await Server.Delay(1000);
            if (!weatherSetup)
            {
                await SetupWeather();
                player.TriggerEvent("curiosity:Client:Weather:Sync", weatherData.CurrentWeather, weatherData.Wind, weatherData.WindSpeed, weatherData.WindHeading);
            }
            else
            {
                player.TriggerEvent("curiosity:Client:Weather:Sync", weatherData.CurrentWeather, weatherData.Wind, weatherData.WindSpeed, weatherData.WindHeading);
            }
            await Server.Delay(0);
        }

        static async Task SetupWeather()
        {
            await Server.Delay(0);
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
                windSpeed = random.Next(0, 2);
            }

            if (weatherData.CurrentWeather == "THUNDER")
            {
                windSpeed = randomSelect.Next(4, 18);
                weatherData.WindSpeed = windSpeed;
            }

            Server.TriggerClientEvent("curiosity:Client:Weather:Sync", weatherData.CurrentWeather, weatherData.Wind, weatherData.WindSpeed, weatherData.WindHeading);

            weatherSetup = true;
        }

        static async Task ChangeWeather()
        {
            await Server.Delay(MINUTES_TO_WAIT);
            await SetupWeather();
        }
    }
}

