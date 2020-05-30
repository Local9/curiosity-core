using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Helper.Area;
using Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle;
using Curiosity.Vehicles.Client.net.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Vehicles.Client.net.Classes.Environment
{

    class SafeZoneVehicle
    {
        Client client = Client.GetInstance();
        CitizenFX.Core.Vehicle _vehicle;

        public SafeZoneVehicle(CitizenFX.Core.Vehicle vehicle)
        {
            _vehicle = vehicle;
            client.RegisterTickHandler(DisableCollision);
            Screen.ShowNotification($"~b~Notification:~w~~n~You have entered a safe zone.", true);
        }

        async Task DisableCollision()
        {
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

            Screen.ShowNotification($"~b~Notification:~w~~n~You have left the safezone.", true);
            Screen.ShowNotification($"If your vehicle speed is stuck, re-enter and exit the safe zone.");
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

        static bool Warning;
        static long TimeEntered;
        const int ONE_MINUITE = (1000 * 60);
        const int FIVE_MINUTES = ONE_MINUITE * 5;

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
            client.RegisterTickHandler(ShowDebugInformation);
        }

        private static async Task ShowDebugInformation()
        {
            if (!Player.PlayerInformation.IsDeveloper())
            {
                client.DeregisterTickHandler(ShowDebugInformation);
            }

            if (Decorators.GetBoolean(Game.PlayerPed.Handle, "player::veh::debug"))
            {
                Vehicle vehicle = Game.PlayerPed.GetVehicleInFront();

                if (vehicle == null) return;

                bool insideSafeZone = Decorators.GetBoolean(vehicle.Handle, Client.DECOR_VEHICLE_SAFEZONE_INSIDE);
                int insideSafeZoneTime = Decorators.GetInteger(vehicle.Handle, Client.DECOR_VEHICLE_SAFEZONE_TIME);

                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("Inside SZ", $"{insideSafeZone}");
                if (insideSafeZone)
                {
                    keyValuePairs.Add("Time Entered SZ", $"{insideSafeZoneTime}");
                    keyValuePairs.Add("Time Inside SZ", $"{API.GetGameTimer() - insideSafeZoneTime}");

                    int gameTimeCountDown = (API.GetGameTimer() - insideSafeZoneTime);
                    bool removeFromSafeZone = gameTimeCountDown > FIVE_MINUTES;
                    int timeLeft = FIVE_MINUTES - (gameTimeCountDown);
                    TimeSpan timeSpan = TimeSpan.FromMilliseconds(timeLeft);
                
                    keyValuePairs.Add("Time Left SZ", $"{timeSpan.Minutes}:{timeSpan.Seconds:00}");
                    keyValuePairs.Add("Remove From SZ", $"{removeFromSafeZone}");
                }

                Wrappers.Helpers.DrawData(vehicle, keyValuePairs);
            }
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

            List<CitizenFX.Core.Vehicle> vehicles = World.GetAllVehicles().ToList().Select(m => m).Where(m => m.Position.Distance(Game.PlayerPed.Position) < 30f).ToList();

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

            //List<Vehicle> vehicles = World.GetAllVehicles().Select(v => v).Where(x => 
            //    Decorators.GetBoolean(x.Handle, Client.DECOR_VEHICLE_SAFEZONE_INSIDE)
            //    && !Decorators.GetBoolean(x.Handle, Client.PLAYER_VEHICLE)).ToList();

            //vehicles.ForEach(async veh =>
            //{
            //    int time = Decorators.GetInteger(veh.Handle, Client.DECOR_VEHICLE_SAFEZONE_TIME);

            //    if (!veh.Driver.Exists())
            //    {
            //        if ((API.GetGameTimer() - time) > FIVE_MINUTES)
            //        {
            //            if (veh.Exists())
            //            {
            //                API.NetworkFadeOutEntity(veh.Handle, false, false);
            //                await BaseScript.Delay(500);
            //                // API.SetNetworkIdCanMigrate(veh.NetworkId, true);
            //                // Spawn.SendDeletionEvent($"{veh.NetworkId}");
            //                if (veh.Exists())
            //                    veh.Delete();
            //            }
            //        }
            //    }
            //});

            if (Game.PlayerPed.IsInVehicle())
                Decorators.Set(Game.PlayerPed.CurrentVehicle.Handle, Client.DECOR_VEHICLE_SAFEZONE_INSIDE, IsInsideSafeZone);

            if (IsInsideSafeZone)
            {
                Game.PlayerPed.Opacity = Opacity;

                if (Game.PlayerPed.IsInVehicle())
                {
                    Decorators.Set(Game.PlayerPed.CurrentVehicle.Handle, Client.DECOR_VEHICLE_SAFEZONE_TIME, API.GetGameTimer());
                    Game.PlayerPed.CurrentVehicle.Opacity = Opacity;
                }

                if ((API.GetGameTimer() - TimeEntered) > ONE_MINUITE && !Warning && Client.CurrentVehicle != null)
                {
                    if (Client.CurrentVehicle == null) return;
                    if (Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle == Client.CurrentVehicle) return;

                    Warning = true;
                    Screen.ShowNotification($"~y~WARNING~n~~w~After 4 mins your vehicle will be deleted from the safe zone.");
                }

                if ((API.GetGameTimer() - TimeEntered) > FIVE_MINUTES && Warning)
                {
                    if (Client.CurrentVehicle == null) return;
                    if (Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle == Client.CurrentVehicle) return;

                    API.NetworkFadeOutEntity(Client.CurrentVehicle.Handle, false, false);
                    await BaseScript.Delay(500);
                    // API.SetNetworkIdCanMigrate(Client.CurrentVehicle.NetworkId, true);
                    // Spawn.SendDeletionEvent($"{Client.CurrentVehicle.NetworkId}");

                    Client.CurrentVehicle.Delete();
                }
            }
            else
            {
                Game.PlayerPed.ResetOpacity();

                if (Game.PlayerPed.IsInVehicle())
                    Game.PlayerPed.CurrentVehicle.ResetOpacity();

                if (Client.CurrentVehicle != null)
                    Client.CurrentVehicle.ResetOpacity();

                Warning = false;
            }
        }

        public static void OnEnter(string identifier, dynamic data)
        {
            if (identifier != SAFE_ZONE) return;

            API.SendNuiMessage(JsonConvert.SerializeObject(new SafeZoneMessage() { safezone = true }));

            client.RegisterTickHandler(SafeZoneVehicles);
            IsInsideSafeZone = true;
            TimeEntered = API.GetGameTimer();

            if (Player.PlayerInformation.IsDeveloper())
            {
                Log.Info("Entered Safezone");
            }
        }

        public static async void OnExit(string identifier, dynamic data)
        {
            try
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
                    CitizenFX.Core.Vehicle Vehicle = Game.PlayerPed.CurrentVehicle;

                    while (API.GetVehicleMaxSpeed(Vehicle.Handle) != API.GetVehicleModelMaxSpeed((uint)Vehicle.Model.Hash))
                    {
                        await BaseScript.Delay(1);
                        Vehicle.MaxSpeed = API.GetVehicleModelMaxSpeed((uint)Vehicle.Model.Hash);
                    }
                }

                if (Player.PlayerInformation.IsDeveloper())
                {
                    Log.Info("Left Safezone");
                }
            }
            catch (Exception ex)
            {

            }
        }
    }

    public class SafeZoneMessage
    {
        public bool safezone;
    }
}
