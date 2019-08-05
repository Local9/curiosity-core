﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.net
{
    public class CuriosityWeather : BaseScript
    {
        public CuriosityWeather()
        {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["playerSpawned"] += new Action(OnPlayerSpawned);
            EventHandlers["curiosity:Client:Weather:Sync"] += new Action<string, bool, float, float>(WeatherSync);

            Tick += TimeNetworkSync;
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

        void OnPlayerSpawned()
        {
            TriggerServerEvent("curiosity:Server:Weather:Sync");
        }

        void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;
            TriggerServerEvent("curiosity:Server:Weather:Sync");
        }

        async Task TimeNetworkSync()
        {
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            API.NetworkGetServerTime(ref hours, ref minutes, ref seconds);
            API.NetworkOverrideClockTime(hours, minutes, 0);
            await Task.FromResult(0);
        }

        async void WeatherSync(string weather, bool wind, float windSpeed, float windHeading)
        {
            Debug.WriteLine($"weather: {weather}, wind: {wind}, windSpeed: {windSpeed}, windHeading: {windHeading}");

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
                API.SetWindSpeed(windSpeed);
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
