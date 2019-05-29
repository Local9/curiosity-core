using CitizenFX.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Curiosity.Server.net.Entity;

namespace Curiosity.Server.net.Classes.Environment
{
    class Vehicles
    {
        static Dictionary<int, VehicleData> tempVehicles = new Dictionary<int, VehicleData>();
        static List<int> tempVehiclesToDelete = new List<int>();

        static Server server = Server.GetInstance();

        public static void Init()
        {
            //server.RegisterEventHandler("curiosity:Server:Vehicles:Spawned", new Action<Player, int>(OnVehicleSpawned));
            server.RegisterEventHandler("curiosity:Server:Vehicles:TempStore", new Action<Player, int>(OnPlayerEnteredVehicle));
            server.RegisterEventHandler("curiosity:Server:Vehicles:RemoveFromTempStore", new Action<Player, int>(OnRemoveFromTempStore));

            server.RegisterTickHandler(OnVehicleCheck);
        }

        static void OnRemoveFromTempStore([FromSource]Player player, int vehicleHandle)
        {
            tempVehiclesToDelete.Add(vehicleHandle);
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
                        tempVehicles[vehicleHandle].PlayerHandle = player.Handle;
                    }
                }
                else
                {
                    tempVehicles.Add(vehicleHandle, new VehicleData { NetworkId = vehicleHandle, Updated = DateTime.Now, PlayerHandle = player.Handle });
                }
            }
        }

        //static void OnVehicleSpawned([FromSource]Player player, int vehicleHandle)
        //{
        //    lock (vehicles)
        //    {
        //        if (vehicles.ContainsKey(player.Handle))
        //        {
        //            vehicles[player.Handle] = vehicleHandle;
        //        }
        //        else
        //        {
        //            vehicles.Add(player.Handle, vehicleHandle);
        //        }
        //    }
        //}

        static async Task OnVehicleCheck()
        {
            while (true)
            {
                foreach (int key in tempVehiclesToDelete)
                {
                    tempVehicles.Remove(key);
                }

                foreach (KeyValuePair<int, VehicleData> vehicle in tempVehicles)
                {
                    if (!SessionManager.PlayerList.ContainsKey(vehicle.Value.PlayerHandle))
                    {
                        BaseScript.TriggerClientEvent("curiosity:Client:Vehicles:Remove", vehicle.Key);
                        // tempVehicles.Select((v) => v.Value.PlayerHandle == vehicle.Value.PlayerHandle).ToList().ForEach(p => BaseScript.TriggerClientEvent("curiosity:Client:Vehicles:Remove", vehicle.Key));
                    }
                    else if ((DateTime.Now - vehicle.Value.Updated).Seconds > 300)
                    {
                        BaseScript.TriggerClientEvent("curiosity:Client:Vehicles:Remove", vehicle.Key);
                    }
                    await BaseScript.Delay(0);
                }

                await BaseScript.Delay(60000);
            }
        }
    }
}
