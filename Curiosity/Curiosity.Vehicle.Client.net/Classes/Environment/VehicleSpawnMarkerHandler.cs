﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Vehicle.Client.net.Classes.Environment
{
    // TODO: Test and debug
    // Might make this class more general later to actually inform other classes that it's in range or getting close to a marker
    class Marker
    {
        public VehicleSpawnTypes SpawnType { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Rotation { get; private set; }
        public Vector3 Direction { get; private set; }
        public MarkerType Type { get; private set; }
        public Vector3 Scale { get; private set; }
        public System.Drawing.Color Color { get; private set; }
        public float DrawThreshold { get; private set; }

        public Marker(VehicleSpawnTypes spawnType, Vector3 position, MarkerType type = MarkerType.VerticleCircle)
        {
            this.SpawnType = SpawnType;
            this.Position = position;
            this.Rotation = new Vector3(0, 0, 0);
            this.Direction = new Vector3(0, 0, 0);
            this.Color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            this.Type = type;
            this.Scale = 1.0f * new Vector3(1f, 1f, 1f);
            this.DrawThreshold = 15f;
        }

        public Marker(VehicleSpawnTypes spawnType, Vector3 position, MarkerType type, System.Drawing.Color color, float scale = 0.3f, float drawThreshold = 15f)
        {
            this.SpawnType = SpawnType;
            this.Position = position;
            this.Rotation = new Vector3(0, 0, 0);
            this.Direction = new Vector3(0, 0, 0);
            this.Color = color;
            this.Type = type;
            this.Scale = scale * new Vector3(1f, 1f, 1f);
            this.DrawThreshold = drawThreshold;
        }

        public Marker(VehicleSpawnTypes spawnType, Vector3 position, MarkerType type, System.Drawing.Color color, Vector3 scale, Vector3 rotation, Vector3 direction, float drawThreshold = 15f)
        {
            this.SpawnType = SpawnType;
            this.Position = position;
            this.Rotation = rotation;
            this.Direction = direction;
            this.Color = color;
            this.Type = type;
            this.Scale = scale;
            this.DrawThreshold = drawThreshold;
        }
    }

    internal static class VehicleSpawnMarkerHandler
    {
        static Client client = Client.GetInstance();
        // For e.g. cinematic mode
        static public bool HideAllMarkers = false;
        static public bool IsMenuOpen = false;

        static float contextAoe = 2f; // How close you need to be to see instruction

        // Any other class can add or remove markers from this dictionary (by ID preferrably)
        static internal Dictionary<int, Marker> All = new Dictionary<int, Marker>();
        static internal List<Marker> Close = new List<Marker>();

        static public void Init()
        {
            client.RegisterTickHandler(OnTickMarkerHandler);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            PeriodicUpdate();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            client.RegisterTickHandler(OnTickInformationPanel);
            // SETUP
            SetupVehicleSpawnMarkers();
            // CONTROLLER
        }

        static public async Task OnTickInformationPanel()
        {
            while (true)
            {
                Marker marker = GetActiveMarker();
                if (marker != null && !IsMenuOpen)
                {
                    API.SetTextComponentFormat("STRING");
                    API.AddTextComponentString($" Press ~INPUT_PICKUP~ to open spawn menu.");
                    API.DisplayHelpTextFromStringLabel(0, false, true, -1);
                }

                if (marker == null && IsMenuOpen)
                {
                    IsMenuOpen = false;
                    Menus.VehicleSpawn.CloseMenu();
                }

                await Client.Delay(0);
                await Task.FromResult(0);
            }
        }

        static public Task OnTickMarkerHandler()
        {
            try
            {
                if (!HideAllMarkers)
                {
                    Close.ForEach(m => CitizenFX.Core.World.DrawMarker(m.Type, m.Position, m.Direction, m.Rotation, m.Scale, m.Color, false, false, true));
                }
                if (
                    Game.IsControlJustPressed(0, Control.Pickup) ||
                    Game.IsControlJustReleased(0, Control.Pickup)
                    )
                {
                    if (IsMenuOpen) return Task.FromResult(0);

                    IsMenuOpen = true;

                    Marker marker = GetActiveMarker();
                    if (marker == null) Task.FromResult(0);
                    Menus.VehicleSpawn.OpenMenu(marker.SpawnType);
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
                await BaseScript.Delay(1000);
            }
        }

        static public Task RefreshClose()
        {
            Close = All.ToList().Select(m => m.Value).Where(m => m.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(m.DrawThreshold, 2)).ToList();
            return Task.FromResult(0);
        }

        static void SetupVehicleSpawnMarkers()
        {
            // THIS WILL BE OFF AN EVENT

            Marker marker = new Marker(VehicleSpawnTypes.PoliceCity, new CitizenFX.Core.Vector3(-1108.226f, -847.1646f, 19.31689f), CitizenFX.Core.MarkerType.CarSymbol, System.Drawing.Color.FromArgb(255, 135, 206, 250), 1.0f, 15f);
            Marker marker2 = new Marker(VehicleSpawnTypes.Spawner, new CitizenFX.Core.Vector3(-1108.226f, -847.1646f, 19.31689f - 1f), CitizenFX.Core.MarkerType.VerticalCylinder, System.Drawing.Color.FromArgb(255, 135, 206, 250), 1.5f, 1.5f);

            All.Add(1, marker);
            All.Add(2, marker2);
        }

        static Marker GetActiveMarker()
        {
            try
            {
                Marker closestMarker = Close.Where(w => w.Position.DistanceToSquared(Game.PlayerPed.Position) < contextAoe).FirstOrDefault();
                if (closestMarker == null) return null;
                return closestMarker;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
