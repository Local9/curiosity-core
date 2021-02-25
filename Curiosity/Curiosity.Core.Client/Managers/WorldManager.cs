using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
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
        List<VehicleHash> vehiclesToSuppress = new List<VehicleHash>()
        {
            VehicleHash.Shamal,
            VehicleHash.Luxor,
            VehicleHash.Luxor2,
            VehicleHash.Jet,
            VehicleHash.Lazer,
            VehicleHash.Titan,
            VehicleHash.Barracks,
            VehicleHash.Barracks2,
            VehicleHash.Barracks3,
            VehicleHash.Crusader,
            VehicleHash.Rhino,
            VehicleHash.Airtug,
            VehicleHash.Ripley,
            VehicleHash.Asea2,
            VehicleHash.Burrito5,
            VehicleHash.Emperor3,
            VehicleHash.Mesa2,
            VehicleHash.PoliceOld1,
            VehicleHash.PoliceOld2,
            VehicleHash.RancherXL2,
            VehicleHash.Sadler2,
            VehicleHash.Stockade3,
            VehicleHash.Tractor3,
            VehicleHash.Buzzard,
            VehicleHash.Buzzard2,
            VehicleHash.Cargobob,
            VehicleHash.Cargobob2,
            VehicleHash.Cargobob3,
            VehicleHash.Cargobob4,
            VehicleHash.Besra
        };

        WeatherType lastWeather = WeatherType.UNKNOWN;
        CuriosityWeather CuriosityWeather = new CuriosityWeather();
        DateTime lastRunWeatherUpdate = DateTime.Now;
        DateTime lastRunVehicleSuppression = DateTime.Now;

        // Time
        double clientBaseTime = 0;
        double clientTimeOffset = 0;
        double clientTimer = 0;
        int hour;
        int minute;

        public override void Begin()
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
                    Instance.ExportDictionary["curiosity-ui"].SetWeather((int)CuriosityWeather.WeatherType, (int)CuriosityWeather.Season);
                    return (int)CuriosityWeather.WeatherType;
                }));

            UpdateWeather();
            LoadIpls();
            LoadScenarios();
        }

        private async void LoadScenarios()
        {
            await Session.Loading();
            await BaseScript.Delay(10000);
            // Zancudo
            SetScenarioGroupEnabled("fort_zancudo_guards", true);
            SetScenarioGroupEnabled("army_guard", false);
            SetScenarioGroupEnabled("army_heli", false);
            // Zancudo Types
            SetScenarioTypeEnabled("WORLD_VEHICLE_MILITARY_PLANES_SMALL", false);
            SetScenarioTypeEnabled("WORLD_VEHICLE_MILITARY_PLANES_BIG", false);
            // Air
            SetScenarioGroupEnabled("blimp", true);
            SetScenarioGroupEnabled("lsa_planes", false);
            SetScenarioGroupEnabled("sandy_planes", false);
            SetScenarioGroupEnabled("grapeseed_planes", false);
            SetScenarioGroupEnabled("ng_planes", false);
            // cinemas
            SetScenarioGroupEnabled("cinema_downtown", true);
            SetScenarioGroupEnabled("cinema_morningwood", true);
            SetScenarioGroupEnabled("cinema_textile", true);
            // Banks
            SetScenarioGroupEnabled("city_banks", true);
            SetScenarioGroupEnabled("countryside_banks", true);
            SetScenarioGroupEnabled("paleto_bank", true);
            // police
            SetScenarioGroupEnabled("guards_at_prison", true);
            SetScenarioGroupEnabled("paleto_cops", true);
            SetScenarioGroupEnabled("police_at_court", true);
            SetScenarioGroupEnabled("police_pound1", true);
            SetScenarioGroupEnabled("police_pound2", true);
            SetScenarioGroupEnabled("police_pound3", true);
            SetScenarioGroupEnabled("police_pound4", true);
            SetScenarioGroupEnabled("police_pound5", true);
            SetScenarioGroupEnabled("prison_towers", true);
            SetScenarioGroupEnabled("prison_transport", true);
            SetScenarioGroupEnabled("sandy_cops", true);
            SetScenarioGroupEnabled("mp_police", true);
            SetScenarioGroupEnabled("mp_police2", true);
            // FIB
            SetScenarioGroupEnabled("fib_group_1", true);
            SetScenarioGroupEnabled("fib_group_2", true);
            // Extra groups
            SetScenarioGroupEnabled("scrap_security", true);
            SetScenarioGroupEnabled("vagos_hangout", true);
            SetScenarioGroupEnabled("lost_hangout", true);
            SetScenarioGroupEnabled("lost_bikers", true);
            SetScenarioGroupEnabled("observatory_bikers", true);
            SetScenarioGroupEnabled("chinese2_hillbillies", true);
            SetScenarioGroupEnabled("chinese2_lunch", true);
            SetScenarioGroupEnabled("movie_studio_security", true);
            // facilities
            SetScenarioGroupEnabled("facility_cannon", false);
            SetScenarioGroupEnabled("facility_main_1", false);
            SetScenarioGroupEnabled("facility_main_2", false);
            SetScenarioGroupEnabled("facility_main_3", false);
            // Others
            SetScenarioGroupEnabled("dealership", true);
            SetScenarioGroupEnabled("kortz_security", true);
            SetScenarioGroupEnabled("ammunation", true);
            SetScenarioGroupEnabled("rampage1", true);
            SetScenarioGroupEnabled("sew_machine", true);
            SetScenarioGroupEnabled("solomon_gate", true);
            SetScenarioGroupEnabled("attract_pap", true);
            SetScenarioGroupEnabled("quarry", true);
            // Night Clubs
            SetScenarioGroupEnabled("club_cypress_flats_warehouse", true);
            SetScenarioGroupEnabled("club_del_perro_building", true);
            SetScenarioGroupEnabled("club_vinewood_dt", true);
            SetScenarioGroupEnabled("club_elysian_island_warehouse", true);
            SetScenarioGroupEnabled("club_la_mesa_warehouse", true);
            SetScenarioGroupEnabled("club_lsia_warehouse", true);
            SetScenarioGroupEnabled("club_mission_row_building", true);
            SetScenarioGroupEnabled("club_strawberry_warehouse", true);
            SetScenarioGroupEnabled("club_vespucci_canals_building", true);
            SetScenarioGroupEnabled("club_west_vinewood_building", true);
            // Animals
            SetScenarioGroupEnabled("armenian_cats", true);
            // Triathlon
            SetScenarioGroupEnabled("triathlon_1", false);
            SetScenarioGroupEnabled("triathlon_1_start", false);
            SetScenarioGroupEnabled("triathlon_2", false);
            SetScenarioGroupEnabled("triathlon_2_start", false);
            SetScenarioGroupEnabled("triathlon_3", false);
            SetScenarioGroupEnabled("triathlon_3_start", false);

            Logger.Verbose($"[SCENARIOS] Initiated");
        }

        private void LoadIpls()
        {
            LoadMpDlcMaps();
            EnableMpDlcMaps(true);

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
        }

        [TickHandler]
        private async static Task OnRemoveCargoShip()
        {
            RemoveIpl("cargoship");
            RemoveIpl("sunkcargoship");
        }

        [TickHandler]
        private async Task OnWeatherSyncTick()
        {
            if (DateTime.Now.Subtract(lastRunWeatherUpdate).TotalSeconds >= 60)
            {
                UpdateWeather();
            }
        }

        async void UpdateWeather()
        {
            lastRunWeatherUpdate = DateTime.Now;

            Vector3 pos = Game.PlayerPed.Position;

            string zoneStr = GetNameOfZone(pos.X, pos.Y, pos.Z);
            Enum.TryParse(zoneStr, out SubRegion subRegion);

            CuriosityWeather = await EventSystem.Request<CuriosityWeather>("weather:sync", (int)subRegion);
            WeatherType weatherType = CuriosityWeather.WeatherType;

            if (!weatherType.Equals(lastWeather))
            {
                Logger.Debug($"Weather Sync: {weatherType} : Region: {subRegion}");

                await BaseScript.Delay(15000);

                ClearOverrideWeather();
                ClearWeatherTypePersist();
                SetWeatherTypeOverTime($"{weatherType}", 30f);
                SetWeatherTypePersist($"{weatherType}");
                SetWeatherTypeNow($"{weatherType}");
                SetWeatherTypeNowPersist($"{weatherType}");

                lastWeather = weatherType;

                Instance.ExportDictionary["curiosity-ui"].SetWeather((int)CuriosityWeather.WeatherType, (int)CuriosityWeather.Season);
            }
        }

        [TickHandler]
        private async Task OnWorldTimeSyncTick()
        {
            await BaseScript.Delay(0);

            //int interior = GetInteriorFromEntity(Game.PlayerPed.Handle);

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
        private async Task OnSuppressVehicles()
        {
            if (DateTime.Now.Subtract(lastRunVehicleSuppression).TotalSeconds >= 10)
            {
                vehiclesToSuppress.ForEach(veh =>
                {
                    SetVehicleModelIsSuppressed((uint)veh, true);
                });

                lastRunVehicleSuppression = DateTime.Now;
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task OnWorldPlayerNamesSync()
        {
            foreach (Player player in PluginManager.Instance.PlayerList)
            {
                if (Game.Player == player) continue;

                Blip blip;

                if (player.Character.AttachedBlip == null)
                {
                    blip = player.Character.AttachBlip();
                    blip.Sprite = BlipSprite.Standard;
                    blip.Scale = 0.85f;

                    SetBlipCategory(blip.Handle, 7);
                    SetBlipPriority(blip.Handle, 11);
                    ShowHeadingIndicatorOnBlip(blip.Handle, true);
                    SetBlipNameToPlayerName(blip.Handle, player.Handle);
                }
                else
                {
                    blip = player.Character.AttachedBlip;

                    if (player.Character.IsDead)
                    {
                        blip.Sprite = BlipSprite.Dead;
                        ShowHeadingIndicatorOnBlip(blip.Handle, false);;
                    }

                    if (Game.IsPaused)
                    {
                        blip.Alpha = 255;
                    }
                    else
                    {
                        Vector3 charPos = player.Character.Position;
                        Vector3 playerPos = Game.PlayerPed.Position;

                        double distance = (Math.Floor(Math.Abs(Math.Sqrt(
                            (charPos.X - playerPos.X) *
                            (charPos.X - playerPos.X) +
                            (charPos.Y - playerPos.Y) *
                            (charPos.Y - playerPos.Y))) / -1)) + 900;

                        if (distance < 0)
                            distance = 0;

                        if (distance > 255)
                            distance = 255;

                        if (player.Character.IsVisible)
                        {
                            blip.Alpha = (int)distance;
                        }
                        else
                        {
                            blip.Alpha = 0;
                        }
                    }
                }

                if (player.Character.IsVisible)
                {
                    string prefix = NetworkIsPlayerTalking(player.Handle) ? "~b~" : "~w~";
                    ScreenInterface.Draw3DText(player.Character.Position, $"[{player.ServerId}] {prefix}{player.Name}", 30, 50);
                }
            }
        }
    }
}
