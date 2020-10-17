﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Entity;
using Curiosity.Server.net.Helpers;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Classes.Environment
{
    class WeatherSystems
    {
        static Server server = Server.GetInstance();

        static Dictionary<string, List<string>> weathers = new Dictionary<string, List<string>>();
        static Dictionary<string, bool> windWeathers = new Dictionary<string, bool>();

        static bool isChristmas = false;
        static bool isHalloween = false;
        static bool isLive = false;
        static bool weatherSetup = false;

        static int MINUTES_TO_WAIT = (1000 * 60) * 60;

        static WeatherData weatherData = new WeatherData();

        static List<string> allowedWeather = new List<string>()
        {
            "OVERCAST",
            "RAIN",
            "THUNDER",
            "CLOUDS",
            "EXTRASUNNY",
            "SMOG",
            "CLEAR",
            "FOGGY",
            "CLEARING",
            "BLIZZARD",
            "XMAS",
            "SNOWLIGHT",
            "SNOW",
            "HALLOWEEN"
        };

        public static void Init()
        {
            isChristmas = API.GetConvar("christmas_weather", "false") == "true";
            isHalloween = API.GetConvar("halloween_weather", "false") == "true";
            isLive = API.GetConvar("server_live", "false") == "true";

            if (isChristmas)
            {
                Log.Success("-----------------------------------------------------------------");
                Log.Success("-> CURIOSITY SERVER WEATHER: CHRISMAS <--------------------------");
                Log.Success("-----------------------------------------------------------------");
            }

            SetupWindWeather();
            SetupWeathers();

            SetupWeather();

            server.RegisterEventHandler("curiosity:Server:Weather:Sync", new Action<CitizenFX.Core.Player>(ClientSyncWeather));
            server.RegisterEventHandler("curiosity:server:weather:setWeather", new Action<CitizenFX.Core.Player, string>(SetWeather));

            server.RegisterTickHandler(ChangeWeather);
        }

        private static void SetWeather([FromSource]CitizenFX.Core.Player player, string weather)
        {
            if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;
            Session session = SessionManager.PlayerList[player.Handle];
            if (!session.IsDeveloper) return;

            string newWeather = weather.ToUpper();

            if (!allowedWeather.Contains(newWeather))
            {
                session.Player.Send(NotificationType.CHAR_LIFEINVADER, 20, $"Unknown Weather", $"'{newWeather}' is unknown", $"");
                return;
            }

            weatherData.CurrentWeather = newWeather;

            if (Server.random.Next(2) == 0)
            {
                weatherData.Wind = windWeathers[weatherData.CurrentWeather];
                weatherData.WindHeading = Server.random.Next(360);
            }

            float windSpeed = Server.random.Next(2);
            if (!weatherData.Wind)
                windSpeed = Server.random.Next(2);

            if (weatherData.CurrentWeather == "THUNDER")
                windSpeed = Server.random.Next(2, 4);

            weatherData.WindSpeed = windSpeed;

            isChristmas = (weatherData.CurrentWeather == "XMAS" || weatherData.CurrentWeather == "BLIZZARD" || weatherData.CurrentWeather == "SNOW" || weatherData.CurrentWeather == "SNOWLIGHT");
            isHalloween = (weatherData.CurrentWeather == "HALLOWEEN");

            Log.Verbose($"Developer Weather Update: {weatherData.CurrentWeather}, Wind: {weatherData.Wind} : s {weatherData.WindSpeed} : h {weatherData.WindHeading}, isChristmas: {isChristmas}, isHalloween: {isHalloween}");

            Server.TriggerClientEvent("curiosity:Client:Weather:Sync", weatherData.CurrentWeather, weatherData.Wind, weatherData.WindSpeed, weatherData.WindHeading, isChristmas, isHalloween);
        }

        static void SetupWindWeather()
        {
            windWeathers.Clear();
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
            windWeathers.Add("SNOWLIGHT", isChristmas);
            windWeathers.Add("SNOW", isChristmas);
            windWeathers.Add("HALLOWEEN", isHalloween);
        }

        static void SetupWeathers()
        {
            weathers.Clear();

            if (isChristmas)
            {
                weathers.Add("XMAS", new List<string> { "XMAS" });
            }
            else if (isHalloween)
            {
                weathers.Add("HALLOWEEN", new List<string> { "HALLOWEEN" });
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

                //weathers.Add("HALLOWEEN", new List<string> { "CLEAR", "CLOUDS", "SMOG", "FOGGY", "RAIN", "CLEARING" });
                //weathers.Add("SNOW", new List<string> { "CLEAR", "CLOUDS", "SMOG", "FOGGY", "RAIN", "CLEARING" });
                //weathers.Add("BLIZZARD", new List<string> { "CLEAR", "CLOUDS", "SMOG", "FOGGY", "RAIN", "CLEARING" });
                //weathers.Add("SNOWLIGHT", new List<string> { "CLEAR", "CLOUDS", "SMOG", "FOGGY", "RAIN", "CLEARING" });
            }
        }

        static async void ClientSyncWeather([FromSource]CitizenFX.Core.Player player)
        {
            await Server.Delay(1000);
            if (!weatherSetup)
            {
                await SetupWeather();
                player.TriggerEvent("curiosity:Client:Weather:Sync", weatherData.CurrentWeather, weatherData.Wind, weatherData.WindSpeed, weatherData.WindHeading, isChristmas, isHalloween);
            }
            else
            {
                player.TriggerEvent("curiosity:Client:Weather:Sync", weatherData.CurrentWeather, weatherData.Wind, weatherData.WindSpeed, weatherData.WindHeading, isChristmas, isHalloween);
            }
            await Server.Delay(0);
        }

        static async Task SetupWeather()
        {
            try
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
                    windSpeed = randomSelect.Next(2, 4);
                    weatherData.WindSpeed = windSpeed;
                }

                if (!isChristmas && (weatherData.CurrentWeather == "XMAS" || weatherData.CurrentWeather == "BLIZZARD" || weatherData.CurrentWeather == "SNOW" || weatherData.CurrentWeather == "SNOWLIGHT"))
                {
                    weatherData.CurrentWeather = weathers["CLEAR"].OrderBy(s => Guid.NewGuid()).First();
                }

                if (isChristmas)
                    weatherData.CurrentWeather = "XMAS";

                if (isHalloween)
                    weatherData.CurrentWeather = "HALLOWEEN";

                Server.TriggerClientEvent("curiosity:Client:Weather:Sync", weatherData.CurrentWeather, weatherData.Wind, weatherData.WindSpeed, weatherData.WindHeading, isChristmas, isHalloween);

                weatherSetup = true;
            }
            catch (Exception ex)
            {
                // 
            }
        }

        static async Task ChangeWeather()
        {
            //if (!Server.isLive)
            //{
            //    MINUTES_TO_WAIT = (1000 * 60);
            //}

            long gameTime = API.GetGameTimer();
            while ((API.GetGameTimer() - gameTime) < MINUTES_TO_WAIT)
            {
                await BaseScript.Delay(60000);
            }

            await Server.Delay(MINUTES_TO_WAIT);

            isChristmas = API.GetConvar("christmas_weather", "false") == "true";
            isHalloween = API.GetConvar("halloween_weather", "false") == "true";

            SetupWindWeather();
            SetupWeathers();

            await SetupWeather();
        }
    }
}

