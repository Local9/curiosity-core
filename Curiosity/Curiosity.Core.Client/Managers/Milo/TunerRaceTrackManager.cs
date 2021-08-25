using CitizenFX.Core;
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
        Position enter = new Position(-2152.99f, 1105.97f, -24.76f, 90.0f);
        Position exit = new Position(-2134.82f, 1106.42f, -27.27f, 270.0f);
        Vector3 scale = new Vector3(5f, 5f, 0.5f);

        NativeUI.Marker markerEnter;
        NativeUI.Marker markerExit;

        public async override void Begin()
        {
            Logger.Info($"Started Tuner Race Track Manager");

            await Session.Loading();

            markerEnter = new NativeUI.Marker(MarkerType.VerticalCylinder, enter.AsVector(true), scale, 10f, System.Drawing.Color.FromArgb(255, 135, 206, 235));
            markerExit = new NativeUI.Marker(MarkerType.VerticalCylinder, exit.AsVector(true), scale, 10f, System.Drawing.Color.FromArgb(255, 135, 206, 235));

            NativeUI.MarkersHandler.AddMarker(markerEnter);
            NativeUI.MarkersHandler.AddMarker(markerExit);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnTurnerTeleporterTick()
        {
            string message = $"Tuner Track";
            Vector3 notificationPosition = Vector3.Zero;

            if (markerEnter.IsInRange)
            {
                message = $"~INPUT_CONTEXT~ to enter {message}";
                notificationPosition = markerEnter.Position;
            }

            if (markerExit.IsInRange)
            {
                message = $"~INPUT_CONTEXT~to exit {message}";
                notificationPosition = markerEnter.Position;
            }

            if (notificationPosition != Vector3.Zero)
            {
                notificationPosition.Z = notificationPosition.Z + 1f;
                NativeUI.Notifications.ShowFloatingHelpNotification(message, notificationPosition);
            }
        }
    }
}
