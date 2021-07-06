using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
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
                        vehicleItem.Message = "No Vehicle Found";
                        return vehicleItem;
                    }

                    Player player = PluginManager.PlayersList[metadata.Sender];

                    RoutingBucket routingBucket = curiosityUser.RoutingBucket;

                    var model = API.GetHashKey(vehicleItem.Hash);

                    Vector3 charPos = player.Character.Position;
                    Vector3 pos = new Vector3(charPos.X, charPos.Y, charPos.Z);
                    float heading = player.Character.Heading;

                    if (vehicleItem.SpawnTypeId == SpawnType.Unknown)
                    {
                        vehicleItem.Message = "Vehicle incorrectly configured, contact support.";
                        return vehicleItem;
                    }

                    // get spawn loacation if not a car
                    if (vehicleItem.SpawnTypeId != SpawnType.Vehicle)
                    {
                        Position spawnPos = ConfigManager.GetModule().NearestSpawnPosition(pos, vehicleItem.SpawnTypeId);
                        await BaseScript.Delay(0);

                        if (spawnPos is not null)
                        {
                            pos = spawnPos.AsVector();
                            heading = spawnPos.H;
                        }
                    }

                    if (Vector3.Distance(charPos, pos) >= 5000)
                    {
                        vehicleItem.Message = "Too far away from a suitable location.";
                        return vehicleItem;
                    }

                    // spawn vehicle in location | need to test distant spawning
                    int vehicleId = API.CreateVehicle((uint)model, pos.X, pos.Y, pos.Z, heading, true, true);
                    await BaseScript.Delay(0);

                    if (vehicleId == 0)
                    {
                        Logger.Debug($"Possible OneSync is Disabled");
                        vehicleItem.Message = "Vehicle not created.";
                        return vehicleItem;
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
                        vehicleItem.Message = "Vehicle not created within a timely manor.";
                        return vehicleItem;
                    }

                    Vehicle vehicle = new Vehicle(vehicleId);

                    API.SetEntityDistanceCullingRadius(vehicle.Handle, 5000f);

                    vehicle.State.Set($"{StateBagKey.VEH_SPAWNED}", true, true);
                    vehicle.State.Set($"{StateBagKey.VEH_OWNER_ID}", player.Handle, true);
                    vehicle.State.Set($"{StateBagKey.VEH_OWNER}", player.Name, true);
                    vehicle.State.Set($"{StateBagKey.VEH_SPAWN_TYPE}", (int)vehicleItem.SpawnTypeId, true);

                    vehicle.State.Set($"{StateBagKey.BLIP_INFORMATION}", new { }, true);

                    if (vehicleItem.SpawnTypeId != SpawnType.Trailer)
                    {
                        vehicle.State.Set($"{StateBagKey.VEH_PERSONAL}", true, true);

                        vehicle.State.Set($"{StateBagKey.VEH_FUEL}", 0f, true);
                        vehicle.State.Set($"{StateBagKey.VEH_FUEL_MULTIPLIER}", 0f, true);
                        vehicle.State.Set($"{StateBagKey.VEH_FUEL_SETUP}", false, true);
                        vehicle.State.Set($"{StateBagKey.VEH_CONTENT}", new { }, true);
                    }

                    if (vehicleItem.SpawnTypeId == SpawnType.Trailer)
                    {
                        vehicle.State.Set($"{StateBagKey.VEH_PERSONAL_TRAILER}", true, true);
                        vehicle.State.Set($"{StateBagKey.VEH_TRAILER_CONTENT}", new { }, true);
                    }

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
