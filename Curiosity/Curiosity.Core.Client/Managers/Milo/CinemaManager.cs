using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;


namespace Curiosity.Core.Client.Managers.Milo
{
    public class CinemaManager : Manager<CinemaManager>
    {
        // Movie Theatre 	new Vector3(-1427.299, -245.1012, 16.8039);

        Position posEnter = new Position(-1423.533f, -214.2786f, 46.50037f, 357.6964f);
        Position posExit = new Position(-1437.539f, -245.3138f, 16.80255f, 279.8171f);
        Vector3 scale = new Vector3(1.5f, 1.5f, 0.5f);

        NativeUI.Marker markerEnter;
        NativeUI.Marker markerExit;

        public async override void Begin()
        {
            Logger.Info($"Started Cinema Manager");

            await Session.Loading();

            markerEnter = new NativeUI.Marker(MarkerType.VerticalCylinder, posEnter.AsVector(true), scale, 10f, System.Drawing.Color.FromArgb(255, 135, 206, 235));
            markerExit = new NativeUI.Marker(MarkerType.VerticalCylinder, posExit.AsVector(true), scale, 10f, System.Drawing.Color.FromArgb(255, 135, 206, 235));

            NativeUI.MarkersHandler.AddMarker(markerEnter);
            NativeUI.MarkersHandler.AddMarker(markerExit);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnCinemaTeleportTick()
        {
            string message = $"Cinema.";
            Vector3 notificationPosition = Vector3.Zero;

            if (markerEnter.IsInRange)
            {
                message = $"Enter {message}";
                notificationPosition = markerEnter.Position;

                if (markerEnter.IsInMarker)
                    NativeUI.Notifications.ShowHelpNotification($"Press ~INPUT_CONTEXT~ to enter the {message}");

                if (markerEnter.IsInMarker && Game.IsControlPressed(0, Control.Context))
                {
                    MovePlayer(true);
                }
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

        private async Task MovePlayer(bool enterCinema = false)
        {
            await Cache.PlayerPed.FadeOut();
            await ScreenInterface.FadeOut(1000);

            WorldManager worldManager = WorldManager.GetModule();

            Cache.PlayerPed.IsCollisionEnabled = false;

            Position pos = enterCinema ? posExit : posEnter;

            if (enterCinema)
            {
                Instance.DiscordRichPresence.Status = $"Watching movies...";
                worldManager.LockAndSetTime(12, 1);
                worldManager.LockAndSetWeather(WeatherType.EXTRASUNNY);
            }
            else
            {
                Instance.DiscordRichPresence.Status = $"Roaming Los Santos...";
                worldManager.UnlockTime();
                worldManager.UnlockAndUpdateWeather();
            }
            Instance.DiscordRichPresence.Commit();

            PlayerOptionsManager.GetModule().DisableWeapons(enterCinema);

            Dictionary<string, BlipData> blips = BlipManager.GetModule().AllBlips;
            foreach (KeyValuePair<string, BlipData> kvp in blips)
            {
                BlipData blip = kvp.Value;
                foreach (Blip b in blip.Blips)
                {
                    SetBlipHiddenOnLegend(b.Handle, enterCinema);
                    SetBlipAlpha(b.Handle, enterCinema ? 0 : 255);
                }
            }

            WorldManager.GetModule().UpdateWeather(true);

            Cache.PlayerPed.IsCollisionEnabled = true;

            SetPedCoordsKeepVehicle(Cache.PlayerPed.Handle, pos.X, pos.Y, pos.Z);

            await BaseScript.Delay(1500);
            await ScreenInterface.FadeIn(1000);
            await Cache.PlayerPed.FadeIn();
        }
    }
}
