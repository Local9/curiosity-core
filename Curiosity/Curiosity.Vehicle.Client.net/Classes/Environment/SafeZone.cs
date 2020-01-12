using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Helper.Area;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Shared.Client.net;
using Newtonsoft.Json;

namespace Curiosity.Vehicle.Client.net.Classes.Environment
{

    class SafeZoneVehicle
    {
        Client client = Client.GetInstance();
        CitizenFX.Core.Vehicle _vehicle;

        public SafeZoneVehicle(CitizenFX.Core.Vehicle vehicle)
        {
            _vehicle = vehicle;
            client.RegisterTickHandler(DisableCollision);
        }

        async Task DisableCollision()
        {
            await Client.Delay(0);

            _vehicle.Opacity = 200;

            _vehicle.SetNoCollision(Game.PlayerPed, false);
            Game.Player.Character.SetNoCollision(_vehicle, false);

            if (Game.PlayerPed.IsInVehicle())
            {
                _vehicle.MaxSpeed = 10f;

                _vehicle.SetNoCollision(Game.PlayerPed.CurrentVehicle, false);
                Game.PlayerPed.CurrentVehicle.SetNoCollision(_vehicle, false);
            }
        }

        public void EnableCollision()
        {
            client.DeregisterTickHandler(DisableCollision);

            _vehicle.ResetOpacity();
            _vehicle.MaxSpeed = API.GetVehicleModelMaxSpeed((uint)_vehicle.Model.Hash);

            _vehicle.SetNoCollision(Game.PlayerPed, true);
            Game.Player.Character.SetNoCollision(_vehicle, true);

            if (Game.PlayerPed.IsInVehicle())
            {
                _vehicle.SetNoCollision(Game.PlayerPed.CurrentVehicle, true);
                Game.PlayerPed.CurrentVehicle.SetNoCollision(_vehicle, true);
            }
        }
    }

    class SafeZone
    {
        private const string SAFE_ZONE = "safezone";
        static Client client = Client.GetInstance();

        static List<AreaBox> safeZones = new List<AreaBox>();
        
        static int Opacity = 200;

        static Dictionary<int, SafeZoneVehicle> safeZoneVehicles = new Dictionary<int, SafeZoneVehicle>();

        static bool IsInsideSafeZone = false;
        static bool DebugAreas = false;

        public static void Init()
        {
            AreaBox areaBox = new AreaBox();
            areaBox.Angle = 10f;
            areaBox.Pos1 = new Vector3(-1095.472f, -880.6858f, -1f);
            areaBox.Pos2 = new Vector3(-1033.078f, -840.2169f, 50f);
            areaBox.Identifier = $"safezone";
            safeZones.Add(areaBox);

            AreaBox areaBox2 = new AreaBox();
            areaBox2.Angle = 0f;
            areaBox2.Pos1 = new Vector3(392.5307f, -1027.381f, -1f);
            areaBox2.Pos2 = new Vector3(454.7709f, -966.1293f, 50f);
            areaBox2.Identifier = $"safezone";
            safeZones.Add(areaBox2);

            client.RegisterEventHandler("curiosity:Client:Player:Environment:OnEnterArea", new Action<string, dynamic>(OnEnter));
            client.RegisterEventHandler("curiosity:Client:Player:Environment:OnExitArea", new Action<string, dynamic>(OnExit));

            client.RegisterEventHandler("curiosity:Client:Player:Environment:DrawAreas", new Action(OnDrawAreas));

            client.RegisterTickHandler(IsInSafeZone);
        }

        static void OnDrawAreas()
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;

            DebugAreas = !DebugAreas;
        }

        static async Task SafeZoneVehicles()
        {
            await Client.Delay(0);
            int PlayerHandle = Game.PlayerPed.Handle;

            List<CitizenFX.Core.Vehicle> vehicles = World.GetAllVehicles().ToList().Select(m => m).Where(m => m.Position.DistanceToSquared(Game.PlayerPed.Position) < 30f).ToList();

            foreach (CitizenFX.Core.Vehicle vehicle in vehicles)
            {
                if (safeZoneVehicles.ContainsKey(vehicle.Handle)) continue;
                safeZoneVehicles.Add(vehicle.Handle, new SafeZoneVehicle(vehicle));
            }
        }

        static async Task IsInSafeZone()
        {
            await Task.FromResult(0);
            if (Game.Player.IsAlive)
            {
                foreach (AreaBox areaBox in safeZones)
                {
                    areaBox.Check();

                    if (DebugAreas)
                        areaBox.Draw();
                }
            }

            if (IsInsideSafeZone)
            {
                Game.PlayerPed.Opacity = Opacity;

                if (Game.PlayerPed.IsInVehicle())
                {
                    Game.PlayerPed.CurrentVehicle.Opacity = Opacity;
                }
            }
            else
            {
                Game.PlayerPed.ResetOpacity();

                if (Game.PlayerPed.IsInVehicle())
                    Game.PlayerPed.CurrentVehicle.ResetOpacity();

                if (Client.CurrentVehicle != null)
                    Client.CurrentVehicle.ResetOpacity();
            }
        }

        public static void OnEnter(string identifier, dynamic data)
        {
            if (identifier != SAFE_ZONE) return;

            API.SendNuiMessage(JsonConvert.SerializeObject(new SafeZoneMessage() { safezone = true }));

            client.RegisterTickHandler(SafeZoneVehicles);
            IsInsideSafeZone = true;

            if (Player.PlayerInformation.IsDeveloper())
            {
                Log.Info("Entered Safezone");
            }
        }

        public static async void OnExit(string identifier, dynamic data)
        {
            if (identifier != SAFE_ZONE) return;

            API.SendNuiMessage(JsonConvert.SerializeObject(new SafeZoneMessage() { safezone = false }));

            client.DeregisterTickHandler(SafeZoneVehicles);
            IsInsideSafeZone = false;

            List<CitizenFX.Core.Vehicle> vehs = World.GetAllVehicles().ToList();

            foreach (CitizenFX.Core.Vehicle vehicle in vehs)
            {
                await Client.Delay(0);
                if (vehicle.Exists())
                {
                    vehicle.ResetOpacity();
                    if (safeZoneVehicles.ContainsKey(vehicle.Handle))
                    {
                        safeZoneVehicles[vehicle.Handle].EnableCollision();
                        safeZoneVehicles.Remove(vehicle.Handle);
                    };
                }
            }

            safeZoneVehicles.Clear();

            if (Game.PlayerPed.IsInVehicle())
            {
                while (API.GetVehicleMaxSpeed(Game.PlayerPed.CurrentVehicle.Handle) != API.GetVehicleModelMaxSpeed((uint)Game.PlayerPed.CurrentVehicle.Model.Hash))
                {
                    await BaseScript.Delay(1);
                    Game.PlayerPed.CurrentVehicle.MaxSpeed = API.GetVehicleModelMaxSpeed((uint)Game.PlayerPed.CurrentVehicle.Model.Hash);
                }
            }

            if (Player.PlayerInformation.IsDeveloper())
            {
                Log.Info("Left Safezone");
            }
        }
    }

    public class SafeZoneMessage
    {
        public bool safezone;
    }
}
