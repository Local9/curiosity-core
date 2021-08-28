using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Extensions.Native;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.Milo.NightClub
{
    public class NightClubManager : Manager<NightClubManager>
    {
        /// <summary>
        ///  https://github.com/TomGrobbe/NightClubs
        ///  TOM YOU'RE A FUCKING LEGEND!
        /// </summary>
        /// 
        private const string NIGHTCLUB_IPL = "ba_dlc_int_01_ba";
        private const string NIGHTCLUB_MILO = "ba_int_placement_ba_interior_0_dlc_int_01_ba_milo_";

        Position posEntrance = new Position(194.6124f, -3167.278f, 5.790269f, 86.42644f);
        Position posExit = new Position(-1569.665f, -3016.758f, -74.40615f, 1.268f);
        Vector3 markerScale = new Vector3(1.5f, 1.5f, .5f);

        WorldManager _WorldManager = WorldManager.GetModule();
        EffectsManager _EffectsManager = EffectsManager.GetModule();

        Color markerColor = Color.FromArgb(255, 135, 206, 235);

        NUIMarker markerEnter;
        NUIMarker markerExit;
        bool isInNightClub = false;

        private List<string> emitters = new List<string>()
        {
            "SE_BA_DLC_INT_01_BOGS",
            "SE_BA_DLC_INT_01_ENTRY_HALL",
            "SE_BA_DLC_INT_01_ENTRY_STAIRS",
            "SE_BA_DLC_INT_01_GARAGE",
            "SE_BA_DLC_INT_01_MAIN_AREA_2",
            "SE_BA_DLC_INT_01_MAIN_AREA",
            "SE_BA_DLC_INT_01_OFFICE",
            "SE_BA_DLC_INT_01_REAR_L_CORRIDOR",
        };

        private List<string> djRadioStations = new List<string>()
        {
            "RADIO_22_DLC_BATTLE_MIX1_CLUB",
            "RADIO_23_DLC_BATTLE_MIX2_CLUB",
            "RADIO_24_DLC_BATTLE_MIX3_CLUB",
            "RADIO_25_DLC_BATTLE_MIX4_CLUB",
        };

        private static readonly List<string> djVideos = new List<string>() { "SOL", "TOU", "DIX", "TBM" }; // Solomun, Tale of Us, Dixon, The Black Madonna
        private static readonly List<string> screenTypes = new List<string>() { "LSER", "LED", "GEO", "RIB", "NL" }; // lasers, led tubes, neon tubes, ribbon bands, no light rig
        private static readonly List<string> clubs = new List<string>() { "GALAXY", "LOS", "OMEGA", "TECH", "GEFANGNIS", "MAIS", "FUNHOUSE", "PALACE", "PARADISE" };

        public override void Begin()
        {
            markerEnter = new NUIMarker(MarkerType.VerticalCylinder, posEntrance.AsVector(), markerScale, 10f, markerColor);
            markerEnter.TeleportPosition = posExit;
            markerEnter.PlaceOnGround = true;
            markerEnter.Data = new { enterNightClub = true };
            markerEnter.Add();

            markerExit = new NUIMarker(MarkerType.VerticalCylinder, posExit.AsVector(), markerScale, 10f, markerColor);
            markerExit.TeleportPosition = posEntrance;
            markerExit.PlaceOnGround = true;
            markerExit.Data = new { enterNightClub = false };
            markerExit.Add();
        }

        [TickHandler(SessionWait = true)]
        private async Task OnTeleportToNightclub()
        {
            string notificationMessage = string.Empty;
            NUIMarker activeMarker = null;

            if (Cache.PlayerPed.IsDead)
            {
                await BaseScript.Delay(1000);
                return;
            }

            if (Cache.PlayerPed.Opacity == 0)
            {
                await BaseScript.Delay(1000);
                return;
            }

            if (markerEnter.IsInRange)
            {
                notificationMessage = "Enter Nightclub";
                activeMarker = markerEnter;
            }

            if (markerExit.IsInRange)
            {
                notificationMessage = "Exit Nightclub";
                activeMarker = markerExit;
            }

            if (activeMarker is null)
            {
                await BaseScript.Delay(1000);
                return;
            }

            while (activeMarker.IsInRange)
            {
                if (activeMarker.IsInMarker)
                {
                    NativeUI.Notifications.ShowHelpNotification($"{notificationMessage}, press ~INPUT_CONTEXT~");

                    if (Game.IsControlPressed(0, Control.Context) && !Cache.PlayerPed.IsDead)
                    {
                        MovePlayer(activeMarker.TeleportPosition, activeMarker.Data.enterNightClub);
                        await BaseScript.Delay(10000);
                    }
                }

                if (!activeMarker.IsInMarker)
                {
                    Vector3 notificationMarker = activeMarker.Position;
                    notificationMarker.Z = notificationMarker.Z + 1f;
                    NativeUI.Notifications.ShowFloatingHelpNotification(notificationMessage, notificationMarker);
                }

                await BaseScript.Delay(0);
            }
        }

        private async Task MovePlayer(Position teleportPosition, dynamic enterNightClub)
        {
            Logger.Debug($"Teleport Into Club: {enterNightClub} Position: {teleportPosition}");

            Cache.PlayerPed.FadeOut();
            await ScreenInterface.FadeOut(1000);

            Logger.Debug($"Screen Faded Out");

            if (teleportPosition is null)
            {
                Logger.Debug($"Position is null");
                await ScreenInterface.FadeIn(1000);
                await Cache.PlayerPed.FadeIn();
            }

            Logger.Debug($"Teleport to: Club");
            if (enterNightClub)
            {
                EnterNightClub();
            }

            Logger.Debug($"Teleport to: Out of Club");
            if (!enterNightClub)
            {
                ExitNightClub();
            }

            Cache.PlayerPed.Position = teleportPosition.AsVector();
            Cache.PlayerPed.Heading = teleportPosition.H;

            await BaseScript.Delay(2000);

            await ScreenInterface.FadeIn(1000);
            Cache.PlayerPed.FadeIn();
        }

        public void EnterNightClub()
        {
            _EffectsManager.Init();
            _WorldManager.LockAndSetTime(0, 1);
            _WorldManager.LockAndSetWeather(WeatherType.FOGGY);

            if (!IsIplActive(NIGHTCLUB_IPL))
            {
                RequestIpl(NIGHTCLUB_IPL);
            }

            if (!IsIplActive(NIGHTCLUB_MILO))
            {
                RequestIpl(NIGHTCLUB_MILO);
            }

            InteriorChanged(true);
        }

        public void ExitNightClub()
        {
            _EffectsManager.Dispose();
            _WorldManager.UnlockTime();
            _WorldManager.UnlockAndUpdateWeather();

            if (IsIplActive(NIGHTCLUB_IPL))
            {
                RemoveIpl(NIGHTCLUB_IPL);
            }

            if (IsIplActive(NIGHTCLUB_MILO))
            {
                RemoveIpl(NIGHTCLUB_MILO);
            }

            InteriorChanged(false);
        }

        private void InteriorChanged(bool enteredInterior)
        {
            isInNightClub = enteredInterior;

            if (enteredInterior)
            {
                //CreateModelHide(-1605.643f, -3012.672f, -77.79608f, 1f, (uint)GetHashKey("ba_prop_club_screens_02"), true);
                CreateModelHide(-1605.643f, -3012.672f, -77.79608f, 1f, (uint)GetHashKey("ba_prop_club_screens_01"), true);

                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_clubname_04");          // Name (galaxy)
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_Style03");              // Style (traditional)
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_style03_podium");       // Podium (traditional)
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_equipment_setup");      // Speaker setup (upgraded part 1)
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_equipment_upgrade");    // Speaker setup (upgraded part 2)
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_security_upgrade");     // Security (cameras and shit)
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_dj04");                 // DJ Booth (variant 3)
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "DJ_03_Lights_01");               // Droplets neon lights (variant 4)
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "DJ_02_Lights_02");               // Neons neon lights (variant 4)
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "DJ_03_Lights_03");               // Band neon lights (variant 4)
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "DJ_04_Lights_04");               // Lasers neon lights (variant 4)
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_bar_content");          // Bar
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_booze_01");             // Booze 1
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_booze_02");             // Booze 2
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_booze_03");             // Booze 3
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_trophy03");             // Trophy variant 3
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_deliverytruck");        // Delivery truck in the garage
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_dry_ice");              // Dry ice machines (no smoke effects)
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_lightgrid_01");         // Roof lights
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_trad_lights");          // Floor/wall lights
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_trophy04");             // Chest in VIP lounge
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_trophy04");             // Chest in VIP lounge
                DisableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_dj01");                 // Dixon DJ posters
                DisableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_dj02");                // Remove other posters
                EnableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_dj03");                // Remove other posters
                DisableInteriorProp(EffectsManager.NIGHTCLUB_INTERIORID, "Int01_ba_dj04");                // Remove other posters

                //if (!IsAudioSceneActive("DLC_Ba_NightClub_Scene"))
                //{
                //    StartAudioScene("DLC_Ba_NightClub_Scene");
                //}

                foreach (string s in emitters)
                {
                    SetEmitterRadioStation(s, djRadioStations[2]);
                }

                EffectsManager.GetModule().AddSmokeParticles();

                RefreshInterior(EffectsManager.NIGHTCLUB_INTERIORID);

                StartTv();
            }
            else
            {
                if (IsAudioSceneActive("DLC_Ba_NightClub_Scene"))
                {
                    StopAudioScene("DLC_Ba_NightClub_Scene");
                }

                EffectsManager.GetModule().RemoveSmokeParticles();
            }

            PlayerOptionsManager.GetModule().DisableWeapons(isInNightClub);
        }

        public async void StartTv()
        {
            await BaseScript.Delay(2000);
            string renderTarget = "club_projector";
            int channel = 2;
            string dj = djVideos[1]; // dixon
            string screenType = screenTypes[1]; // neon tubes
            string clubName = clubs[3]; // galaxy

            int renderTargetHandle = 0;

            SetTvChannel(-1); // disable tv.

            async Task DoSetup()
            {
                LoadTvChannelSequence(channel, $"PL_{dj}_{screenType}_{clubName}", false);
                await BaseScript.Delay(500);

                ReleaseNamedRendertarget(renderTarget);

                await BaseScript.Delay(500);

                if (!IsNamedRendertargetRegistered(renderTarget))
                {
                    RegisterNamedRendertarget(renderTarget, false);
                }

                while (!IsNamedRendertargetRegistered(renderTarget))
                {
                    await BaseScript.Delay(0);
                }


                if (!IsNamedRendertargetLinked((uint)GetHashKey("ba_prop_club_screens_01")))
                {
                    LinkNamedRendertarget((uint)GetHashKey("ba_prop_club_screens_01"));
                }

                while (!IsNamedRendertargetLinked((uint)GetHashKey("ba_prop_club_screens_01")))
                {
                    await BaseScript.Delay(0);
                }

                renderTargetHandle = GetNamedRendertargetRenderId(renderTarget);

                SetTvChannel(channel);
                SetTvVolume(100f);
                SetTvAudioFrontend(false);
            }

            await DoSetup();

            int timer = GetGameTimer() + 1000;
            while (isInNightClub)
            {
                DisablePlayerFiring(PlayerId(), true);

                if (GetGameTimer() > timer)
                {
                    timer = GetGameTimer() + 1000;
                    SetTvChannel(channel);
                }
                await BaseScript.Delay(0);

                SetTextRenderId(renderTargetHandle);
                SetScriptGfxDrawOrder(4);
                SetScriptGfxDrawBehindPausemenu(true);

                DrawTvChannel(0.5f, 0.5f, 1f, 1f, 0f, 255, 255, 255, 255);

                SetScriptGfxDrawBehindPausemenu(false);
                SetTextRenderId(GetDefaultScriptRendertargetRenderId());
            }

            SetTvChannel(-1); // disable tv.
            ReleaseNamedRendertarget(renderTarget);
        }
    }
}
