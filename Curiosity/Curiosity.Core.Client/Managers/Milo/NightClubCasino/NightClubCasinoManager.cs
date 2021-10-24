using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Extensions.Native;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers.Milo.NightClubCasino
{
    public class NightClubCasinoManager : Manager<NightClubCasinoManager>
    {
        Position posEntrance = new Position(987.6438f, 79.82104f, 80.99055f, 86.42644f);
        Position posExit = new Position(1578.096f, 253.0886f, -46.0051f, 1.268f);
        Vector3 markerScale = new Vector3(1.5f, 1.5f, .5f);

        WorldManager _WorldManager = WorldManager.GetModule();

        Color markerColor = Color.FromArgb(255, 135, 206, 235);

        NUIMarker markerEnter;
        NUIMarker markerExit;
        bool isInNightClub = false;

        int interiorId => GetInteriorAtCoords(1550f, 250f, -48f);

        Dictionary<string, bool> InteriorEntitySets = new Dictionary<string, bool>()
        {
            { "int01_ba_bar_content", true },
            { "int01_ba_booze_01", true },
            { "int01_ba_booze_02", true },
            { "int01_ba_booze_03", true },
            { "light_rigs_off", false }, // Club rig but all the lights are off
            { "int01_ba_lightgrid_01", true },
            // Light Set 1
            { "dj_01_lights_01", false }, // Rain Drops
            { "dj_01_lights_02", false }, // Grid
            { "dj_01_lights_03", true }, // Panels
            { "dj_01_lights_04", false }, // LaserShow
            // Light Set 2
            { "dj_02_lights_01", false },
            { "dj_02_lights_02", false },
            { "dj_02_lights_03", false },
            { "dj_02_lights_04", false },
            // Light Set 3
            { "dj_03_lights_01", false },
            { "dj_03_lights_02", false },
            { "dj_03_lights_03", false },
            { "dj_03_lights_04", false },
            // Light Set 4
            { "dj_04_lights_01", false },
            { "dj_04_lights_02", false },
            { "dj_04_lights_03", false },
            { "dj_04_lights_04", false },
            // Decks
            { "int01_ba_dj01", true },
            { "int01_ba_dj02", false },
            { "int01_ba_dj03", false },
            { "int01_ba_dj04", false },
            { "int01_ba_dry_ice", true },
            { "int01_ba_equipment_setup", true },
            { "int01_ba_equipment_upgrade", true },
            // Cameras
            { "int01_ba_security_upgrade", true },
            // Screens near the DJ
            { "int01_ba_lights_screen", true },
            { "int01_ba_screen", true },
            // DJ DJ Layout
            { "int01_ba_style02_podium", true },
            // Special DJ Layouts
            { "int01_ba_dj_keinemusik", false },
            { "int01_ba_dj_palms_trax", false },
            { "int01_ba_dj_moodyman", false },
            // DJ Lighting true
            { "EntitySet_DJ_Lighting", true },
        };

        public override void Begin()
        {
            markerEnter = new NUIMarker(MarkerType.VerticalCylinder, posEntrance.AsVector(), markerScale, 5f, markerColor);
            markerEnter.TeleportPosition = posExit;
            markerEnter.PlaceOnGround = true;
            markerEnter.Data = new { enterNightClub = true };
            markerEnter.Add();

            markerExit = new NUIMarker(MarkerType.VerticalCylinder, posExit.AsVector(), markerScale, 3f, markerColor);
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

            if (Cache.PlayerPed.IsInVehicle())
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
                    NativeUI.Notifications.ShowHelpNotification($"Press ~INPUT_CONTEXT~ to {notificationMessage}.");

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
                Instance.DiscordRichPresence.Status = $"Partying in the Casino Nightclub";
            }

            Logger.Debug($"Teleport to: Out of Club");
            if (!enterNightClub)
            {
                ExitNightClub();
                Instance.DiscordRichPresence.Status = $"Roaming Los Santos...";
            }
            Instance.DiscordRichPresence.Commit();

            Cache.PlayerPed.Position = teleportPosition.AsVector();
            Cache.PlayerPed.Heading = teleportPosition.H;


            PlayerOptionsManager.GetModule().DisableWeapons(enterNightClub);

            await BaseScript.Delay(2000);

            await ScreenInterface.FadeIn(1000);
            Cache.PlayerPed.FadeIn();
        }
        public void EnterNightClub()
        {
            _WorldManager.LockAndSetTime(0, 1);
            _WorldManager.LockAndSetWeather(WeatherType.FOGGY);
            EnableInteriorSets();
        }

        public void ExitNightClub()
        {
            _WorldManager.UnlockTime();
            _WorldManager.UnlockAndUpdateWeather();
            DisableInteriorSets();
        }

        public void EnableInteriorSets()
        {
            List<string> lst = InteriorEntitySets.Where(x => x.Value).Select(y => y.Key).ToList();
            for (int i = 0; i < lst.Count; i++)
            {
                ActivateInteriorEntitySet(interiorId, lst[i]);
            }
        }

        public void DisableInteriorSets()
        {
            List<string> lst = InteriorEntitySets.Select(y => y.Key).ToList();
            for (int i = 0; i < lst.Count; i++)
            {
                DeactivateInteriorEntitySet(interiorId, lst[i]);
            }
        }
    }
}
