using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Data;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Curiosity.Global.Shared.net.Data.BlipHandler;

namespace Curiosity.Police.Client.net.Environment.Job
{
    class Marker
    {
        public Vector3 Position { get; private set; }
        public MarkerType Type { get; private set; }
        public Vector3 Scale { get; private set; }
        public System.Drawing.Color Color { get; private set; }
        public float DrawThreshold { get; private set; }

        public Vector3 Rotation { get; private set; }
        public Vector3 Direction { get; private set; }

        public PatrolZone PatrolZone { get; private set; }

        public Marker(Vector3 position, PatrolZone patrolZone = PatrolZone.City, MarkerType type = MarkerType.VerticalCylinder, float drawThreshold = 5f)
        {
            this.PatrolZone = patrolZone;
            this.Position = position;
            this.Color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            this.Type = type;
            this.Scale = 1.0f * new Vector3(1f, 1f, 1f);
            this.DrawThreshold = drawThreshold;
            this.Rotation = new Vector3(0, 0, 0);
            this.Direction = new Vector3(0, 0, 0);
        }
    }

    static class DutyMarkers
    {
        static Client client = Client.GetInstance();
        // For e.g. cinematic mode
        static public bool HideAllMarkers = false;

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

            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));

            Setup();
        }

        static void OnPlayerSpawned(dynamic spawnObj)
        {
            Setup();
        }

        static void Setup()
        {
            if (MarkersAll.Count == 0)
            {
                MarkersAll.Add(1, new Marker(new Vector3(-1110.701f, -844.0428f, 18.31688f), PatrolZone.City));
                MarkersAll.Add(2, new Marker(new Vector3(441.0764f, -981.1343f, 29.6896f), PatrolZone.City));
                MarkersAll.Add(3, new Marker(new Vector3(622.13f, 17.52761f, 86.86286f), PatrolZone.City));
                MarkersAll.Add(4, new Marker(new Vector3(1851.431f, 3683.458f, 33.26704f), PatrolZone.Country)); // SANDY
                MarkersAll.Add(5, new Marker(new Vector3(-448.4843f, 6008.57f, 30.71637f), PatrolZone.Country)); // PALETO Bay
                MarkersAll.Add(6, new Marker(new Vector3(368.8574f, -1610.202f, 28.29193f), PatrolZone.City));
                MarkersAll.Add(7, new Marker(new Vector3(826.2648f, -1288.504f, 27.24066f), PatrolZone.City));
                MarkersAll.Add(8, new Marker(new Vector3(-561.2444f, -132.6783f, 37.04274f), PatrolZone.City)); // ROCKFORD

                if (AllBlips.Count == 0)
                {
                    foreach (KeyValuePair<int, Marker> keyValuePair in MarkersAll)
                    {
                        AddBlip(new BlipData(keyValuePair.Key, "Police Duty", keyValuePair.Value.Position, BlipSprite.PoliceStation, BlipCategory.Unknown, BlipColor.Blue));
                    }
                }
            }
        }

        static public async Task OnTickInformationPanel()
        {
            while (true)
            {
                Marker marker = GetActiveMarker();
                if (marker != null)
                {
                    API.SetTextComponentFormat("STRING");

                    string dutyText = (!DutyManager.IsPoliceJobActive) ? "go on Duty" : "go off Duty";

                    API.AddTextComponentString($"Press ~INPUT_PICKUP~ to {dutyText}.");
                    API.DisplayHelpTextFromStringLabel(0, false, true, -1);

                    if (Game.IsControlJustPressed(0, Control.Pickup) ||
                    Game.IsControlJustReleased(0, Control.Pickup))
                    {
                        Classes.Menus.MenuLoadout.OpenMenu();

                        if (!DutyManager.IsPoliceJobActive)
                        {
                            Client.TriggerEvent("curiosity:Client:Interface:Duty", true, false, "police");
                            Client.TriggerEvent("curiosity:Client:Police:PatrolZone", (int)marker.PatrolZone);
                        }
                        else
                        {
                            Client.TriggerEvent("curiosity:Client:Interface:Duty", false, false, "jobless");
                        }
                        await Client.Delay(5000);
                    }
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
                    MarkersClose.ForEach(m => {
                        CitizenFX.Core.World.DrawMarker(m.Type, m.Position, m.Direction, m.Rotation, m.Scale, m.Color, false, false, true);
                        string dutyMessage = (!DutyManager.IsPoliceJobActive) ? "~g~Go on Duty" : "~r~Go off Duty";
                        NativeWrappers.Draw3DText(m.Position.X, m.Position.Y, m.Position.Z + 1, $"~s~Police Duty Status~n~{dutyMessage}", 50f, 10f);
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
                await BaseScript.Delay(1000);
            }
        }

        static public Task RefreshClose()
        {
            MarkersClose = MarkersAll.ToList().Select(m => m.Value).Where(m => m.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(m.DrawThreshold, 2)).ToList();
            return Task.FromResult(0);
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
