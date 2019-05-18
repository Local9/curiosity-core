using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    static class Speedometer3D
    {
        static string speedUnit = "mph";

        static List<VehicleClass> blackList = new List<VehicleClass> {
            VehicleClass.Industrial,
            VehicleClass.Utility,
            VehicleClass.Vans,
            VehicleClass.Boats,
            VehicleClass.Helicopters,
            VehicleClass.Planes,
            VehicleClass.Commercial,
            VehicleClass.Trains };

        public static void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static async Task OnTick()
        {
            try
            {
                if (Game.PlayerPed.IsInVehicle() && !CinematicMode.DoHideHud)
                {
                    Vector3 position = new Vector3(-1.2f, -2.0f, 0.0f);

                    CitizenFX.Core.Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

                    if (VehicleClass.Motorcycles == vehicle.ClassType)
                    {
                        position.Y = -1.1f;
                        position.X = -0.8f;
                    }

                    int handle = vehicle.Handle;
                    Vector3 offset1 = API.GetOffsetFromEntityInWorldCoords(handle, 0.15f + position.X, position.Y, position.Z);

                    float vSpeed = float.Parse(string.Format("{0:0}", Game.PlayerPed.CurrentVehicle.Speed * (speedUnit == "mph" ? 2.24f : 3.60f)));

                    if (ClassCheck(vehicle))
                    {

                        if (vSpeed > -1 && vSpeed < 10)
                        {
                            int speed1 = (int)Math.Floor(vSpeed);

                            DrawMarker(speed1, offset1, vehicle.Heading);
                        }
                        else if (vSpeed > 9 && vSpeed < 100)
                        {
                            int speed1 = (int)Math.Floor(vSpeed / 10);
                            int speed2 = (int)Math.Floor(vSpeed % 10);

                            Vector3 offset2 = API.GetOffsetFromEntityInWorldCoords(handle, 0.0f + position.X, position.Y, position.Z);

                            DrawMarker(speed2, offset1, vehicle.Heading);
                            DrawMarker(speed1, offset2, vehicle.Heading);
                        }
                        else if (vSpeed > 99 && vSpeed < 1000)
                        {
                            int speed1 = (int)Math.Floor(vSpeed / 100);
                            int speed2 = (int)Math.Floor(vSpeed / 10 % 10);
                            int speed3 = (int)Math.Floor(vSpeed % 10);

                            Vector3 offset2 = API.GetOffsetFromEntityInWorldCoords(handle, 0.0f + position.X, position.Y, position.Z);
                            Vector3 offset3 = API.GetOffsetFromEntityInWorldCoords(handle, -0.15f + position.X, position.Y, position.Z);

                            DrawMarker(speed3, offset1, vehicle.Heading);
                            DrawMarker(speed2, offset2, vehicle.Heading);
                            DrawMarker(speed1, offset3, vehicle.Heading);
                        }
                    }
                }
                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Log.Info($"[SPEEDOMETER] {ex.GetType().ToString()} thrown");
            }


            await Task.FromResult(0);
        }

        static void DrawMarker(int value, Vector3 position, float rotation)
        {
            // API.DrawMarker()
            Function.Call(Hash.DRAW_MARKER, value + 10, position.X, position.Y, position.Z, 0, 0, 0, 0.0f, 0.0f, rotation, 0.2f, 0.2f, 0.2f, 255, 255, 255, 200, false, false, 0, false, 0, 0, false);
        }

        static bool ClassCheck(CitizenFX.Core.Vehicle vehicle)
        {
            if (vehicle == null)
                return true;

            foreach(VehicleClass veh in blackList)
            {
                if (vehicle.ClassType == veh)
                {
                    return false;
                }
                return true;
            }

            return true;
        }
    }
}
