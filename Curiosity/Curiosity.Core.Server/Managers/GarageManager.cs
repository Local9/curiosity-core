using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Core.Server.Managers
{
    public class GarageManager : Manager<GarageManager>
    {
        public override void Begin()
        {
            /*
             * 1. Can get a list of vehicles - selCharacterVehicles
             * 2. Can request a spawn of the vehicle
             * 3. Can sell a vehicle
             * 5. Can edit a vehicle (mod etc)
             * 
             */

            EventSystem.GetModule().Attach("garage:get:list", new AsyncEventCallback(async metadata => {
                try
                {
                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                    return await Database.Store.VehicleDatabase.GetAllVehicles(curiosityUser.Character.CharacterId);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "garage:get:list");
                    return null;
                }
            }));

            EventSystem.GetModule().Attach("garage:get:vehicle", new AsyncEventCallback(async metadata => {
                try
                {
                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                    int characterVehicleId = metadata.Find<int>(0);

                    VehicleItem vehicleItem = await Database.Store.VehicleDatabase.GetVehicle(characterVehicleId);

                    if (vehicleItem is null)
                    {
                        Logger.Error($"Vehicle {characterVehicleId} returned null");
                        return null;
                    }

                    if (string.IsNullOrEmpty(vehicleItem.VehicleInfo.plateText)) vehicleItem.VehicleInfo.plateText = $"#{curiosityUser.Character.CharacterId}";

                    Player player = PluginManager.PlayersList[metadata.Sender];

                    RoutingBucket routingBucket = curiosityUser.RoutingBucket;

                    var model = API.GetHashKey(vehicleItem.Hash);

                    Vector3 pos = player.Character.Position;

                    int vehicleId = API.CreateVehicle((uint)model, pos.X, pos.Y, pos.Z, player.Character.Heading, true, true);

                    if (vehicleId == 0)
                    {
                        Logger.Debug($"Possible OneSync is Disabled");
                        return null;
                    }

                    DateTime maxWaitTime = DateTime.UtcNow.AddSeconds(5);

                    while (!API.DoesEntityExist(vehicleId))
                    {
                        await BaseScript.Delay(0);

                        if (maxWaitTime < DateTime.UtcNow) break;
                    }

                    if (!API.DoesEntityExist(vehicleId))
                    {
                        Logger.Debug($"Failed to create vehicle in timely manor.");
                        return null;
                    }

                    Vehicle vehicle = new Vehicle(vehicleId);
                    vehicle.State.Set("VEH_SPAWNED", true, true);
                    vehicle.State.Set("VEH_PERSONAL", true, true);

                    API.SetEntityRoutingBucket(vehicleId, (int)routingBucket);

                    vehicleItem.NetworkId = API.NetworkGetNetworkIdFromEntity(vehicleId);

                    return vehicleItem;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "garage:get:vehicle");
                    return null;
                }
            }));


        }
    }
}
