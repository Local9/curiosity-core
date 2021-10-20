using CitizenFX.Core;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Environment.Entities.Models;
using Curiosity.MissionManager.Client.Environment.Enums;
using Curiosity.MissionManager.Client.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Managers
{
    public class MarkerManager : Manager<MarkerManager>
    {
        static internal List<Marker> MarkersAll = new List<Marker>();
        static internal List<Marker> MarkersClose = new List<Marker>();

        public override void Begin()
        {
            Logger.Info($"- [MarkerManager] Begin --------------------------");
        }

        [TickHandler] // TODO: Maybe move this also into "Job Start"
        private async Task OnTickMarkerHandler()
        {
            try
            {
                MarkersClose.ForEach(m =>
                {
                    World.DrawMarker(m.Type, m.Position, m.Direction, m.Rotation, m.Scale, m.Color, false, false, true);
                    Vector3 pos = m.Position;
                    pos.Z = pos.Z + 1f;
                    NativeUI.Notifications.DrawText3D(m.Message, pos, System.Drawing.Color.FromArgb(255, 255, 255, 255));
                });
            }
            catch (Exception ex)
            {
                // qqq
            }
        }

        [TickHandler] // TODO: Maybe move this also into "Job Start"
        private async Task PeriodicUpdate()
        {
            while (true)
            {
                RefreshClose();
                await BaseScript.Delay(1000);
            }
        }

        void RefreshClose()
        {
            MarkersClose = MarkersAll.ToList().Select(m => m).Where(m => m.Position.Distance(Cache.PlayerPed.Position) < Math.Pow(m.DrawThreshold, 2)).ToList();
        }

        public static Marker GetActiveMarker(MarkerFilter markerFilter)
        {
            try
            {
                Marker closestMarker = closestMarker = MarkersClose.Where(w => w.Position.Distance(Cache.PlayerPed.Position) < w.ContextAoe).FirstOrDefault();

                if (markerFilter == MarkerFilter.Unknown)
                {
                    if (closestMarker == null) return null;
                    return closestMarker;
                }

                if (closestMarker == null) return null;

                if (closestMarker.MarkerFilter == markerFilter)
                {
                    return closestMarker;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void RemoveMarker(Marker marker)
        {
            MarkersAll.Remove(marker);
        }
    }
}
