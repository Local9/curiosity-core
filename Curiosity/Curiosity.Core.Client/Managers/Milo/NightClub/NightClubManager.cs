using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Core.Client.Extensions.Native;
using Curiosity.Core.Client.Extensions;
using System.Drawing;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Diagnostics;

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

        Position posEntrance = new Position(194.6124f, -3167.278f, 5.790269f, 86.42644f);
        Position posExit = new Position(-1569.665f, -3016.758f, -74.40615f, 86.42644f);
        Vector3 markerScale = new Vector3(1f, 1f, .5f);

        WorldManager WorldManager = WorldManager.GetModule();
        EffectsManager EffectsManager = EffectsManager.GetModule();

        Color markerColor = Color.FromArgb(255, 135, 206, 235);

        NUIMarker markerEnter;
        NUIMarker markerExit;

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
            markerEnter.Data = new { enterNightClub = false };
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

            Logger.Debug($"Teleport to: Los Santos");
            if (enterNightClub)
            {
                EnterNightClub();
            }

            Logger.Debug($"Teleport to: Cayo Perico");
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
            EffectsManager.Init();
            WorldManager.LockAndSetTime(0, 1);
            WorldManager.LockAndSetWeather(WeatherType.FOGGY);

            if (!IsIplActive(NIGHTCLUB_IPL))
            {
                RequestIpl(NIGHTCLUB_IPL);
            }
        }

        public void ExitNightClub()
        {
            EffectsManager.Dispose();
            WorldManager.UnlockTime();
            WorldManager.UnlockAndUpdateWeather();

            if (IsIplActive(NIGHTCLUB_IPL))
            {
                RemoveIpl(NIGHTCLUB_IPL);
            }
        }
    }
}
