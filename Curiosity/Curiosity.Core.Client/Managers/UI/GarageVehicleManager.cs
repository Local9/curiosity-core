using CitizenFX.Core;
using CitizenFX.Core.Native;
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

                if (vehicleItem is null)
                {
                    NotificationManger.GetModule().Error("Vehicle failed to be created successfully.");
                    return new { success = true };
                }

                if (vehicleItem.NetworkId == 0)
                {
                    NotificationManger.GetModule().Error("Vehicle failed to be created successfully.");
                    return new { success = true };
                }

                if (vehicleItem.NetworkId > 0)
                {

                    Vehicle vehicle = null;

                    if (Cache.PersonalVehicle is not null && !vehicleItem.IsTrailer)
                        vehicle = Cache.PersonalVehicle.Vehicle;

                    if (Cache.PersonalTrailer is not null && vehicleItem.IsTrailer)
                        vehicle = Cache.PersonalTrailer.Vehicle;

                    if (vehicle is not null)
                    {
                        if (vehicle.Exists()) // personal vehicle
                        {
                            if (vehicle.Driver == Cache.PlayerPed && !vehicleItem.IsTrailer)
                                Cache.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);

                            await vehicle.FadeOut(true);

                            vehicle.IsPositionFrozen = true;
                            vehicle.IsCollisionEnabled = false;

                            EventSystem.GetModule().Send("delete:entity", vehicle.NetworkId);

                            if (vehicle.Exists())
                                vehicle.Delete();

                            await BaseScript.Delay(500);
                        }
                    }

                    if (Cache.Entity is not null && Cache.Entity.Vehicle is not null)
                    {
                        if (Cache.Entity.Vehicle.Exists()) // get vehicle player is in
                        {
                            vehicle = Cache.Entity.Vehicle;

                            if (vehicle.Driver == Cache.PlayerPed && !vehicleItem.IsTrailer)
                                Cache.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);

                            await vehicle.FadeOut(true);

                            vehicle.IsPositionFrozen = true;
                            vehicle.IsCollisionEnabled = false;

                            EventSystem.GetModule().Send("delete:entity", vehicle.NetworkId);

                            if (vehicle.Exists())
                                vehicle.Delete();

                            await BaseScript.Delay(500);
                        }
                    }

                    int vehId = API.NetworkGetEntityFromNetworkId(vehicleItem.NetworkId);

                    if (!API.DoesEntityExist(vehId))
                    {
                        NotificationManger.GetModule().Error("Vehicle failed to be created successfully.");
                        return new { success = true };
                    }

                    vehicle = new Vehicle(vehId);

                    vehicle.Mods.LicensePlate = vehicleItem.VehicleInfo.plateText;

                    Cache.PersonalVehicle = new State.VehicleState(vehicle);
                    Cache.PlayerPed.Task.WarpIntoVehicle(Cache.PersonalVehicle.Vehicle, VehicleSeat.Driver);

                    Cache.Player.User.SendEvent("vehicle:log:player", vehicle.NetworkId);

                    await vehicle.FadeIn();
                    return new { success = true };
                }

                return new { success = true };


            }));
        }
    }
}
