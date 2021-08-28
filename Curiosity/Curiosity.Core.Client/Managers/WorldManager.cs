using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers
{
    public class WorldManager : Manager<WorldManager>
    {
        List<int> vehiclesToSuppress = new List<int>();

        WeatherType lastWeather = WeatherType.UNKNOWN;
        CuriosityWeather CuriosityWeather = new CuriosityWeather();
        DateTime lastRunWeatherUpdate = DateTime.Now;
        DateTime lastRunVehicleSuppression = DateTime.Now;

        List<Vehicle> vehiclesToLock = new List<Vehicle>();

        // Time
        double clientBaseTime = 0;
        double clientTimeOffset = 0;
        double clientTimer = 0;
        int hour;
        int minute;

        bool isWeatherLocked = false;
        bool isTimeLocked = false;
        int timeLockHour = 0;
        int timeLockMins = 0;

        public void LockAndSetTime(int hour, int minute)
        {
            timeLockHour = hour;
            timeLockMins = minute;
            isTimeLocked = true;
        }

        public void UnlockTime()
        {
            isTimeLocked = false;
        }

        public void LockAndSetWeather(WeatherType weatherType)
        {
            ClearOverrideWeather();
            ClearWeatherTypePersist();
            SetWeatherTypeOvertimePersist($"{weatherType}", 2f);
            isWeatherLocked = true;
        }

        public void UnlockAndUpdateWeather()
        {
            isWeatherLocked = false;
            UpdateWeather();
        }

        public override async void Begin()
        {
            EventSystem.Attach("world:time", new EventCallback(metadata =>
            {
                clientBaseTime = metadata.Find<double>(0);
                clientTimeOffset = metadata.Find<double>(1);
                return null;
            }));

            Instance.ExportDictionary.Add("GetWeather", new Func<int>(
                () =>
                {
                    return (int)CuriosityWeather.WeatherType;
                }));

            UpdateWeather();

            API.SetWeatherOwnedByNetwork(false);

            await Session.Loading();

            List<string> vehicles = ConfigurationManager.GetModule().VehiclesToSuppress();

            for (int i = 0; i < vehicles.Count; i++)
            {
                int modelHash = API.GetHashKey(vehicles[i]);
                Model model = new Model(modelHash);

                if (!model.IsValid) continue;

                vehiclesToSuppress.Add(modelHash);
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task OnWeatherSyncTick()
        {
            if (DateTime.Now.Subtract(lastRunWeatherUpdate).TotalSeconds >= 60)
            {
                UpdateWeather();
            }
            await BaseScript.Delay(1000);
        }

        async void UpdateWeather()
        {
            await Session.Loading();

            if (isWeatherLocked) return;

            lastRunWeatherUpdate = DateTime.Now;

            Vector3 pos = Cache.PlayerPed.Position;

            string zoneStr = GetNameOfZone(pos.X, pos.Y, pos.Z);
            Enum.TryParse(zoneStr, out SubRegion subRegion);

            CuriosityWeather = await EventSystem.Request<CuriosityWeather>("weather:sync", (int)subRegion);
            WeatherType weatherType = CuriosityWeather.WeatherType;

            if (!weatherType.Equals(lastWeather))
            {
                string area = World.GetZoneLocalizedName(pos);

                int interiorId = GetInteriorFromEntity(PlayerPedId());

                if (interiorId == 0)
                    Notify.TouristBoard($"{area} Weather Update", GetForecastText(weatherType));
                
                await BaseScript.Delay(15000);

                ClearOverrideWeather();
                ClearWeatherTypePersist();
                SetWeatherTypeOvertimePersist($"{weatherType}", 30f);
                // SetWeatherTypePersist($"{weatherType}");
                // SetWeatherTypeNow($"{weatherType}");
                // SetWeatherTypeNowPersist($"{weatherType}");

                if (Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed)
                {
                    Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
                    if (vehicle is not null && vehicle.Exists() && vehicle.Model.IsPlane)
                    {
                        if (weatherType.Equals(WeatherType.THUNDER) && weatherType.Equals(WeatherType.BLIZZARD))
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, 1.0f);
                        }
                        else if (weatherType.Equals(WeatherType.RAIN))
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, .75f);
                        }
                        else if (weatherType.Equals(WeatherType.EXTRASUNNY))
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, .25f);
                        }
                        else
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, .5f);
                        }
                    }
                }

                lastWeather = weatherType;
            }
        }

        private string GetForecastText(WeatherType weather)
        {
            switch (weather)
            {
                case WeatherType.EXTRASUNNY:
                    return "Skies will be completely clear for a few hours.";
                case WeatherType.NEUTRAL:
                    return "Strange shit happening on the skies soon. Beware.";
                case WeatherType.XMAS:
                    return "Christmas itself is expected. Ho ho ho!";
                case WeatherType.FOGGY:
                    return "Its going to be foggy for a while.";
                case WeatherType.THUNDER:
                    return "Heavy rain accompanied by thunder is expected.";
                case WeatherType.OVERCAST:
                    return "Its going to be very cloudy for some time.";
                case WeatherType.SNOW:
                    return "Copious ammounts of snow for the next hours.";
                case WeatherType.SMOG:
                    return "Clear skies accompanied by little fog are expected.";
                case WeatherType.SNOWLIGHT:
                    return "Some snow is expected.";
                case WeatherType.BLIZZARD:
                    return "A big blizzard is expected.";
                case WeatherType.CLOUDS:
                    return "Clouds will cover the sky for some hours.";
                case WeatherType.CLEAR:
                    return "The skies will be clear for the next couple of hours.";
                case WeatherType.RAIN:
                    return "Rain is expected to feature the following hours.";
                case WeatherType.CLEARING:
                    return "Skies will clear in the next hours.";
                default:
                    return "No idea.";
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task OnWorldTimeSyncTick()
        {
            await BaseScript.Delay(0);

            while(!Cache.Player.Character.MarkedAsRegistered)
            {
                NetworkOverrideClockTime(12, 1, 0);
                SetClockTime(12, 1, 0);
                SetWeatherTypeNow("EXTRASUNNY");
                await BaseScript.Delay(0);
            }

            if (isTimeLocked)
            {
                NetworkOverrideClockTime(timeLockHour, timeLockMins, 0);
                SetClockTime(timeLockHour, timeLockMins, 0);
                await BaseScript.Delay(500);
                return;
            }

            //int interior = GetInteriorFromEntity(Cache.PlayerPed.Handle);

            //if (interior > 0) // If they are indoors, lock the timer
            //{
            //    NetworkOverrideClockTime(0, 0, 0);
            //    SetClockTime(0, 0, 0);
            //    return;
            //}

            double newBaseTime = clientBaseTime;
            if ((GetGameTimer() - 500) > clientTimer)
            {
                newBaseTime += 0.25;
                clientTimer = GetGameTimer();
            }

            clientBaseTime = newBaseTime;

            hour = (int)Math.Floor(((clientBaseTime + clientTimeOffset) / 60) % 24);
            minute = (int)Math.Floor((clientBaseTime + clientTimeOffset) % 60);

            NetworkOverrideClockTime(hour, minute, 0);
            SetClockTime(hour, minute, 0);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnLockVehicles()
        {
            if (vehiclesToLock.Count > 0)
                vehiclesToLock.Clear();

            vehiclesToLock = World.GetAllVehicles().Select(x => x).Where(x => Cache.PlayerPed.IsInRangeOf(x.Position, 50f)).ToList();

            vehiclesToLock.ForEach(vehicle =>
            {
                bool serverSpawned = vehicle.State.Get(StateBagKey.VEH_SPAWNED) ?? false;

                if (serverSpawned && vehicle.LockStatus.Equals(VehicleLockStatus.LockedForPlayer))
                {
                    vehicle.LockStatus = VehicleLockStatus.Unlocked;
                }
                else if (!serverSpawned && vehicle.IsVisible)
                { 
                        vehicle.LockStatus = VehicleLockStatus.LockedForPlayer;
                }
                
            });

            await BaseScript.Delay(500);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnRemoveSuppressedVehicles()
        {
            if (DateTime.Now.Subtract(lastRunVehicleSuppression).TotalSeconds >= 10)
            {
                vehiclesToSuppress.ForEach(veh =>
                {
                    SetVehicleModelIsSuppressed((uint)veh, true);
                });

                List<Vehicle> vehicles = World.GetAllVehicles().Select(x => x).Where(x => Cache.PlayerPed.IsInRangeOf(x.Position, 50f) && vehiclesToSuppress.Contains(x.Model.Hash)).ToList();

                vehicles.ForEach(veh =>
                {
                    bool serverSpawned = veh.State.Get(StateBagKey.VEH_SPAWNED) ?? false;
                    bool shouldBeDeleted = veh.State.Get(StateBagKey.ENTITY_DELETE) ?? false;

                    if (veh.Driver == Game.PlayerPed)
                    {
                        Game.PlayerPed.Task.WarpOutOfVehicle(veh);
                        NotificationManager.GetModule().Warn($"This is a blacklisted vehicle.");
                    }

                    if (!serverSpawned)
                        veh.RemoveFromWorld();

                    if (shouldBeDeleted)
                        veh.RemoveFromWorld();
                });

                vehicles = null;

                lastRunVehicleSuppression = DateTime.Now;
            }
        }
    }
}
