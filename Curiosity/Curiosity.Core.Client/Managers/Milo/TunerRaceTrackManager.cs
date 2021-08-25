﻿using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Extensions;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers.Milo
{
    public class TunerRaceTrackManager : Manager<TunerRaceTrackManager>
    {
        Position posEnter = new Position(783.46f, -1868.02f, 28.27f, 255.0f);
        Position posExit = new Position(-2127.311f, 1106.741f, -28.36684f, 268.4973f);
        Vector3 scale = new Vector3(5f, 5f, 0.5f);

        NativeUI.Marker markerEnter;
        NativeUI.Marker markerExit;

        public async override void Begin()
        {
            Logger.Info($"Started Tuner Race Track Manager");

            await Session.Loading();

            markerEnter = new NativeUI.Marker(MarkerType.VerticalCylinder, posEnter.AsVector(true), scale, 10f, System.Drawing.Color.FromArgb(255, 135, 206, 235));
            markerExit = new NativeUI.Marker(MarkerType.VerticalCylinder, posExit.AsVector(true), scale, 10f, System.Drawing.Color.FromArgb(255, 135, 206, 235));

            NativeUI.MarkersHandler.AddMarker(markerEnter);
            NativeUI.MarkersHandler.AddMarker(markerExit);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnTurnerTeleporterTick()
        {
            string message = $"Tuner Track.";
            Vector3 notificationPosition = Vector3.Zero;

            bool isDriving = false;

            if (markerEnter.IsInRange || markerExit.IsInRange)
            {
                isDriving = Cache.PlayerPed.IsInVehicle() && Cache.PlayerPed.CurrentVehicle.Driver == Cache.PlayerPed;
            }

            if (markerEnter.IsInRange && isDriving)
            {
                message = $"Enter {message}";
                notificationPosition = markerEnter.Position;

                if (markerEnter.IsInMarker && isDriving)
                    NativeUI.Notifications.ShowHelpNotification($"Press ~INPUT_CONTEXT~ to enter the {message}");

                if (markerEnter.IsInMarker && Game.IsControlPressed(0, Control.Context))
                {
                    MovePlayer(true);
                }
            }
            else if (markerEnter.IsInRange)
            {
                notificationPosition = markerEnter.Position;
                message = "You must be driving a vehicle to enter the track.";
            }

            if (markerExit.IsInRange)
            {
                message = $"Exit {message}";
                notificationPosition = markerExit.Position;

                if (markerExit.IsInMarker)
                    NativeUI.Notifications.ShowHelpNotification($"Press ~INPUT_CONTEXT~ to left the {message}");

                if (markerExit.IsInMarker && Game.IsControlPressed(0, Control.Context))
                {
                    MovePlayer();
                }
            }

            if (notificationPosition != Vector3.Zero)
            {
                notificationPosition.Z = notificationPosition.Z + 1f;
                NativeUI.Notifications.ShowFloatingHelpNotification(message, notificationPosition);
            }
        }

        private async Task MovePlayer(bool enter = false)
        {
            await Cache.PlayerPed.FadeOut();
            await FadeScreenOut();

            Cache.PlayerPed.IsCollisionEnabled = false;
            Cache.PlayerPed.CurrentVehicle.IsCollisionEnabled = false;

            Position pos = enter ? posExit : posEnter;

            int interiorId = GetInteriorAtCoords(-2000.0f, 1113.211f, -25.36243f);

            if (enter)
            {
                RequestIpl("tr_tuner_meetup");
                RequestIpl("tr_tuner_race_line");

                ActivateInteriorEntitySet(interiorId, "entity_set_meet_lights");
                ActivateInteriorEntitySet(interiorId, "entity_set_meet_lights_cheap");
                ActivateInteriorEntitySet(interiorId, "entity_set_test_lights");
                ActivateInteriorEntitySet(interiorId, "entity_set_test_lights_cheap");
                ActivateInteriorEntitySet(interiorId, "entity_set_time_trial");
            }
            else
            {
                RemoveIpl("tr_tuner_meetup");
                RemoveIpl("tr_tuner_race_line");

                DeactivateInteriorEntitySet(interiorId, "entity_set_meet_lights");
                DeactivateInteriorEntitySet(interiorId, "entity_set_meet_lights_cheap");
                DeactivateInteriorEntitySet(interiorId, "entity_set_test_lights");
                DeactivateInteriorEntitySet(interiorId, "entity_set_test_lights_cheap");
                DeactivateInteriorEntitySet(interiorId, "entity_set_time_trial");
            }

            if (IsValidInterior(interiorId))
                RefreshInterior(interiorId);

            SetPedCoordsKeepVehicle(Cache.PlayerPed.Handle, pos.X, pos.Y, pos.Z);
            Cache.PlayerPed.CurrentVehicle.Heading = pos.H;

            await BaseScript.Delay(1500);
            await FadeScreenIn();
            await Cache.PlayerPed.FadeIn();

            while(Cache.PlayerPed.IsInRangeOf(pos.AsVector(), 10f))
            {
                Cache.PlayerPed.IsCollisionEnabled = false;
                Cache.PlayerPed.CurrentVehicle.IsCollisionEnabled = false;
            }

            Cache.PlayerPed.IsCollisionEnabled = true;
            Cache.PlayerPed.CurrentVehicle.IsCollisionEnabled = true;
        }

        private async Task FadeScreenOut()
        {
            Screen.Fading.FadeOut(1000);

            while (Screen.Fading.IsFadingOut)
            {
                await BaseScript.Delay(100);
            }

            await BaseScript.Delay(1000);
        }

        private async Task FadeScreenIn()
        {
            Screen.Fading.FadeIn(1000);

            while (Screen.Fading.IsFadingIn)
            {
                await BaseScript.Delay(100);
            }
        }
    }
}