using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
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
            LoadLosSantosIpls();

            API.SetWeatherOwnedByNetwork(false);

            await Session.Loading();

            List<string> vehicles = GetConfig().VehiclesToSuppress;

            for (int i = 0; i < vehicles.Count; i++)
            {
                int modelHash = API.GetHashKey(vehicles[i]);
                Model model = new Model(modelHash);

                if (!model.IsValid) continue;

                vehiclesToSuppress.Add(modelHash);
            }
        }

        private ClientConfig GetConfig()
        {
            ClientConfig config = new();

            string jsonFile = LoadResourceFile(GetCurrentResourceName(), "config/config.json"); // Fuck you VS2019 UTF8 BOM

            try
            {
                if (string.IsNullOrEmpty(jsonFile))
                {
                    Logger.Error($"config.json file is empty or does not exist, please fix this");
                }
                else
                {
                    return JsonConvert.DeserializeObject<ClientConfig>(jsonFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Config JSON File Exception\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }

        private async void UnloadLosSantosIpls()
        {
            // Ferris Wheel
            RemoveIpl("ferris_finale_anim");
            RemoveIpl("ferris_finale_anim_lod");

            // Tunnel Roof
            RemoveIpl("dt1_03_gr_closed");

            // Missing Elevators
            RemoveIpl("dt1_21_prop_lift");
            // RequestIpl("dt1_21_prop_lift_on");

            // Fountain Fix
            RemoveIpl("DT1_05_HC_REMOVE");

            RemoveIpl("cs5_4_trains"); // 2773.61 2835.327 35.1903
            RemoveIpl("airfield"); // 1743.682 3286.251 40.0875
            RemoveIpl("trailerparkA_grp1"); // Lost trailer
            RemoveIpl("dockcrane1"); // 889.3 -2910.9 40
            RemoveIpl("chophillskennel"); // 19.0568 536.4818 169.6277

            // FIB WINDOW: 136.1795f, -750.701f, 262.0516f
            RemoveIpl("atriumglstatic");

            // Hospital: 330.4596 -584.8196 42.3174
            RemoveIpl("RC12B_HospitalInterior"); // Broken interior
            RemoveIpl("RC12B_Destroyed"); // broken windows
            RemoveIpl("RC12B_Default"); // default look
            RemoveIpl("RC12B_Fixed"); // boarded up

            // Trevor: 1985.48132, 3828.76757, 32.5
            // Trash or Tidy.Only choose one.
            RemoveIpl("TrevorsTrailer");
            RemoveIpl("TrevorsTrailerTidy");

            // rails: 2626.374 2949.869 39.1409
            RemoveIpl("ld_rail_01_track");
            RemoveIpl("ld_rail_02_track");
            RemoveIpl("FBI_repair");

            // golf flags
            RemoveIpl("golfflags");

            // Casino Exterior;
            RemoveIpl("hei_dlc_casino_aircon");
            RemoveIpl("hei_dlc_casino_aircon_lod");
            RemoveIpl("hei_dlc_casino_door");
            RemoveIpl("hei_dlc_casino_door_lod");
            RemoveIpl("hei_dlc_vw_roofdoors_locked");
            RemoveIpl("hei_dlc_windows_casino");
            RemoveIpl("hei_dlc_windows_casino_lod");
            RemoveIpl("vw_ch3_additions");
            RemoveIpl("vw_ch3_additions_long_0");
            RemoveIpl("vw_ch3_additions_strm_0");
            RemoveIpl("vw_dlc_casino_door");
            RemoveIpl("vw_dlc_casino_door_lod");

            //RemoveIpl("tr_tuner_meetup");
            //RemoveIpl("tr_tuner_race_line");
            RemoveIpl("tr_tuner_shop_burton");
            RemoveIpl("tr_tuner_shop_mesa");
            RemoveIpl("tr_tuner_shop_mission");
            RemoveIpl("tr_tuner_shop_rancho");
            RemoveIpl("tr_tuner_shop_strawberry");

            SwitchTrainTrack(3, false); // Enable Metro
        }

        private async void LoadLosSantosIpls()
        {
            await Session.Loading();
            LoadMpDlcMaps();
            EnableMpDlcMaps(true);

            SwitchTrainTrack(3, true); // Enable Metro

            // Ferris Wheel
            RequestIpl("ferris_finale_anim");
            RequestIpl("ferris_finale_anim_lod");

            // Tunnel Roof
            RequestIpl("dt1_03_gr_closed");

            // Missing Elevators
            RequestIpl("dt1_21_prop_lift");
            // RequestIpl("dt1_21_prop_lift_on");

            // Fountain Fix
            RequestIpl("DT1_05_HC_REMOVE");

            RequestIpl("cs5_4_trains"); // 2773.61 2835.327 35.1903
            RequestIpl("airfield"); // 1743.682 3286.251 40.0875
            RequestIpl("trailerparkA_grp1"); // Lost trailer
            RequestIpl("dockcrane1"); // 889.3 -2910.9 40
            RequestIpl("chophillskennel"); // 19.0568 536.4818 169.6277

            // FIB WINDOW: 136.1795f, -750.701f, 262.0516f
            RequestIpl("atriumglstatic");

            // Hospital: 330.4596 -584.8196 42.3174
            RemoveIpl("RC12B_HospitalInterior"); // Broken interior
            RemoveIpl("RC12B_Destroyed"); // broken windows
            RequestIpl("RC12B_Default"); // default look
            RemoveIpl("RC12B_Fixed"); // boarded up

            // Trevor: 1985.48132, 3828.76757, 32.5
            // Trash or Tidy.Only choose one.
            RequestIpl("TrevorsTrailer");
            RequestIpl("TrevorsTrailerTidy");

            // rails: 2626.374 2949.869 39.1409
            RequestIpl("ld_rail_01_track");
            RequestIpl("ld_rail_02_track");
            RequestIpl("FBI_repair");

            // golf flags
            RequestIpl("golfflags");

            // Casino Exterior;
            RequestIpl("hei_dlc_casino_aircon");
            RequestIpl("hei_dlc_casino_aircon_lod");
            RequestIpl("hei_dlc_casino_door");
            RequestIpl("hei_dlc_casino_door_lod");
            RequestIpl("hei_dlc_vw_roofdoors_locked");
            RequestIpl("hei_dlc_windows_casino");
            RequestIpl("hei_dlc_windows_casino_lod");
            RequestIpl("vw_ch3_additions");
            RequestIpl("vw_ch3_additions_long_0");
            RequestIpl("vw_ch3_additions_strm_0");
            RequestIpl("vw_dlc_casino_door");
            RequestIpl("vw_dlc_casino_door_lod");

            // IPLs for Cayo Island in the Distance
            RequestIpl("xn_h4_islandx_terrain_01_slod");
            RequestIpl("xn_h4_islandx_terrain_02_slod");
            RequestIpl("xn_h4_islandx_terrain_04_slod");
            RequestIpl("xn_h4_islandx_terrain_05_slod");
            RequestIpl("xn_h4_islandx_terrain_06_slod");

            //RequestIpl("tr_tuner_meetup");
            //RequestIpl("tr_tuner_race_line");
            RequestIpl("tr_tuner_shop_burton");
            RequestIpl("tr_tuner_shop_mesa");
            RequestIpl("tr_tuner_shop_mission");
            RequestIpl("tr_tuner_shop_rancho");
            RequestIpl("tr_tuner_shop_strawberry");
        }

        //[TickHandler(SessionWait = true)]
        //private async static Task OnRemoveCargoShip()
        //{
        //    await BaseScript.Delay(10);

        //    if (IsIplActive("cargoship"))
        //        RemoveIpl("cargoship");

        //    if (IsIplActive("sunkcargoship"))
        //        RemoveIpl("sunkcargoship");
        //}

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
