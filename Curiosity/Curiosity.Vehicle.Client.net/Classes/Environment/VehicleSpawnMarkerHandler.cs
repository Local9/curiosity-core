using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using Curiosity.Shared.Client.net;
using Curiosity.Vehicles.Client.net.Classes.CurPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Vehicles.Client.net.Classes.Environment
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
        public int SpawnId { get; private set; }

        public Marker(VehicleSpawnTypes spawnType, int spawnId, Vector3 position, MarkerType type = MarkerType.VerticleCircle)
        {
            this.SpawnType = SpawnType;
            this.Position = position;
            this.Rotation = new Vector3(0, 0, 0);
            this.Direction = new Vector3(0, 0, 0);
            this.Color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            this.Type = type;
            this.Scale = 1.0f * new Vector3(1f, 1f, 1f);
            this.DrawThreshold = 15f;
            this.SpawnId = spawnId;
        }

        public Marker(VehicleSpawnTypes spawnType, int spawnId, Vector3 position, MarkerType type, System.Drawing.Color color, float scale = 0.3f, float drawThreshold = 15f)
        {
            this.SpawnType = SpawnType;
            this.Position = position;
            this.Rotation = new Vector3(0, 0, 0);
            this.Direction = new Vector3(0, 0, 0);
            this.Color = color;
            this.Type = type;
            this.Scale = scale * new Vector3(1f, 1f, 1f);
            this.DrawThreshold = drawThreshold;
            this.SpawnId = spawnId;
        }

        public Marker(VehicleSpawnTypes spawnType, int spawnId, Vector3 position, MarkerType type, System.Drawing.Color color, Vector3 scale, Vector3 rotation, Vector3 direction, float drawThreshold = 15f)
        {
            this.SpawnType = SpawnType;
            this.Position = position;
            this.Rotation = rotation;
            this.Direction = direction;
            this.Color = color;
            this.Type = type;
            this.Scale = scale;
            this.DrawThreshold = drawThreshold;
            this.SpawnId = spawnId;
        }
    }

    internal static class VehicleSpawnMarkerHandler
    {
        static Plugin client = Plugin.GetInstance();
        // For e.g. cinematic mode
        static public bool HideAllMarkers = false;
        static public bool IsMenuOpen = false;
        static public bool IsSpawning = false;

        static float contextAoe = 2f; // How close you need to be to see instruction

        // Any other class can add or remove markers from this dictionary (by ID preferrably)
        static internal Dictionary<int, Marker> MarkersAll = new Dictionary<int, Marker>();
        static internal List<Marker> MarkersClose = new List<Marker>();

        static string DeveloperJson;

        static public void Init()
        {
            client.RegisterTickHandler(OnTickMarkerHandler);
            // client.RegisterTickHandler(OnTickBlipHandler);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            PeriodicUpdate();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            client.RegisterTickHandler(OnTickInformationPanel);
            // CONTROLLER
            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
            client.RegisterEventHandler("curiosity:Client:Vehicle:SpawnLocations", new Action<string>(OnVehicleSpawnLocations));
        }

        static async void OnPlayerSpawned(dynamic spawnObj)
        {
            IsSpawning = true;
            await Plugin.Delay(5000);
            Plugin.TriggerServerEvent("curiosity:Server:Vehicle:GetVehicleSpawnLocations");
        }

        static async void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            await Plugin.Delay(10000);
            if (!IsSpawning)
            {
                Plugin.TriggerServerEvent("curiosity:Server:Vehicle:GetVehicleSpawnLocations");
            }
        }

        static public async Task OnTickInformationPanel()
        {
            while (true)
            {
                Marker marker = GetActiveMarker();
                if (marker != null && !IsMenuOpen)
                {
                    API.SetTextComponentFormat("STRING");
                    API.AddTextComponentString($" Press ~INPUT_PICKUP~ to open menu.");
                    API.DisplayHelpTextFromStringLabel(0, false, true, -1);

                    if (Game.IsControlJustPressed(0, Control.Pickup) ||
                    Game.IsControlJustReleased(0, Control.Pickup))
                    {
                        if (!IsMenuOpen)
                        {
                            IsMenuOpen = true;
                            Menus.VehicleSpawn.OpenMenu(marker.SpawnId);
                        }
                    }
                }

                if (marker == null && IsMenuOpen)
                {
                    IsMenuOpen = false;
                    Menus.VehicleSpawn.CloseMenu();
                }

                await Plugin.Delay(0);
                await Task.FromResult(0);
            }
        }

        static public Task OnTickMarkerHandler()
        {
            try
            {
                if (!HideAllMarkers)
                {
                    MarkersClose.ForEach(m => CitizenFX.Core.World.DrawMarker(m.Type, m.Position, m.Direction, m.Rotation, m.Scale, m.Color, false, false, true));
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
            MarkersClose = MarkersAll.ToList().Select(m => m.Value).Where(m => m.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(m.DrawThreshold, 2)).ToList();
            return Task.FromResult(0);
        }

        static async void OnVehicleSpawnLocations(string encodedJson)
        {
            try
            {
                MarkersAll.Clear();

                string json = Encode.BytesToStringConverted(System.Convert.FromBase64String(encodedJson));

                DeveloperJson = json;

                List<VehicleSpawnLocation> vehicleSpawnLocations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<VehicleSpawnLocation>>(json);

                foreach (VehicleSpawnLocation vehicleSpawnLocation in vehicleSpawnLocations)
                {
                    if (!MarkersAll.ContainsKey(vehicleSpawnLocation.spawnId))
                    {
                        await Plugin.Delay(5);
                        Vector3 markerLocation = new Vector3(vehicleSpawnLocation.X, vehicleSpawnLocation.Y, vehicleSpawnLocation.Z);
                        Marker marker = new Marker((VehicleSpawnTypes)vehicleSpawnLocation.spawnTypeId, vehicleSpawnLocation.spawnId, markerLocation, (MarkerType)vehicleSpawnLocation.spawnMarker, System.Drawing.Color.FromArgb(255, 255, 255, 255), 1.0f, 15f);
                        MarkersAll.Add(vehicleSpawnLocation.spawnId, marker);

                        BlipData blipData = new BlipData(vehicleSpawnLocation.spawnId, vehicleSpawnLocation.spawnBlipName, markerLocation, (BlipSprite)vehicleSpawnLocation.spawnBlip, Shared.Client.net.Enums.BlipCategory.Unknown, (BlipColor)vehicleSpawnLocation.spawnBlipColor);
                        BlipHandler.Add(blipData);
                    }
                }

                if (PlayerInformation.IsDeveloper())
                {
                    Plugin.TriggerEvent("curiosity:Client:Notification:LifeV", 2, "Vehicle Resource", $"Markers & Blips Setup", string.Empty, 2);
                }
            }
            catch (Exception ex)
            {
                if (PlayerInformation.IsDeveloper())
                {
                    Log.Error($"{ex.Message}");
                    Log.Error($"{ex.ToString()}");
                    Log.Error($"{DeveloperJson}");
                }
            }
        }

        static Marker GetActiveMarker()
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
    }
}
