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

        NativeUI.Marker markerEnter;
        NativeUI.Marker markerExit;

        public override void Begin()
        {
            Logger.Info($"Started Tuner Race Track Manager");

            markerEnter = new NativeUI.Marker(MarkerType.VerticalCylinder, enter.AsVector(), 30f, System.Drawing.Color.FromArgb(255, 135, 206, 235));
            markerExit = new NativeUI.Marker(MarkerType.VerticalCylinder, exit.AsVector(), 30f, System.Drawing.Color.FromArgb(255, 135, 206, 235));

            NativeUI.MarkersHandler.AddMarker(markerEnter);
            NativeUI.MarkersHandler.AddMarker(markerExit);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnTurnerTeleporterTick()
        {
            if (markerEnter.IsInRange)
            {
                NativeUI.Notifications.ShowFloatingHelpNotification("Enter Tuner Track", markerEnter.Position);
            }

            if (markerExit.IsInRange)
            {
                NativeUI.Notifications.ShowFloatingHelpNotification("Exit Tuner Track", markerExit.Position);
            }
        }
    }
}
