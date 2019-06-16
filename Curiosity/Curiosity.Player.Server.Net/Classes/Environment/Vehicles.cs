using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Server.net.Entity;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Classes.Environment
{
    class Vehicles
    {
        static Dictionary<int, VehicleData> tempVehicles = new Dictionary<int, VehicleData>();
        static List<int> tempVehiclesToDelete = new List<int>();
        static long timerCheck = API.GetGameTimer();
        static Server server = Server.GetInstance();

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Vehicles:TempStore", new Action<CitizenFX.Core.Player, int>(OnPlayerEnteredVehicle));
            server.RegisterEventHandler("curiosity:Server:Vehicles:RemoveFromTempStore", new Action<CitizenFX.Core.Player, int>(OnRemoveFromTempStore));
            server.RegisterEventHandler("playerDropped", new Action<CitizenFX.Core.Player, string>(OnPlayerDropped));
            server.RegisterTickHandler(OnVehicleCheck);
            Log.Verbose("Vehicle Manager Init");
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
            try
            {
                lock (tempVehiclesToDelete)
                {
                    tempVehiclesToDelete.Add(vehicleHandle);
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.Warn($"OnRemoveFromTempStore() -> {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error($"OnRemoveFromTempStore() -> {ex.Message}");
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
            if ((API.GetGameTimer() - timerCheck) > (1000 * 60) * 5)
            {
                timerCheck = API.GetGameTimer();
                try
                {
                    foreach (int key in tempVehiclesToDelete)
                    {
                        if (!Server.isLive)
                            Log.Verbose($"OnVehicleCheck() -> Remove deleted vehicles from list");

                        tempVehicles.Remove(key);
                    }
                    tempVehiclesToDelete.Clear();

                    Dictionary<int, VehicleData> vehiclesToCheck = tempVehicles;

                    if (!Server.isLive)
                        Log.Verbose($"OnVehicleCheck() -> Vehicles to check {vehiclesToCheck.Count}");

                    int sent = 0;

                    foreach (KeyValuePair<int, VehicleData> vehicle in vehiclesToCheck)
                    {
                        if (!SessionManager.PlayerList.ContainsKey(vehicle.Value.PlayerHandle))
                        {
                            Server.TriggerClientEvent("curiosity:Client:Vehicles:Remove", vehicle.Key);
                            sent++;
                        }
                        else if ((DateTime.Now - vehicle.Value.Updated).Seconds > 300)
                        {
                            sent++;
                            Server.TriggerClientEvent("curiosity:Client:Vehicles:Remove", vehicle.Key);
                        }
                        await BaseScript.Delay(0);
                    }

                    if (!Server.isLive && sent > 0)
                        Log.Verbose($"OnVehicleCheck() -> Vehicles {sent} sent to check if they can be deleted");
                }
                catch (InvalidOperationException ex)
                {
                    Log.Warn($"OnVehicleCheck() -> {ex.Message}");
                }
                catch (Exception ex)
                {
                    Log.Error($"OnVehicleCheck() -> {ex.Message}");
                }
            }
            await Task.FromResult(0);
        }
    }
}
