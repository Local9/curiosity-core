using CitizenFX.Core;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core.UI;

namespace Curiosity.Missions.Client.net.Scripts
{
    class MarkerHandler
    {
        static Client client = Client.GetInstance();
        // For e.g. cinematic mode
        static public bool HideAllMarkers = false;
        static float contextAoe = 2f; // How close you need to be to see instruction
        // Any other class can add or remove markers from this dictionary (by ID preferrably)
        static internal List<Marker> MarkersAll = new List<Marker>();
        static internal List<Marker> MarkersClose = new List<Marker>();

        static public void Init()
        {
            client.RegisterTickHandler(OnTickMarkerHandler);
            client.RegisterTickHandler(PeriodicUpdate);
        }

        static public void Dispose()
        {
            client.DeregisterTickHandler(OnTickMarkerHandler);
            client.DeregisterTickHandler(PeriodicUpdate);
            MarkersAll.Clear();
            MarkersClose.Clear();
        }

        static public Task OnTickMarkerHandler()
        {
            try
            {
                if (!HideAllMarkers)
                {
                    MarkersClose.ForEach(m => {
                        World.DrawMarker(m.Type, m.Position, m.Direction, m.Rotation, m.Scale, m.Color, false, false, true);
                        NativeWrappers.Draw3DText(m.Position.X, m.Position.Y, m.Position.Z + 1, m.Message, 50f, 10f);
                    });
                }
            }
            catch (Exception ex)
            {
                // qqq
            }
            return Task.FromResult(0);
        }

        static public async Task PeriodicUpdate()
        {
            while (true)
            {
                await RefreshClose();
                await Client.Delay(1000);
            }
        }

        static public Task RefreshClose()
        {
            MarkersClose = MarkersAll.ToList().Select(m => m).Where(m => m.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(m.DrawThreshold, 2)).ToList();
            return Task.FromResult(0);
        }

        public static Marker GetActiveMarker()
        {
            try
            {
                Marker closestMarker = MarkersClose.Where(w => w.Position.DistanceToSquared(Game.PlayerPed.Position) < contextAoe).FirstOrDefault();
                if (closestMarker == null) return null;
                return closestMarker;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        static void RemoveMarker(Marker marker)
        {
            MarkersAll.Remove(marker);
        }
    }
}
