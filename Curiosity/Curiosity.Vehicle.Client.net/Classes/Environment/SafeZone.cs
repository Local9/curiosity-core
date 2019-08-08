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

namespace Curiosity.Vehicle.Client.net.Classes.Environment
{
    class SafeZone
    {
        static Client client = Client.GetInstance();

        static Vector3 pos1 = new Vector3(-1095.472f, -880.6858f, -1f);
        static Vector3 pos2 = new Vector3(-1033.078f, -840.2169f, 10f);
        static AreaBox areaBox = new AreaBox();
        static List<CitizenFX.Core.Vehicle> vehicles = new List<CitizenFX.Core.Vehicle>();
        static bool HasBeenInAnArea = false;

        static bool IsInArea = false;

        public static void Init()
        {
            areaBox.Angle = 10f;
            areaBox.Pos1 = pos1;
            areaBox.Pos2 = pos2;
            areaBox.Identifier = $"{SpawnLocations.VespucciPD}";

            client.RegisterEventHandler("curiosity:Client:Player:Environment:OnEnterArea", new Action(OnEnter));
            client.RegisterEventHandler("curiosity:Client:Player:Environment:OnExitArea", new Action(OnExit));
            client.RegisterTickHandler(IsInSafeZone);
        }

        static async Task IsInSafeZone()
        {
            await Task.FromResult(0);
            if (Game.Player.IsAlive)
            {
                // areaBox.Draw();

                if (!Game.PlayerPed.IsInVehicle()) return;
                CitizenFX.Core.Vehicle veh = Game.PlayerPed.CurrentVehicle;

                areaBox.Check();

                while (IsInArea)
                {
                    HasBeenInAnArea = true;
                    vehicles = World.GetAllVehicles().ToList().Select(m => m).Where(m => m.Position.DistanceToSquared(veh.Position) < 15f).ToList();
                    vehicles.Remove(veh);
                    await Client.Delay(0);
                    veh.Opacity = 150;
                    Game.PlayerPed.Opacity = 150;

                    while (vehicles.Count > 0)
                    {
                        vehicles.Remove(veh);
                        await Client.Delay(0);
                        foreach (CitizenFX.Core.Vehicle vehicle in vehicles)
                        {
                            if (veh.Handle != vehicle.Handle)
                            {
                                vehicle.Opacity = 150;
                                API.SetEntityNoCollisionEntity(veh.Handle, vehicle.Handle, false);
                                await Client.Delay(0);
                            }
                        }
                        vehicles = World.GetAllVehicles().ToList().Select(m => m).Where(m => m.Position.DistanceToSquared(veh.Position) < 15f).ToList();
                    }

                    vehicles.Clear();

                    areaBox.Check();
                }

                if (HasBeenInAnArea)
                {
                    vehicles = World.GetAllVehicles().ToList().Select(m => m).Where(m => m.Position.DistanceToSquared(veh.Position) > 50f).ToList();

                    foreach (CitizenFX.Core.Vehicle vehicle in vehicles)
                    {
                        vehicle.ResetOpacity();
                        API.SetEntityNoCollisionEntity(veh.Handle, vehicle.Handle, true);
                        await Client.Delay(5);
                    }

                    Game.PlayerPed.ResetOpacity();
                    veh.ResetOpacity();
                    HasBeenInAnArea = false;
                }
            }
        }

        public static void OnEnter()
        {
            IsInArea = true;
        }

        public static void OnExit()
        {
            IsInArea = false;
        }
    }
}
