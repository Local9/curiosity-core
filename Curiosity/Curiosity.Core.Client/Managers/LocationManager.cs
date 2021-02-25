using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Interface.Menus;
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

        private List<Location> Locations = new List<Location>();
        private List<Vector3> HospitalSpawns = new List<Vector3>();

        string currentJob = "unemployed";
        bool markerCooldown = false;

        public override void Begin()
        {
            LocationManagerInstance = this;

            Instance.EventRegistry[LegacyEvents.Client.CuriosityJob] += new Action<bool, bool, string>(OnJobDutyEvent);

            OnGetLocations();
        }

        private void OnJobDutyEvent(bool active, bool onDuty, string job)
        {
            if (job == currentJob) return;

            currentJob = job;
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

                Locations = await EventSystem.Request<List<Location>>("config:locations");

                foreach (Location location in Locations)
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

                    if (location.Name == "Hospital")
                    {
                        foreach(Position pos in location.Positions)
                        {
                            HospitalSpawns.Add(pos.AsVector());
                        }
                    }

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
                            markerData.JobRequirement = m.JobRequirement;

                            MarkersAll.Add(markerData);
                        });
                    });
                }

                InteractionMenu.MenuInstance.UpdateGpsMenuItem(true);
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
            MarkersClose = MarkersAll.ToList().Select(m => m).Where(m => 
                Cache.PlayerPed.IsInRangeOf(m.Position, m.DrawThreshold)
                && (m.JobRequirement == currentJob || string.IsNullOrEmpty(m.JobRequirement))
            ).ToList();
        }

        public MarkerData GetActiveMarker()
        {
            try
            {
                MarkerData closestMarker = closestMarker = MarkersClose.Where(w => Cache.PlayerPed.IsInRangeOf(w.Position, w.ContextAoe)).FirstOrDefault();

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
                Logger.Debug(activeMarker.JobRequirement);

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

        public bool IsNearLocation(Vector3 position, string eventName, float distance = 0f)
        {
            foreach (Location location in Locations)
            {
                if (location.Markers.Count == 0)
                    continue;

                foreach (Marker marker in location.Markers)
                {
                    if (marker.Event == eventName)
                    {
                        foreach (Position pos in marker.Positions)
                        {
                            Vector3 posV = pos.AsVector();
                            float dist = Vector3.Distance(position, posV);
                            float distanceToCheck = (distance > 0f) ? distance : marker.ContextAoe;
                            bool distanceValid = dist <= distanceToCheck;

                            Logger.Debug($"Position {posV} Close: {distanceValid}");

                            if (distanceValid)
                            {
                                return true;
                            }
                        };
                    }
                }
            }

            return false;
        }

        internal Vector3 NearestHospital()
        {
            Cache.UpdatePedId();
            return FindClosestPoint(Cache.PlayerPed.Position, HospitalSpawns);
        }

        public Vector3 FindClosestPoint(Vector3 startingPoint, IEnumerable<Vector3> points)
        {
            if (points.Count() == 0) return Vector3.Zero;

            return points.OrderBy(x => Vector3.Distance(startingPoint, x)).First();
        }
    }
}
