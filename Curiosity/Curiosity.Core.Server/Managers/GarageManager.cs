using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    float x = metadata.Find<float>(1);
                    float y = metadata.Find<float>(2);
                    float z = metadata.Find<float>(3);
                    float h = metadata.Find<float>(4);
                    float d = metadata.Find<float>(5);
                    uint model = metadata.Find<uint>(6);

                    VehicleItem vehicleItem = await Database.Store.VehicleDatabase.GetVehicle(characterVehicleId);
                    await BaseScript.Delay(0);

                    if (vehicleItem is null)
                    {
                        Logger.Error($"Vehicle {characterVehicleId} returned null");
                        vehicleItem.Message = "No Vehicle Found";
                        return vehicleItem;
                    }

                    Player player = PluginManager.PlayersList[metadata.Sender];
                    RoutingBucket routingBucket = curiosityUser.RoutingBucket;

                    Vector3 charPos = new Vector3(x, y, z);
                    Vector3 pos = new Vector3(x, y, z);
                    float heading = h;

                    if (vehicleItem.SpawnTypeId == SpawnType.Unknown)
                    {
                        Logger.Debug($"Vehicle {vehicleItem.CharacterVehicleId} incorrectly configured, contact support.");
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

                        if (AnyVehicleNearPoint(pos, d))
                        {
                            List<object> vehicles = GetVehiclesNearPoint(pos, d);

                            Logger.Debug($"Player {curiosityUser.LatestName} requested a vehicle, but the current location is blocked by another vehicle. So its being deleted!");

                            for(int i = 0; i < vehicles.Count; i++)
                            {
                                int vehIdToDelete = Convert.ToInt32(vehicles[i]);
                                if (API.DoesEntityExist(vehIdToDelete))
                                    API.DeleteEntity(vehIdToDelete);
                            }

                            //vehicleItem.Message = "Current location is blocked by another vehicle";
                            //return vehicleItem;
                        }
                    }
                    else
                    {
                        pos = new Vector3(x, y, z);
                        heading = h;
                    }

                    if (Vector3.Distance(charPos, pos) >= 5000.0f)
                    {
                        Logger.Debug($"Too far away from a suitable location.");
                        vehicleItem.Message = "Too far away from a suitable location.";
                        return vehicleItem;
                    }

                    int vehicleId = API.CreateVehicle(model, pos.X, pos.Y, pos.Z, heading, true, true);

                    if (vehicleId == 0)
                    {
                        Logger.Debug($"Possible OneSync is Disabled");
                        vehicleItem.Message = "Vehicle not created.";
                        return vehicleItem;
                    }

                    vehicleItem.ServerHandle = vehicleId;

                    DateTime maxWaitTime = DateTime.UtcNow.AddSeconds(10);

                    await BaseScript.Delay(0);

                    while (!API.DoesEntityExist(vehicleId))
                    {
                        await BaseScript.Delay(0);

                        if (maxWaitTime < DateTime.UtcNow) break;
                    }

                    if (!API.DoesEntityExist(vehicleId))
                    {
                        Logger.Debug($"Failed to create vehicle in timely manner. Move a little and it may spawn.");
                        vehicleItem.Message = "Failed to create vehicle in timely manner. Move a little and it may spawn.";
                        return vehicleItem;
                    }

                    Vehicle vehicle = new Vehicle(vehicleId);

                    vehicleItem.NetworkId = API.NetworkGetNetworkIdFromEntity(vehicleId);

                    Player p = PluginManager.PlayersList[metadata.Sender];

                    switch(vehicleItem.SpawnTypeId)
                    {
                        case SpawnType.Boat:
                            if (curiosityUser.PersonalBoat > 0)
                                EntityManager.GetModule().NetworkDeleteEntity(curiosityUser.PersonalBoat);

                            p.State.Set(StateBagKey.VEH_BOAT_NETWORK_ID, vehicle.NetworkId, true);
                            break;
                        case SpawnType.Plane:
                            if (curiosityUser.PersonalPlane > 0)
                                EntityManager.GetModule().NetworkDeleteEntity(curiosityUser.PersonalPlane);

                            p.State.Set(StateBagKey.VEH_PLANE_NETWORK_ID, vehicle.NetworkId, true);
                            break;
                        case SpawnType.Helicopter:
                            if (curiosityUser.PersonalHelicopter > 0)
                                EntityManager.GetModule().NetworkDeleteEntity(curiosityUser.PersonalHelicopter);

                            p.State.Set(StateBagKey.VEH_HELI_NETWORK_ID, vehicle.NetworkId, true);
                            break;
                        case SpawnType.Trailer:
                            if (curiosityUser.PersonalTrailer > 0)
                                EntityManager.GetModule().NetworkDeleteEntity(curiosityUser.PersonalTrailer);

                            p.State.Set(StateBagKey.VEH_TRAILER_NETWORK_ID, vehicle.NetworkId, true);
                            break;
                        default:
                            if (curiosityUser.PersonalVehicle > 0)
                                EntityManager.GetModule().NetworkDeleteEntity(curiosityUser.PersonalVehicle);

                            p.State.Set(StateBagKey.VEH_NETWORK_ID, vehicle.NetworkId, true);
                            break;
                    }

                    vehicle.State.Set(StateBagKey.VEH_SPAWNED, true, true);
                    vehicle.State.Set(StateBagKey.VEH_OWNER_ID, player.Handle, true);
                    vehicle.State.Set(StateBagKey.VEH_OWNER, player.Name, true);
                    vehicle.State.Set(StateBagKey.PLAYER_NAME, player.Name, true);
                    vehicle.State.Set(StateBagKey.VEHICLE_MISSION, false, true);
                    vehicle.State.Set(StateBagKey.VEH_SPAWN_TYPE, (int)vehicleItem.SpawnTypeId, true);

                    vehicle.State.Set(StateBagKey.BLIP_INFORMATION, new { }, true);

                    if (vehicleItem.SpawnTypeId != SpawnType.Trailer)
                    {
                        vehicle.State.Set(StateBagKey.VEH_PERSONAL, true, true);

                        vehicle.State.Set(StateBagKey.VEH_FUEL, 0f, true);
                        vehicle.State.Set(StateBagKey.VEH_FUEL_MULTIPLIER, 0f, true);
                        vehicle.State.Set(StateBagKey.VEH_FUEL_SETUP, false, true);
                        vehicle.State.Set(StateBagKey.VEH_CONTENT, new { }, true);

                        vehicle.State.Set(StateBagKey.VEH_SIREN_LIGHTS, false, true);
                        vehicle.State.Set(StateBagKey.VEH_SIREN_SOUND, "", true);
                        vehicle.State.Set(StateBagKey.VEH_SIREN_BLIP, false, true);
                        vehicle.State.Set(StateBagKey.VEH_SIREN_AIRHORN, false, true);
                        vehicle.State.Set(StateBagKey.VEH_SIREN_STATE, false, true);
                    }

                    if (vehicleItem.SpawnTypeId == SpawnType.Trailer)
                    {
                        vehicle.State.Set(StateBagKey.VEH_PERSONAL_TRAILER, true, true);
                        vehicle.State.Set(StateBagKey.VEH_TRAILER_CONTENT, new { }, true);
                    }

                    vehicleItem.NetworkId = API.NetworkGetNetworkIdFromEntity(vehicleId);

                    API.SetEntityRoutingBucket(vehicleId, (int)routingBucket);
                    API.SetEntityDistanceCullingRadius(vehicleId, 15000f);

                    return vehicleItem;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "garage:get:vehicle");
                    return null;
                }
            }));


        }

        public bool AnyVehicleNearPoint(Vector3 location, float distance)
        {
            // Distance should be greater than zero if you want to know if any vehicles are in area
            if (float.IsNaN(distance) || distance <= 0)
            {
                return false;
            }

            List<object> vehicles = API.GetAllVehicles();

            return vehicles?.Any(x => VehicleDistance(x, location) <= distance) ?? false;
        }

        public List<object> GetVehiclesNearPoint(Vector3 location, float distance)
        {
            // Distance should be greater than zero if you want to know if any vehicles are in area
            if (float.IsNaN(distance) || distance <= 0)
            {
                return null;
            }

            List<object> vehicles = API.GetAllVehicles();

            return vehicles?.Where(x => VehicleDistance(x, location) <= distance).ToList();
        }

        public float VehicleDistance(object value, Vector3 location)
        {
            if (!(value is null))
            {
                if (int.TryParse(value.ToString(), out int handle))
                {
                    var vehicle = Entity.FromHandle(handle);
                    return vehicle?.Position.DistanceToSquared(location) ?? float.PositiveInfinity;
                }
            }

            return float.PositiveInfinity;
        }
    }
}
