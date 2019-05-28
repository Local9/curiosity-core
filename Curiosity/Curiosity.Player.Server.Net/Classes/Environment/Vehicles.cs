using CitizenFX.Core;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Curiosity.Server.net.Classes.Environment
{
    class Vehicles
    {
        static List<Vehicle> vehicles = new List<Vehicle>();

        public static void Init()
        {
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Vehicles:Spawned", new Action<int>(OnVehicleSpawned));

            Server.GetInstance().RegisterTickHandler(OnVehicleCheck);
        }

        static void OnVehicleSpawned(int vehicleHandle)
        {
            vehicles.Add(new Vehicle(vehicleHandle));
        }

        static async Task OnVehicleCheck()
        {
            foreach(Vehicle vehicle in vehicles)
            {
                if (!SessionManager.PlayerList.ContainsKey(vehicle.Owner.Handle))
                {
                    Server.TriggerClientEvent("curiosity:Server:Vehicles:Remove", vehicle.Handle);
                    vehicles.Remove(vehicle);
                }
            }
            await Task.FromResult(0);
        }
    }
}
