using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Managers.UI
{
    public class GarageVehicleManager : Manager<GarageVehicleManager>
    {
        public override void Begin()
        {
            Instance.AttachNuiHandler("GarageVehicles", new AsyncEventCallback(async metadata =>
            {
                List<dynamic> vehicles = new List<dynamic>();

                List<VehicleItem> srvVeh = await EventSystem.Request<List<VehicleItem>>("garage:get:list");

                foreach(VehicleItem v in srvVeh)
                {
                    var m = new
                    {
                        characterVehicleId = v.CharacterVehicleId,
                        label = v.Label,
                        licensePlate = v.VehicleInfo.plateText,
                        datePurchased = v.DatePurchased
                    };
                    vehicles.Add(m);
                }

                return vehicles;
            }));

            Instance.AttachNuiHandler("GarageVehicleRequest", new AsyncEventCallback(async metadata =>
            {
                int characterVehicleId = metadata.Find<int>(0);

                VehicleItem vehicleItem = await EventSystem.Request<VehicleItem>("garage:get:vehicle", characterVehicleId);

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

                Logger.Debug($"SpawnTypeId: {vehicleItem.SpawnTypeId}");

                if (vehicleItem.NetworkId > 0)
                {
                    Vehicle vehicle = null;
                    Vehicle previousVehicle = null;

                    int vehId = API.NetworkGetEntityFromNetworkId(vehicleItem.NetworkId);

                    int failRate = 0;

                    while (!API.DoesEntityExist(vehId))
                    {
                        if (failRate >= 10)
                            goto FAILED;

                        await BaseScript.Delay(5);
                        failRate++;
                    }

                    vehicle = new Vehicle(vehId);

                    vehicle.IsPositionFrozen = true;
                    vehicle.IsCollisionEnabled = false;

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

                            if (previousVehicle.Exists())
                            {
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

                    API.SetNewWaypoint(vehicle.Position.X, vehicle.Position.Y);

                    vehicle.IsPositionFrozen = false;
                    vehicle.IsCollisionEnabled = true;

                    await vehicle.FadeIn();
                    return new { success = true };

                FAILED:

                    API.DeleteEntity(ref vehId);

                    NotificationManger.GetModule().Error("Vehicle failed to be created successfully. It might exist but will be a glitch in the matrix.");
                    return new { success = true };
                }

                return new { success = true };


            }));
        }
    }
}
