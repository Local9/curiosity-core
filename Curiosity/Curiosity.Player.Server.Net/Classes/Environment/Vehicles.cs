using CitizenFX.Core;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Curiosity.Server.net.Classes.Environment
{
    class Vehicles
    {
        static Dictionary<string, int> vehicles = new Dictionary<string, int>();
        static List<string> toDelete = new List<string>();

        public static void Init()
        {
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Vehicles:Spawned", new Action<Player, int>(OnVehicleSpawned));

            Server.GetInstance().RegisterTickHandler(OnVehicleCheck);
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

        static async Task OnVehicleCheck()
        {
            lock (vehicles)
            {
                foreach (KeyValuePair<string, int> vehicle in vehicles)
                {
                    lock (SessionManager.PlayerList)
                    {
                        if (!SessionManager.PlayerList.ContainsKey(vehicle.Key))
                        {
                            Server.TriggerClientEvent("curiosity:Server:Vehicles:Remove", vehicle.Value);

                            toDelete.Add(vehicle.Key);
                        }
                    }
                }

                foreach(string key in toDelete)
                {
                    vehicles.Remove(key);
                }

                if (toDelete.Count > 0)
                {
                    toDelete.Clear();
                }
            }
            await Task.FromResult(0);
        }
    }
}
