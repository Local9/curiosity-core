using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.EventWrapperLegacy;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class LocationManager : Manager<LocationManager>
    {
        public static LocationManager LocationManagerInstance;

        internal List<MarkerData> MarkersAll = new List<MarkerData>();
        internal List<MarkerData> MarkersClose = new List<MarkerData>();

        string previousJob = "unemployed";
        bool markerCooldown = false;

        public override void Begin()
        {
            LocationManagerInstance = this;

            Instance.EventRegistry[LegacyEvents.Client.CuriosityJob] += new Action<bool, bool, string>(OnJobDutyEvent);

            OnGetLocations();
        }

        private void OnJobDutyEvent(bool active, bool onDuty, string job)
        {
            if (job == previousJob) return;
        }

        internal async Task OnGetLocations()
        {
            try
            {
                await Session.Loading();

                if (MarkersAll.Count > 0)
                {
                    BlipManager.ManagerInstance.RemoveAllBlips();
                    MarkersAll.Clear();
                    MarkersClose.Clear();
                }

                List<Location> locations = await EventSystem.Request<List<Location>>("config:locations");

                foreach (Location location in locations)
                {
                    BlipData blipData = new BlipData();
                    blipData.Positions = location.Positions;
                    blipData.Name = location.Name;
                    blipData.Sprite = location.Sprite;
                    blipData.Color = location.Color;
                    blipData.IsShortRange = location.IsShortRange;
                    blipData.Category = location.Category;
                    blipData.Scale = location.Scale;
                    blipData.Priority = location.Priority;

                    BlipManager.ManagerInstance.AddBlip(blipData);

                    location.Markers.ForEach(m =>
                    {
                        m.Positions.ForEach(p =>
                        {
                            MarkerData markerData = new MarkerData();
                            markerData.ColorArgb = System.Drawing.Color.FromArgb(m.Color.Alpha, m.Color.Red, m.Color.Green, m.Color.Blue);
                            markerData.Message = m.Message;
                            markerData.HelpText = m.HelpText;
                            markerData.Event = m.Event;
                            markerData.Position = p.AsVector();
                            markerData.MarkerId = m.MarkerId;
                            markerData.Scale = m.Scale;
                            markerData.VScale = 1.0f * m.Scale.AsVector();
                            markerData.DrawThreshold = m.DrawThreshold;
                            markerData.VRotation = m.Rotation.AsVector();
                            markerData.VDirection = m.Direction.AsVector();
                            markerData.ContextAoe = m.ContextAoe;

                            MarkersAll.Add(markerData);
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                string msg = $"OnGetLocations -> {ex.Message}";
                Logger.Error(msg);
                EventSystem.Send("user:log:exception", msg, ex.StackTrace);
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task PeriodicUpdate()
        {
            RefreshClose();
            await BaseScript.Delay(1000);
        }

        void RefreshClose()
        {
            MarkersClose = MarkersAll.ToList().Select(m => m).Where(m => Game.PlayerPed.IsInRangeOf(m.Position, m.DrawThreshold)).ToList();
        }

        public MarkerData GetActiveMarker()
        {
            try
            {
                MarkerData closestMarker = closestMarker = MarkersClose.Where(w => Game.PlayerPed.IsInRangeOf(w.Position, w.ContextAoe)).FirstOrDefault();

                if (closestMarker == null) return null;

                return closestMarker;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void RemoveMarker(string markerMessage)
        {
            List<MarkerData> markerCopy = new List<MarkerData>(MarkersAll);
            markerCopy.ForEach(m =>
            {
                if (m.Message == markerMessage)
                    MarkersAll.Remove(m);
            });
        }

        [TickHandler(SessionWait = true)]
        private async Task OnMarkerCreateTick()
        {
            MarkersClose.ForEach(m =>
            {
                float ground = 0f;

                if (API.GetGroundZFor_3dCoord_2(m.Position.X, m.Position.Y, m.Position.Z, ref ground, false))
                    m.Position.Z = ground;

                World.DrawMarker((MarkerType)m.MarkerId, m.Position, m.VDirection, m.VRotation, m.VScale, m.ColorArgb);

                ScreenInterface.Draw3DText(m.Position, m.Message, 50f, m.DrawThreshold, 2f);
            });
        }

        [TickHandler(SessionWait = true)]
        private async Task OnMarkerInteractionTick()
        {
            MarkerData activeMarker = GetActiveMarker();

            if (activeMarker == null)
            {
                await BaseScript.Delay(1500);
                return;
            }

            Screen.DisplayHelpTextThisFrame(activeMarker.HelpText);

            if (Game.IsControlJustPressed(0, (Control)activeMarker.Control))
            {
                if (markerCooldown)
                {
                    Notify.Alert("Cooldown Active, please wait...");
                    return;
                }

                OnMarkerCooldown();

                Logger.Debug($"Control for event '{activeMarker.Event}' pressed");

                if (activeMarker.IsServerEvent)
                {
                    if (activeMarker.IsLegacyEvent || activeMarker.IsLuaEvent)
                    {
                        BaseScript.TriggerServerEvent(activeMarker.Event, activeMarker.Position.X, activeMarker.Position.Y, activeMarker.Position.Z);
                    }
                    else
                    {
                        EventSystem.GetModule().Send(activeMarker.Event, activeMarker.Position.X, activeMarker.Position.Y, activeMarker.Position.Z);
                    }
                }
                else
                {
                    if (activeMarker.IsLegacyEvent || activeMarker.IsLuaEvent)
                    {
                        BaseScript.TriggerEvent(activeMarker.Event, activeMarker.Position.X, activeMarker.Position.Y, activeMarker.Position.Z);
                    }
                    else
                    {
                        EventSystem.GetModule().Send(activeMarker.Event, activeMarker.Position.X, activeMarker.Position.Y, activeMarker.Position.Z);
                    }
                }
            }
        }

        private async void OnMarkerCooldown()
        {
            markerCooldown = true;
            await BaseScript.Delay(5000);
            markerCooldown = false;
        }
    }
}
