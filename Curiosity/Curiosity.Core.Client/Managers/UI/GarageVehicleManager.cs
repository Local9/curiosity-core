using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Managers.UI
{
    public class GarageVehicleManager : Manager<GarageVehicleManager>
    {
        private const string BLIP_PERSONAL_VEHICLE = "blipPersonalVehicle";
        private const string BLIP_PERSONAL_TRAILER = "blipPersonalTrailer";
        private const string BLIP_PERSONAL_PLANE = "blipPersonalPlane";
        private const string BLIP_PERSONAL_BOAT = "blipPersonalBoat";
        private const string BLIP_PERSONAL_HELICOPTER = "blipPersonalHelicopter";

        public override void Begin()
        {
            API.AddTextEntry(BLIP_PERSONAL_VEHICLE, "Personal Vehicle");
            API.AddTextEntry(BLIP_PERSONAL_TRAILER, "Personal Trailer");
            API.AddTextEntry(BLIP_PERSONAL_PLANE, "Personal Plane");
            API.AddTextEntry(BLIP_PERSONAL_BOAT, "Personal Boat");
            API.AddTextEntry(BLIP_PERSONAL_HELICOPTER, "Personal Helicopter");

            Instance.AttachNuiHandler("GarageVehicles", new AsyncEventCallback(async metadata =>
            {
                List<dynamic> vehicles = new List<dynamic>();

                List<VehicleItem> srvVeh = await EventSystem.Request<List<VehicleItem>>("garage:get:list");

                foreach (VehicleItem v in srvVeh)
                {
                    var m = new
                    {
                        characterVehicleId = v.CharacterVehicleId,
                        label = v.Label,
                        licensePlate = v.VehicleInfo.plateText,
                        datePurchased = v.DatePurchased,
                        hash = v.Hash
                    };
                    vehicles.Add(m);
                }

                return vehicles;
            }));

            Instance.AttachNuiHandler("GarageVehicleRequest", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    string characterVehicleIdString = metadata.Find<string>(0);
                    int characterVehicleId = 0;

                    if (!int.TryParse(characterVehicleIdString, out characterVehicleId))
                    {
                        NotificationManger.GetModule().Error("Vehicle information is invalid, if it happens again write up what you were doing on the forums.");
                        return new { success = false };
                    }

                    string hash = metadata.Find<string>(1);

                    Model vehModel = new Model(hash);

                    if (!vehModel.IsValid)
                    {
                        NotificationManger.GetModule().Error($"Model '{hash}' is not valid.");
                        return new { success = false };
                    }

                    DateTime maxTime = DateTime.UtcNow.AddSeconds(10);

                    while (!vehModel.IsLoaded)
                    {
                        await vehModel.Request(3000);

                        if (DateTime.UtcNow > maxTime) break;
                    }

                    if (!vehModel.IsLoaded)
                    {
                        NotificationManger.GetModule().Error("Vehicle was unable to load.<br>If the vehicle is a custom model, please try again after it has finished downloading.");
                        return new { success = false };
                    }

                    Vector3 charPos = Cache.PlayerPed.Position;
                    Vector3 spawnPos = Vector3.Zero;
                    float spawnHeading = 0f;

                    Vector3 spawnRoad = Vector3.Zero;

                    API.GetClosestVehicleNodeWithHeading(charPos.X, charPos.Y, charPos.Z, ref spawnPos, ref spawnHeading, 1, 3f, 0);
                    API.GetRoadSidePointWithHeading(spawnPos.X, spawnPos.Y, spawnPos.Z, spawnHeading, ref spawnRoad);

                    float distance = vehModel.GetDimensions().Y;

                    VehicleItem vehicleItem = await EventSystem.Request<VehicleItem>("garage:get:vehicle", characterVehicleId, spawnRoad.X, spawnRoad.Y, spawnRoad.Z, spawnHeading, distance, (uint)vehModel.Hash);

                    if (API.IsAnyVehicleNearPoint(spawnRoad.X, spawnRoad.Y, spawnRoad.Z, distance) && vehicleItem.SpawnTypeId == SpawnType.Vehicle)
                    {
                        Vehicle[] vehicles = World.GetAllVehicles();

                        for (int i = 0; i < vehicles.Length; i++)
                        {
                            Vehicle vehicle = vehicles[i];
                            if (vehicle.IsInRangeOf(spawnRoad, distance) && vehicle.NetworkId != vehicleItem.NetworkId)
                            {
                                EventSystem.Send("delete:entity", vehicle.NetworkId);
                                await BaseScript.Delay(100);
                            }
                        }

                        NotificationManger.GetModule().Info("Location is blocked by another vehicle, so it was removed from this world.");
                        vehModel.MarkAsNoLongerNeeded();
                    }

                    await BaseScript.Delay(0);

                    vehModel.MarkAsNoLongerNeeded();

                    await BaseScript.Delay(0);

                    if (vehicleItem is null)
                    {
                        NotificationManger.GetModule().Error("Vehicle failed to be created. Please try again.");
                        return new { success = false };
                    }

                    if (!string.IsNullOrEmpty(vehicleItem.Message))
                    {
                        NotificationManger.GetModule().Error(vehicleItem.Message);
                        return new { success = false };
                    }

                    if (vehicleItem.NetworkId == 0)
                    {
                        NotificationManger.GetModule().Error("Vehicle failed to be created on the server. Please try again.");
                        return new { success = false };
                    }

                    await BaseScript.Delay(0);

                    if (vehicleItem.NetworkId > 0)
                    {
                        Vehicle vehicle = null;
                        Vehicle previousVehicle = null;

                        int vehId = API.NetworkGetEntityFromNetworkId(vehicleItem.NetworkId);
                        API.NetworkRequestControlOfEntity(vehId);

                        int failRate = 0;

                        while (!API.DoesEntityExist(vehId))
                        {
                            if (failRate >= 10)
                                goto FAILED;

                            await BaseScript.Delay(5);
                            failRate++;
                        }

                        vehicle = new Vehicle(vehId);

                        vehicle.IsPersistent = true;
                        vehicle.PreviouslyOwnedByPlayer = true;
                        vehicle.IsPositionFrozen = true;
                        vehicle.IsCollisionEnabled = false;

                        API.SetNetworkIdExistsOnAllMachines(vehicle.NetworkId, true);
                        API.SetNetworkIdCanMigrate(vehicle.NetworkId, true);
                        API.SetVehicleHasBeenOwnedByPlayer(vehicle.Handle, true);

                        vehicle.PlaceOnGround();
                        vehicle.RadioStation = RadioStation.RadioOff;

                        await vehicle.FadeOut();

                        await BaseScript.Delay(100);

                        if (Cache.PersonalVehicle is not null && vehicleItem.SpawnTypeId == SpawnType.Vehicle)
                            previousVehicle = Cache.PersonalVehicle.Vehicle;

                        if (Cache.PersonalBoat is not null && vehicleItem.SpawnTypeId == SpawnType.Boat)
                            previousVehicle = Cache.PersonalBoat.Vehicle;

                        if (Cache.PersonalHelicopter is not null && vehicleItem.SpawnTypeId == SpawnType.Helicopter)
                            previousVehicle = Cache.PersonalHelicopter.Vehicle;

                        if (Cache.PersonalPlane is not null && vehicleItem.SpawnTypeId == SpawnType.Boat)
                            previousVehicle = Cache.PersonalBoat.Vehicle;

                        if (Cache.PersonalTrailer is not null && vehicleItem.SpawnTypeId == SpawnType.Trailer)
                            previousVehicle = Cache.PersonalTrailer.Vehicle;

                        if (previousVehicle is not null)
                        {
                            if (previousVehicle.Exists()) // personal vehicle
                            {
                                if (previousVehicle.Driver == Cache.PlayerPed && vehicleItem.SpawnTypeId != SpawnType.Trailer)
                                    Cache.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);

                                await previousVehicle.FadeOut(true);

                                previousVehicle.IsPositionFrozen = true;
                                previousVehicle.IsCollisionEnabled = false;

                                EventSystem.GetModule().Send("delete:entity", previousVehicle.NetworkId);
                                await BaseScript.Delay(5);

                                if (previousVehicle.Exists())
                                {
                                    EntityManager.GetModule().RemoveEntityBlip(previousVehicle);
                                    await BaseScript.Delay(5);

                                    previousVehicle.Delete();
                                    previousVehicle = null;
                                }

                                await BaseScript.Delay(100);
                            }
                        }

                        Vector3 pos = vehicle.Position;
                        API.ClearAreaOfEverything(pos.X, pos.Y, pos.Z, 4f, false, false, false, false);

                        vehicle.Mods.LicensePlate = vehicleItem.VehicleInfo.plateText;

                        if (vehicleItem.SpawnTypeId == SpawnType.Vehicle)
                        {
                            Cache.PersonalVehicle = new State.VehicleState(vehicle);
                            Cache.PlayerPed.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
                            Cache.Player.User.SendEvent("vehicle:log:player", vehicle.NetworkId);
                        }

                        if (vehicleItem.SpawnTypeId == SpawnType.Plane)
                        {
                            Cache.PersonalPlane = new State.VehicleState(vehicle);
                            Cache.Player.User.SendEvent("vehicle:log:player:plane", vehicle.NetworkId);
                        }

                        if (vehicleItem.SpawnTypeId == SpawnType.Boat)
                        {
                            Cache.PersonalBoat = new State.VehicleState(vehicle);
                            Cache.Player.User.SendEvent("vehicle:log:player:boat", vehicle.NetworkId);
                        }

                        if (vehicleItem.SpawnTypeId == SpawnType.Helicopter)
                        {
                            Cache.PersonalHelicopter = new State.VehicleState(vehicle);
                            Cache.Player.User.SendEvent("vehicle:log:player:helicopter", vehicle.NetworkId);
                        }

                        if (vehicleItem.SpawnTypeId == SpawnType.Trailer)
                        {
                            Cache.Player.User.SendEvent("vehicle:log:player:trailer", vehicle.NetworkId);
                            Cache.PersonalTrailer = new State.VehicleState(vehicle);
                        }

                        Blip blip = CreateBlip(vehicle);

                        if (vehicleItem.SpawnTypeId != SpawnType.Trailer)
                            API.SetVehicleExclusiveDriver_2(vehicle.Handle, Game.PlayerPed.Handle, 1);

                        vehicle.State.Set($"{StateBagKey.BLIP_ID}", blip.Handle, false);

                        API.SetNewWaypoint(vehicle.Position.X, vehicle.Position.Y);

                        vehicle.IsPositionFrozen = false;
                        vehicle.IsCollisionEnabled = true;

                        API.SetVehicleAutoRepairDisabled(vehicle.Handle, true);

                        NotificationManger.GetModule().Success("Vehicle has been requested successfully, please follow the waypoint on your map.");

                        // VehicleSpawnSafetyManager.GetModule().EnableSafeSpawnCheck();

                        await vehicle.FadeIn();

                        return new { success = true };

                    FAILED:

                        API.DeleteEntity(ref vehId);

                        NotificationManger.GetModule().Error("Vehicle failed to be created successfully. It might exist but will be a glitch in the matrix.");
                        return new { success = true };
                    }

                    return new { success = true };

                }
                catch (Exception ex)
                {
                    Logger.Error($"Oh well....");
                    NotificationManger.GetModule().Error("FiveM fucked something up");
                    return new { success = false };
                }
            }));
        }

        public Blip CreateBlip(Vehicle vehicle)
        {
            Blip blip = vehicle.AttachBlip();

            int spawnType = vehicle.State.Get($"{StateBagKey.VEH_SPAWN_TYPE}") ?? (int)SpawnType.Vehicle;

            bool setBlip = false;

            if (spawnType == (int)SpawnType.Vehicle)
            {
                API.BeginTextCommandSetBlipName(BLIP_PERSONAL_VEHICLE);
                blip.Sprite = BlipSprite.PersonalVehicleCar;
                setBlip = true;
            }

            if (spawnType == (int)SpawnType.Trailer)
            {
                API.BeginTextCommandSetBlipName(BLIP_PERSONAL_TRAILER);
                API.SetBlipSprite(blip.Handle, 479);
                setBlip = true;
            }

            if (spawnType == (int)SpawnType.Boat)
            {
                API.BeginTextCommandSetBlipName(BLIP_PERSONAL_BOAT);
                blip.Sprite = BlipSprite.Boat;
                setBlip = true;
            }

            if (spawnType == (int)SpawnType.Plane)
            {
                API.BeginTextCommandSetBlipName(BLIP_PERSONAL_PLANE);
                blip.Sprite = BlipSprite.Plane;
                setBlip = true;
            }

            if (spawnType == (int)SpawnType.Helicopter)
            {
                API.BeginTextCommandSetBlipName(BLIP_PERSONAL_HELICOPTER);
                blip.Sprite = BlipSprite.Helicopter;
                setBlip = true;
            }

            API.EndTextCommandSetBlipName(blip.Handle);

            VehicleHash vehicleHash = (VehicleHash)vehicle.Model.Hash;

            if (!setBlip)
            {
                if (ScreenInterface.VehicleBlips.ContainsKey(vehicleHash))
                {
                    API.SetBlipSprite(blip.Handle, ScreenInterface.VehicleBlips[vehicleHash]);
                }
                else
                {
                    if (ScreenInterface.VehicleClassBlips.ContainsKey(vehicle.ClassType))
                    {
                        API.SetBlipSprite(blip.Handle, ScreenInterface.VehicleClassBlips[vehicle.ClassType]);
                    }
                }
            }

            blip.Scale = 0.85f;
            blip.Color = BlipColor.White;
            blip.Priority = 10;
            return blip;
        }
    }
}
