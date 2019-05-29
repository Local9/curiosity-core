using CitizenFX.Core;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Curiosity.Server.net.Entity;

namespace Curiosity.Server.net.Classes.Environment
{
    class Vehicles
    {
        static Dictionary<string, int> vehicles = new Dictionary<string, int>();
        static List<string> toDelete = new List<string>();

        static Dictionary<int, VehicleData> tempVehicles = new Dictionary<int, VehicleData>();
        static List<int> tempVehiclesToDelete = new List<int>();

        static Server server = Server.GetInstance();

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Vehicles:Spawned", new Action<Player, int>(OnVehicleSpawned));
            server.RegisterEventHandler("curiosity:Server:Vehicles:TempStore", new Action<Player, int>(OnPlayerEnteredVehicle));

            server.RegisterTickHandler(OnVehicleCheck);
            server.RegisterTickHandler(OnTempVehicleCheck);
        }

        static void OnPlayerEnteredVehicle([FromSource]Player player, int vehicleHandle)
        {
            lock (tempVehicles)
            {
                if (tempVehicles.ContainsKey(vehicleHandle))
                {
                    if ((DateTime.Now - tempVehicles[vehicleHandle].Updated).Seconds > 5)
                    {
                        tempVehicles[vehicleHandle].Updated = DateTime.Now;
                        Log.Verbose($"Vehicle with NetworkID {vehicleHandle} updated in temp store");
                    }
                }
                else
                {
                    tempVehicles.Add(vehicleHandle, new VehicleData { NetworkId = vehicleHandle, Updated = DateTime.Now });
                    Log.Verbose($"Vehicle with NetworkID {vehicleHandle} added to temp store");
                }
            }
        }

        static void OnVehicleSpawned([FromSource]Player player, int vehicleHandle)
        {
            lock (vehicles)
            {
                if (vehicles.ContainsKey(player.Handle))
                {
                    vehicles[player.Handle] = vehicleHandle;
                }
                else
                {
                    vehicles.Add(player.Handle, vehicleHandle);
                }
            }
        }

        static async Task OnTempVehicleCheck()
        {
            while (true)
            {
                foreach (KeyValuePair<int, VehicleData> vehicle in tempVehicles)
                {
                    if ((DateTime.Now - vehicle.Value.Updated).Seconds > 10)
                    {
                        BaseScript.TriggerClientEvent("curiosity:Client:Vehicles:Remove", vehicle.Key);

                        tempVehiclesToDelete.Add(vehicle.Key);

                        Log.Verbose($"Marked Vehicle with NetworkID {vehicle.Value.NetworkId} for removal check");
                    }
                }

                foreach (int key in tempVehiclesToDelete)
                {
                    tempVehicles.Remove(key);
                }

                if (tempVehiclesToDelete.Count > 0)
                {
                    tempVehiclesToDelete.Clear();
                }
                await BaseScript.Delay(1000);
            }
        }

        static async Task OnVehicleCheck()
        {
            while (true)
            {
                foreach (KeyValuePair<string, int> vehicle in vehicles)
                {
                    lock (SessionManager.PlayerList)
                    {
                        if (!SessionManager.PlayerList.ContainsKey(vehicle.Key))
                        {
                            BaseScript.TriggerClientEvent("curiosity:Client:Vehicles:Remove", vehicle.Value);

                            toDelete.Add(vehicle.Key);

                            Log.Verbose($"Marked Vehicle with NetworkID {vehicle.Value} for removal check");
                        }
                    }
                }

                foreach (string key in toDelete)
                {
                    vehicles.Remove(key);
                }

                if (toDelete.Count > 0)
                {
                    toDelete.Clear();
                }
                await BaseScript.Delay(1000);
            }
        }
    }
}
