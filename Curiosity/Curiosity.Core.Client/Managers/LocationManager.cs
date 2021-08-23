using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Interface.Menus;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.EventWrapperLegacy;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class LocationManager : Manager<LocationManager>
    {
        public static LocationManager LocationManagerInstance;

        internal LocationConfig configCache = new LocationConfig();

        internal List<MarkerData> MarkersAll = new List<MarkerData>();
        internal List<MarkerData> MarkersClose = new List<MarkerData>();

        private List<Location> Locations = new List<Location>();
        private List<Position> HospitalSpawns = new List<Position>();
        private List<Vector3> HospitalSpawnsVectors = new List<Vector3>();

        string currentJob = "unemployed";
        bool markerCooldown = false;

        public override async void Begin()
        {
            LocationManagerInstance = this;

            Instance.EventRegistry[LegacyEvents.Client.CuriosityJob] += new Action<bool, bool, string>(OnJobDutyEvent);

            await Session.Loading();

            OnGetLocations();
        }

        private void OnJobDutyEvent(bool active, bool onDuty, string job)
        {
            if (job == currentJob) return;

            currentJob = job;
        }

        private LocationConfig GetLocationConfig()
        {
            LocationConfig config = new LocationConfig();

            string jsonFile = API.LoadResourceFile(API.GetCurrentResourceName(), "config/locations.json"); // Fuck you VS2019 UTF8 BOM

            try
            {
                if (string.IsNullOrEmpty(jsonFile))
                {
                    Logger.Error($"locations.json file is empty or does not exist, please fix this");
                }
                else
                {
                    config = JsonConvert.DeserializeObject<LocationConfig>(jsonFile);
                    configCache = config;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Location JSON File Exception\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }

        private List<Location> GetLocations()
        {
            return GetLocationConfig().Locations;
        }

        internal async Task OnGetLocations()
        {
            try
            {
                if (MarkersAll.Count > 0)
                {
                    BlipManager.ManagerInstance.RemoveAllBlips();
                    MarkersAll.Clear();
                    MarkersClose.Clear();
                }

                Locations = GetLocations();

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

                    if (location.SpawnType == SpawnType.Hospital)
                    {
                        foreach(Position pos in location.Positions)
                        {
                            HospitalSpawns.Add(pos);
                            HospitalSpawnsVectors.Add(pos.AsVector());
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
                            markerData.Bob = m.Bob;
                            markerData.Rotate = m.Rotate;
                            markerData.WrappingMarker = m.WrappingMarker;
                            markerData.SetOnGround = m.SetOnGround;
                            markerData.SpawnType = location.SpawnType;

                            float ground = 0f;

                            if (API.GetGroundZFor_3dCoord_2(markerData.Position.X, markerData.Position.Y, markerData.Position.Z, ref ground, false) && m.SetOnGround)
                                markerData.Position.Z = ground;

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
            MarkersClose = MarkersAll.Where(m => 
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
                World.DrawMarker((MarkerType)m.MarkerId, m.Position, m.VDirection, m.VRotation, m.VScale, m.ColorArgb, bobUpAndDown: m.Bob, faceCamera: m.FaceCamera, rotateY: m.Rotate);
                Vector3 pos = m.Position;
                pos.Z = pos.Z + 1f;
                NativeUI.Notifications.ShowFloatingHelpNotification(m.Message, pos);
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

            if (activeMarker.WrappingMarker)
            {
                float ground = 0f;
                Vector3 position = activeMarker.Position;

                Vector3 scale = activeMarker.VScale;
                scale.Z = .5f;
                scale.X = activeMarker.ContextAoe;
                scale.Y = activeMarker.ContextAoe;

                if (API.GetGroundZFor_3dCoord_2(activeMarker.Position.X, activeMarker.Position.Y, activeMarker.Position.Z, ref ground, false))
                    position.Z = ground;

                World.DrawMarker(MarkerType.VerticalCylinder, position, activeMarker.VDirection, activeMarker.VRotation, scale, activeMarker.ColorArgb);
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

        internal Position NearestHospital()
        {
            Cache.UpdatePedId();
            return FindClosestPoint(Cache.PlayerPed.Position, HospitalSpawns);
        }

        public Position FindClosestPoint(Vector3 startingPoint, IEnumerable<Position> points)
        {
            if (points.Count() == 0) return new Position(0f, 0f, 0f, 0f);

            return points.OrderBy(x => Vector3.Distance(startingPoint, x.AsVector())).First();
        }
    }
}
