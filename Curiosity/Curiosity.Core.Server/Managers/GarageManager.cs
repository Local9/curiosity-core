using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Server.Managers
{
    public class GarageManager : Manager<GarageManager>
    {
        private const float SPAWN_DISTANCE_CHECK = 600.0f;

        public override void Begin()
        {
            /*
             * 1. Can get a list of vehicles - selCharacterVehicles
             * 2. Can request a spawn of the vehicle
             * 3. Can sell a vehicle
             * 5. Can edit a vehicle (mod etc)
             * 
             */

            EventSystem.GetModule().Attach("garage:get:list", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                    {
                        return new List<VehicleItem>();
                    }

                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                    return await Database.Store.VehicleDatabase.GetAllVehicles(curiosityUser.Character.CharacterId);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "garage:get:list");
                    return null;
                }
            }));

            EventSystem.GetModule().Attach("garage:save", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    ExportMessage exportMessage = new ExportMessage();
                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                    if (curiosityUser.Character.Cash < 5000)
                    {
                        exportMessage.Error = "Not enough cash, $5,000 required to save";
                        return exportMessage;
                    }

                    int networkId = metadata.Find<int>(0);
                    VehicleInfo vehicleInfo = metadata.Find<VehicleInfo>(1);

                    int entityId = API.NetworkGetEntityFromNetworkId(networkId);

                    if (!API.DoesEntityExist(entityId))
                    {
                        return false;
                    }

                    Vehicle veh = new Vehicle(entityId);
                    int vehId = veh.State.Get(StateBagKey.VEH_ID);

                    string json = JsonConvert.SerializeObject(vehicleInfo);

                    bool success = await Database.Store.VehicleDatabase.SaveVehicle(curiosityUser.Character.CharacterId, vehId, json);

                    if (success)
                    {
                        long characterAmount = await Database.Store.BankDatabase.Adjust(curiosityUser.Character.CharacterId, -5000);
                        curiosityUser.Character.Cash = characterAmount;
                        return exportMessage;
                    }
                    else
                    {
                        exportMessage.Error = "Vehicle settings were not saved.";
                        return exportMessage;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "garage:save");
                    return false;
                }
            }));

            EventSystem.GetModule().Attach("garage:get:vehicle", new AsyncEventCallback(async metadata =>
            {
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
                    int routingBucket = curiosityUser.RoutingBucket;

                    Vector3 charPos = new Vector3(x, y, z);
                    Vector3 pos = new Vector3(x, y, z);
                    float heading = h;

                    if (vehicleItem.SpawnTypeId == SpawnType.Unknown)
                    {
                        Logger.Debug($"Vehicle {vehicleItem.CharacterVehicleId} incorrectly configured, contact support.");
                        vehicleItem.Message = "Vehicle incorrectly configured, contact support.";
                        return vehicleItem;
                    }

                    bool isVehicle = Equals(vehicleItem.SpawnTypeId, SpawnType.Vehicle);

                    Logger.Debug($"Requested Vehicle Spawn Type: {vehicleItem.SpawnTypeId}:{isVehicle}");

                    // get spawn loacation if not a car
                    if (!isVehicle)
                    {
                        List<Position> spawnPositions = ConfigManager.GetModule().NearestSpawnPositions(pos, vehicleItem.SpawnTypeId, SPAWN_DISTANCE_CHECK);

                        for (int i = 0; i < spawnPositions.Count; i++)
                        {
                            Vector3 positionToCheck = spawnPositions[i].AsVector();
                            if (!AnyVehicleNearPoint(positionToCheck, d))
                            {
                                pos = positionToCheck;
                                heading = spawnPositions[i].H;
                                Logger.Debug($"Spawn position found. {pos.X} {pos.Y} {pos.Z}");
                                goto SpawnVehicle;
                            }

                            Logger.Debug($"Vehicle at position, moving to next one.");
                        }

                        vehicleItem.Message = "Unable to find any nearby spawn locations";
                        return vehicleItem;
                    }

                SpawnVehicle:
                    if (Vector3.Distance(charPos, pos) >= SPAWN_DISTANCE_CHECK)
                    {
                        Logger.Debug($"Too far away from a suitable location.");
                        vehicleItem.Message = "Too far away from a suitable location.";
                        return vehicleItem;
                    }

                    vehicleItem.Heading = heading;
                    vehicleItem.X = pos.X;
                    vehicleItem.Y = pos.Y;
                    vehicleItem.Z = pos.Z;

                    return vehicleItem;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "garage:get:vehicle");
                    return null;
                }
            }));

            EventSystem.GetModule().Attach("garage:set:vehicle", new EventCallback(metadata =>
            {
                try
                {
                    Player player = PluginManager.PlayersList[metadata.Sender];
                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                    int networkId = metadata.Find<int>(0);
                    SpawnType spawnTypeId = (SpawnType)metadata.Find<int>(1);
                    int characterVehicleId = metadata.Find<int>(2);

                    int entityHandle = API.NetworkGetEntityFromNetworkId(networkId);
                    Vehicle vehicle = new Vehicle(entityHandle);

                    float cullingRange = SPAWN_DISTANCE_CHECK;

                    switch (spawnTypeId)
                    {
                        case SpawnType.Boat:
                            if (curiosityUser.PersonalBoat > 0)
                                EntityManager.GetModule().NetworkDeleteEntity(curiosityUser.PersonalBoat);

                            player.State.Set(StateBagKey.VEH_BOAT_NETWORK_ID, vehicle.NetworkId, true);
                            break;
                        case SpawnType.Plane:
                            if (curiosityUser.PersonalPlane > 0)
                                EntityManager.GetModule().NetworkDeleteEntity(curiosityUser.PersonalPlane);

                            cullingRange = 5000f;
                            player.State.Set(StateBagKey.VEH_PLANE_NETWORK_ID, vehicle.NetworkId, true);
                            break;
                        case SpawnType.Helicopter:
                            if (curiosityUser.PersonalHelicopter > 0)
                                EntityManager.GetModule().NetworkDeleteEntity(curiosityUser.PersonalHelicopter);

                            player.State.Set(StateBagKey.VEH_HELI_NETWORK_ID, vehicle.NetworkId, true);
                            break;
                        case SpawnType.Trailer:
                            if (curiosityUser.PersonalTrailer > 0)
                                EntityManager.GetModule().NetworkDeleteEntity(curiosityUser.PersonalTrailer);

                            player.State.Set(StateBagKey.VEH_TRAILER_NETWORK_ID, vehicle.NetworkId, true);
                            break;
                        default:
                            if (curiosityUser.PersonalVehicle > 0)
                                EntityManager.GetModule().NetworkDeleteEntity(curiosityUser.PersonalVehicle);

                            player.State.Set(StateBagKey.VEH_NETWORK_ID, vehicle.NetworkId, true);
                            break;
                    }
                    API.SetEntityDistanceCullingRadius(entityHandle, cullingRange);

                    vehicle.State.Set(StateBagKey.VEH_SPAWNED, true, true);
                    vehicle.State.Set(StateBagKey.VEH_OWNER_ID, player.Handle, true);
                    vehicle.State.Set(StateBagKey.VEH_OWNER, player.Name, true);
                    vehicle.State.Set(StateBagKey.PLAYER_NAME, player.Name, true);
                    vehicle.State.Set(StateBagKey.VEHICLE_MISSION, false, true);
                    vehicle.State.Set(StateBagKey.VEH_SPAWN_TYPE, (int)spawnTypeId, true);
                    vehicle.State.Set(StateBagKey.VEH_ID, characterVehicleId, true);

                    vehicle.State.Set(StateBagKey.BLIP_INFORMATION, new { }, true);

                    if (spawnTypeId != SpawnType.Trailer)
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

                    if (spawnTypeId == SpawnType.Trailer)
                    {
                        vehicle.State.Set(StateBagKey.VEH_PERSONAL_TRAILER, true, true);
                        vehicle.State.Set(StateBagKey.VEH_TRAILER_CONTENT, new { }, true);
                    }

                    API.SetEntityRoutingBucket(entityHandle, (int)curiosityUser.RoutingBucket);

                    API.SetVehicleNumberPlateText(vehicle.Handle, player.Name);

                    Logger.Debug($"Completed setting up vehicle for {player.Name}");

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "garage:set:vehicle");
                    return false;
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

            return vehicles?.Any(x => VehicleDistance(x, location) <= distance + 1.0f) ?? false;
        }

        public List<object> GetVehiclesNearPoint(Vector3 location, float distance)
        {
            // Distance should be greater than zero if you want to know if any vehicles are in area
            if (float.IsNaN(distance) || distance <= 0)
            {
                return null;
            }

            List<object> vehicles = API.GetAllVehicles();

            return vehicles?.Where(x => VehicleDistance(x, location) <= distance + 1.0f).ToList();
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
