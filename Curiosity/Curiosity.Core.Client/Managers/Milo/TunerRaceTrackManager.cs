using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

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
                    NativeUI.Notifications.ShowHelpNotification($"Press ~INPUT_CONTEXT~ to leave the {message}");

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

        private async Task MovePlayer(bool enterTrack = false)
        {
            await Cache.PlayerPed.FadeOut();
            await ScreenInterface.FadeOut(1000);

            Cache.PlayerPed.IsCollisionEnabled = false;
            Cache.PlayerPed.CurrentVehicle.IsCollisionEnabled = false;

            Position pos = enterTrack ? posExit : posEnter;

            int interiorId = GetInteriorAtCoords(-2000.0f, 1113.211f, -25.36243f);

            if (enterTrack)
            {
                Instance.DiscordRichPresence.Status = $"Drifting on the Tuner Track...";

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
                Instance.DiscordRichPresence.Status = $"Roaming Los Santos...";

                RemoveIpl("tr_tuner_meetup");
                RemoveIpl("tr_tuner_race_line");

                DeactivateInteriorEntitySet(interiorId, "entity_set_meet_lights");
                DeactivateInteriorEntitySet(interiorId, "entity_set_meet_lights_cheap");
                DeactivateInteriorEntitySet(interiorId, "entity_set_test_lights");
                DeactivateInteriorEntitySet(interiorId, "entity_set_test_lights_cheap");
                DeactivateInteriorEntitySet(interiorId, "entity_set_time_trial");
            }
            Instance.DiscordRichPresence.Commit();

            PlayerOptionsManager.GetModule().DisableWeapons(enterTrack);

            Dictionary<string, BlipData> blips = BlipManager.GetModule().AllBlips;
            foreach (KeyValuePair<string, BlipData> kvp in blips)
            {
                BlipData blip = kvp.Value;
                foreach (Blip b in blip.Blips)
                {
                    SetBlipHiddenOnLegend(b.Handle, enterTrack);
                }
            }

            if (IsValidInterior(interiorId))
                RefreshInterior(interiorId);

            WorldManager.GetModule().UpdateWeather(true);

            Cache.PlayerPed.IsCollisionEnabled = true;
            Cache.PlayerPed.CurrentVehicle.IsCollisionEnabled = true;

            SetPedCoordsKeepVehicle(Cache.PlayerPed.Handle, pos.X, pos.Y, pos.Z);
            Cache.PlayerPed.CurrentVehicle.Heading = pos.H;

            await BaseScript.Delay(1500);
            await ScreenInterface.FadeIn(1000);
            await Cache.PlayerPed.FadeIn();

            //while(Cache.PlayerPed.IsInRangeOf(pos.AsVector(), 10f))
            //{
            //    Cache.PlayerPed.IsCollisionEnabled = false;
            //    Cache.PlayerPed.CurrentVehicle.IsCollisionEnabled = false;
            //}
        }
    }
}
