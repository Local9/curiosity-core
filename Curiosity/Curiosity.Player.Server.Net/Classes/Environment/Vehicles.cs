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
            server.RegisterEventHandler("curiosity:Server:Vehicles:TempStore", new Action<CitizenFX.Core.Player, int>(OnPlayerEnteredVehicle));
            server.RegisterEventHandler("curiosity:Server:Vehicles:RemoveFromTempStore", new Action<CitizenFX.Core.Player, int>(OnRemoveFromTempStore));

            server.RegisterEventHandler("playerDropped", new Action<CitizenFX.Core.Player, string>(OnPlayerDropped));

            server.RegisterTickHandler(OnVehicleCheck);
        }

        static void OnPlayerDropped([FromSource]CitizenFX.Core.Player player, string reason)
        {
            foreach (KeyValuePair<int, VehicleData> vehicle in tempVehicles)
            {
                if (vehicle.Value.PlayerHandle == player.Handle)
                {
                    BaseScript.TriggerClientEvent("curiosity:Client:Vehicles:Remove", vehicle.Key);
                }
            }
        }

        static void OnRemoveFromTempStore([FromSource]CitizenFX.Core.Player player, int vehicleHandle)
        {
            lock (tempVehiclesToDelete)
            {
                tempVehiclesToDelete.Add(vehicleHandle);
            }
        }

        static void OnPlayerEnteredVehicle([FromSource]CitizenFX.Core.Player player, int vehicleHandle)
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

        static async Task OnVehicleCheck()
        {
            while (true)
            {
                try
                {
                    lock (tempVehicles)
                    {
                        foreach (int key in tempVehiclesToDelete)
                        {
                            tempVehicles.Remove(key);
                        }
                    }

                    lock (tempVehicles)
                    {
                        RunChecker();
                    }
                }
                catch(Exception ex)
                {
                    Log.Error($"OnVehicleCheck() -> {ex.Message}");
                }

                await BaseScript.Delay(60000);
            }
        }

        static async void RunChecker()
        {
            try
            {
                foreach (KeyValuePair<int, VehicleData> vehicle in tempVehicles)
                {
                    if (!SessionManager.PlayerList.ContainsKey(vehicle.Value.PlayerHandle))
                    {
                        BaseScript.TriggerClientEvent("curiosity:Client:Vehicles:Remove", vehicle.Key);
                    }
                    else if ((DateTime.Now - vehicle.Value.Updated).Seconds > 300)
                    {
                        BaseScript.TriggerClientEvent("curiosity:Client:Vehicles:Remove", vehicle.Key);
                    }
                    await BaseScript.Delay(0);
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"Vehicle -> RunChecker() -> {ex.Message}");
            }
        }
    }
}
