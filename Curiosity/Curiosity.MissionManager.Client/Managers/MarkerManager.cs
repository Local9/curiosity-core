using CitizenFX.Core;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Environment.Entities.Models;
using Curiosity.MissionManager.Client.Environment.Enums;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Managers
{
    public class MarkerManager : Manager<MarkerManager>
    {
        static float contextAoe = 1.5f; // How close you need to be to see instruction
        static internal List<Marker> MarkersAll = new List<Marker>();
        static internal List<Marker> MarkersClose = new List<Marker>();

        public override void Begin()
        {
            Logger.Info($"- [MarkerManager] Begin --------------------------");
        }

        [TickHandler]
        private async Task OnTickMarkerHandler()
        {
            try
            {
                MarkersClose.ForEach(m =>
                {
                    World.DrawMarker(m.Type, m.Position, m.Direction, m.Rotation, m.Scale, m.Color, false, false, true);
                    NativeWrappers.Draw3DText(m.Position.X, m.Position.Y, m.Position.Z + 1, m.Message, 50f, 10f);
                });
            }
            catch (Exception ex)
            {
                // qqq
            }
        }

        [TickHandler]
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
            MarkersClose = MarkersAll.ToList().Select(m => m).Where(m => m.Position.Distance(Game.PlayerPed.Position) < Math.Pow(m.DrawThreshold, 2)).ToList();
        }

        public static Marker GetActiveMarker(MarkerFilter markerFilter)
        {
            try
            {
                Marker closestMarker = closestMarker = MarkersClose.Where(w => w.Position.Distance(Game.PlayerPed.Position) < contextAoe).FirstOrDefault();

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
