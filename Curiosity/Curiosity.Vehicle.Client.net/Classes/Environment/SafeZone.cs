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
                _vehicle.SetNoCollision(Game.PlayerPed.CurrentVehicle, false);
                Game.PlayerPed.CurrentVehicle.SetNoCollision(_vehicle, false);
            }
        }

        public void EnableCollision()
        {
            client.DeregisterTickHandler(DisableCollision);

            _vehicle.ResetOpacity();

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
        static Client client = Client.GetInstance();

        static List<AreaBox> safeZones = new List<AreaBox>();
        
        static int Opacity = 200;

        static Dictionary<int, SafeZoneVehicle> safeZoneVehicles = new Dictionary<int, SafeZoneVehicle>();

        static bool IsInsideSafeZone = false;

        public static void Init()
        {
            AreaBox areaBox = new AreaBox();
            areaBox.Angle = 10f;
            areaBox.Pos1 = new Vector3(-1095.472f, -880.6858f, -1f);
            areaBox.Pos2 = new Vector3(-1033.078f, -840.2169f, 100f);
            areaBox.Identifier = $"{SpawnLocations.VespucciPD}";
            safeZones.Add(areaBox);

            AreaBox areaBox2 = new AreaBox();
            areaBox2.Angle = 0f;
            areaBox2.Pos1 = new Vector3(392.5307f, -1027.381f, -1f);
            areaBox2.Pos2 = new Vector3(454.7709f, -966.1293f, 100f);
            areaBox2.Identifier = $"MissionRow";
            safeZones.Add(areaBox2);

            client.RegisterEventHandler("curiosity:Client:Player:Environment:OnEnterArea", new Action(OnEnter));
            client.RegisterEventHandler("curiosity:Client:Player:Environment:OnExitArea", new Action(OnExit));

            client.RegisterTickHandler(IsInSafeZone);
        }

        static async Task SafeZonePeds()
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

        public static void OnEnter()
        {
            client.RegisterTickHandler(SafeZonePeds);
            IsInsideSafeZone = true;

            if (Player.PlayerInformation.IsDeveloper())
            {
                Log.Info("Entered Safezone");
            }
        }

        public static async void OnExit()
        {
            client.DeregisterTickHandler(SafeZonePeds);
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

            if (Player.PlayerInformation.IsDeveloper())
            {
                Log.Info("Left Safezone");
            }
        }
    }
}
