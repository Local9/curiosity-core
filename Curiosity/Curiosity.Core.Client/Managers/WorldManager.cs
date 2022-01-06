using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Extensions;
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

        CuriosityWeather CuriosityWeather = new CuriosityWeather();
        DateTime lastRunWeatherUpdate = DateTime.Now;
        DateTime lastRunVehicleSuppression = DateTime.Now;

        List<Vehicle> vehiclesToLock = new List<Vehicle>();

        Dictionary<Region, WeatherType> regionalWeather = new Dictionary<Region, WeatherType>();

        // Time
        double hours = 0;
        double minutes = 0;
        double clientTimer = 0;
        int hour;
        int minute;

        bool isWeatherLocked = false;
        bool isTimeLocked = false;
        int timeLockHour = 0;
        int timeLockMins = 0;

        private DispatchType[] dispatchTypes = new DispatchType[13] {
            DispatchType.DT_PoliceAutomobile,
            DispatchType.DT_PoliceHelicopter,
            DispatchType.DT_SwatAutomobile,
            DispatchType.DT_PoliceRiders,
            DispatchType.DT_PoliceVehicleRequest,
            DispatchType.DT_PoliceRoadBlock,
            DispatchType.DT_PoliceAutomobileWaitPulledOver,
            DispatchType.DT_PoliceAutomobileWaitCruising,
            DispatchType.DT_SwatHelicopter,
            DispatchType.DT_PoliceBoat,
            DispatchType.DT_ArmyVehicle,
            DispatchType.DT_AmbulanceDepartment,
            DispatchType.DT_FireDepartment
        };


        public override async void Begin()
        {
            EventSystem.Attach("world:time:sync", new EventCallback(metadata =>
            {
                int hours = metadata.Find<int>(0);
                int minutes = metadata.Find<int>(1);

                if (hours > 23)
                    hours = 0;

                if (minutes > 59)
                    minutes = 0;

                // NetworkOverrideClockTime(hour, minutes, 0);

                return null;
            }));

            EventSystem.Attach("world:server:weather:sync", new EventCallback(metadata =>
            {
                regionalWeather = metadata.Find<Dictionary<Region, WeatherType>>(0);

                return null;
            }));

            Instance.ExportDictionary.Add("GetWeather", new Func<int>(
                () =>
                {
                    return (int)CuriosityWeather.WeatherType;
                }));

            UnlockAndUpdateWeather();

            API.SetWeatherOwnedByNetwork(false);

            await Session.Loading();

            UnlockTime();

            List<string> vehicles = ConfigurationManager.GetModule().VehiclesToSuppress();

            for (int i = 0; i < vehicles.Count; i++)
            {
                int modelHash = API.GetHashKey(vehicles[i]);
                Model model = new Model(modelHash);

                if (!model.IsValid) continue;

                vehiclesToSuppress.Add(modelHash);
            }

            ToggleDispatch(false); // turn off all dispatch
        }

        public async Task<bool> IsWinter()
        {
            bool isWinter = await EventSystem.Request<bool>("weather:is:winter");

            Logger.Debug($"isWinter: {isWinter}");

            return isWinter;
        }

        public async Task<bool> IsHalloween()
        {
            bool isHalloween = await EventSystem.Request<bool>("weather:is:halloween");

            Logger.Debug($"isHalloween: {isHalloween}");

            return isHalloween;
        }

        public void LockAndSetTime(int hour, int minute)
        {
            if (hour > 23)
                hour = 0;

            if (minute > 59)
                minute = 0;

            NetworkOverrideClockTime(hour, minute, 0);
            
            isTimeLocked = true;

            Logger.Debug($"LockAndSetTime: {hour:00}:{minute:00}");
        }

        public void UnlockTime()
        {
            NetworkClearClockTimeOverride();
            isTimeLocked = false;
            
            Logger.Debug($"UnlockTime");
        }

        public void LockAndSetWeather(WeatherType weatherType)
        {
            ClearOverrideWeather();
            ClearWeatherTypePersist();
            World.Weather = (Weather)weatherType;
            World.TransitionToWeather((Weather)weatherType, 2f);

            isWeatherLocked = true;
            Logger.Debug($"LockAndSetWeather: {World.Weather}");
        }

        public async void UnlockAndUpdateWeather()
        {
            isWeatherLocked = false;
            await BaseScript.Delay(100);

            World.Weather = (Weather)WeatherType.UNKNOWN; // force changes
            regionalWeather = await EventSystem.Request<Dictionary<Region, WeatherType>>("weather:sync:regions");

            UpdateWeather(true);
            Logger.Debug($"UnlockAndUpdateWeather");
        }

        //[TickHandler]
        //private async Task OnWeatherRegionSyncTick()
        //{
        //    if (DateTime.UtcNow > lastRunWeatherUpdate)
        //    {
        //        regionalWeather = await EventSystem.Request<Dictionary<Region, WeatherType>>("weather:sync:regions");
        //    }
        //    await BaseScript.Delay(1000);
        //}

        [TickHandler(SessionWait = true)]
        private async Task OnWeatherSyncTick()
        {
            if (DateTime.UtcNow > lastRunWeatherUpdate)
            {
                UpdateWeather();
            }
            await BaseScript.Delay(1000);
        }

        public async void UpdateWeather(bool instant = false, SubRegion subRegion = SubRegion.UNKNOWN)
        {
            if (isWeatherLocked)
            {
                // Logger.Debug($"Weather State: Locked | {lastWeather}");
                return;
            }

            lastRunWeatherUpdate = DateTime.UtcNow.AddSeconds(15);

            Vector3 pos = Cache.PlayerPed.Position;

            if (subRegion.Equals(SubRegion.UNKNOWN))
            {
                string zoneStr = GetNameOfZone(pos.X, pos.Y, pos.Z);
                Enum.TryParse(zoneStr, out subRegion);
            }

            Region region = MapRegions.RegionBySubRegion[subRegion];

            WeatherType weatherType = regionalWeather[region];

            if (Logger.IsDebugTimeEnabled)
                Logger.Debug($"wt: {weatherType}, sr: {subRegion}, cw: {World.Weather}");

            ToggleAlamoSea(region);

            if (!World.Weather.Equals((Weather)weatherType))
            {
                string area = World.GetZoneLocalizedName(pos);
                int interiorId = GetInteriorFromEntity(PlayerPedId());

                if (instant)
                {
                    World.Weather = (Weather)weatherType;
                    World.TransitionToWeather((Weather)weatherType, 1f);
                    Logger.Debug($"Force weather change: {(Weather)weatherType}");

                    await BaseScript.Delay(5000);

                    SetTrails();

                    if (interiorId == 0)
                        NotificationManager.GetModule().Info($"<b>🌡 Weather Update 🌡</b><br /><b>Area</b>: {area}<br />{GetForecastText(weatherType)}");

                    return;
                }

                if (interiorId == 0)
                    NotificationManager.GetModule().Info($"<b>🌡 Weather Update 🌡</b><br /><b>Area</b>: {area}<br />{GetForecastText(weatherType)}");

                await BaseScript.Delay(5000);

                ClearOverrideWeather();
                ClearWeatherTypePersist();
                World.TransitionToWeather((Weather)weatherType, 30f);

                await BaseScript.Delay(5000);

                SetTrails();

                if (Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed)
                {
                    Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
                    if (vehicle is not null && vehicle.Exists() && vehicle.Model.IsPlane)
                    {
                        if (weatherType.Equals(WeatherType.NEUTRAL) && weatherType.Equals(WeatherType.SNOWING))
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, 1.0f);
                        }
                        else if (weatherType.Equals(WeatherType.CLEARING))
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, .20f);
                        }
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, .5f);
                        }
                    }
                }
            }
        }

        void ToggleAlamoSea(Region region)
        {
            bool enable = (World.Weather == Weather.Christmas || World.Weather == Weather.Snowing || World.Weather == Weather.Blizzard) && (region == Region.GrandSenoraDesert);
            bool isIplActive = IsIplActive("alamo_ice");

            if (enable && !isIplActive)
                RequestIpl("alamo_ice");

            if (!enable && isIplActive)
                RemoveIpl("alamo_ice");
        }

        void SetTrails()
        {
            bool trails = World.Weather == Weather.Christmas;
            Logger.Debug($"Trails: {trails}");
            SetForceVehicleTrails(trails);
            SetForcePedFootstepsTracks(trails);
        }

        private string GetForecastText(WeatherType weather)
        {
            switch (weather)
            {
                case WeatherType.EXTRASUNNY:
                    return "☀️ Skies will be completely clear for a few hours.";
                case WeatherType.CLOUDS:
                    return "☁️ Clouds will cover the sky for some hours.";
                case WeatherType.SNOWLIGHT:
                    return "Copious ammounts of snow for the next hours.";
                case WeatherType.FOGGY:
                    return "🌫 Its going to be foggy for a while.";
                case WeatherType.NEUTRAL:
                    return "✨ Strange shit happening on the skies soon. Beware.";
                case WeatherType.OVERCAST:
                    return "☁️ Its going to be very cloudy for some time.";
                case WeatherType.CHRISTMAS:
                    return "🎄 Christmas itself is expected. Ho ho ho!";
                case WeatherType.SMOG:
                    return "🌫 Clear skies accompanied by little fog are expected.";
                case WeatherType.BLIZZARD:
                    return "❄️ A big blizzard is expected.";
                case WeatherType.SNOWING:
                    return "❄️ Some snow is expected.";
                case WeatherType.RAINING:
                    return "🌧 Rain is expected to feature the following hours.";
                case WeatherType.CLEAR:
                    return "☀️ The skies will be clear for the next couple of hours.";
                case WeatherType.CLEARING:
                    return "🌧 Skies will clear in the next hours.";
                case WeatherType.THUNDERSTORM:
                    return "🌩 Heavy rain accompanied by thunder is expected.";
                default:
                    return "😱 No idea. We've lost the plot";
            }
        }


        // TRUST IN THE NETWORK
        //[TickHandler(SessionWait = true)]
        private async Task OnWorldTimeSyncTick()
        {
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            NetworkGetGlobalMultiplayerClock(ref hours, ref minutes, ref seconds);

            Screen.ShowSubtitle($"{hours:00}:{minutes:00}:{seconds:00}");

            //while (!Cache.Player.Character.MarkedAsRegistered)
            //{
            //    NetworkOverrideClockTime(12, 1, 0);
            //    SetClockTime(12, 1, 0);
            //    SetWeatherTypeNow("EXTRASUNNY");
            //    await BaseScript.Delay(0);
            //}

            //if (isTimeLocked)
            //{
            //    NetworkOverrideClockTime(timeLockHour, timeLockMins, 0);
            //    SetClockTime(timeLockHour, timeLockMins, 0);
            //    await BaseScript.Delay(0);
            //    return;
            //}

            //double newBaseTime = clientBaseTime;
            //if ((GetGameTimer() - 500) > clientTimer)
            //{
            //    newBaseTime += 0.25;
            //    clientTimer = GetGameTimer();
            //}

            //clientBaseTime = newBaseTime;

            //hour = (int)Math.Floor(((clientBaseTime + clientTimeOffset) / 60) % 24);
            //minute = (int)Math.Floor((clientBaseTime + clientTimeOffset) % 60);

            //NetworkOverrideClockTime(hour, minute, 0);
            //SetClockTime(hour, minute, 0);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnLockVehicles()
        {
            if (vehiclesToLock.Count > 0)
                vehiclesToLock.Clear();

            vehiclesToLock = World.GetAllVehicles().Select(x => x).Where(x => Cache.PlayerPed.IsInRangeOf(x.Position, 10f)).ToList();

            vehiclesToLock.ForEach(async vehicle =>
            {
                bool serverSpawned = vehicle.State.Get(StateBagKey.VEH_SPAWNED) ?? false;

                if (serverSpawned && vehicle.LockStatus.Equals(VehicleLockStatus.LockedForPlayer))
                {
                    vehicle.LockStatus = VehicleLockStatus.Unlocked;
                }

                if (!serverSpawned && !vehicle.LockStatus.Equals(VehicleLockStatus.LockedForPlayer))
                {
                    vehicle.LockStatus = VehicleLockStatus.LockedForPlayer;

                    if (vehicle.Driver == Game.PlayerPed)
                    {
                        Game.PlayerPed.Task.WarpOutOfVehicle(vehicle);
                        await BaseScript.Delay(100);
                        vehicle.Dispose();
                        Notify.Info($"Vehicle was not created via the server and has been removed.");
                    }
                }
            });

            await BaseScript.Delay(1000);
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

                    if (veh.Driver == Game.PlayerPed && !Cache.Player.User.IsStaff && !serverSpawned)
                    {
                        Game.PlayerPed.Task.WarpOutOfVehicle(veh);
                        NotificationManager.GetModule().Warn($"This is a blacklisted vehicle.");
                        veh.Dispose();
                    }
                });

                vehicles = null;

                lastRunVehicleSuppression = DateTime.Now;
            }
        }

        private void ToggleDispatch(bool toggle)
        {
            for (int i = 0; i < dispatchTypes.Length; i++)
            {
                EnableDispatchService((int)dispatchTypes[i], toggle);
            }
        }
    }
}
