using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Events;
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

        public async override void Begin()
        {
            API.AddTextEntry(BLIP_PERSONAL_VEHICLE, "Personal Vehicle");
            API.AddTextEntry(BLIP_PERSONAL_TRAILER, "Personal Trailer");
            API.AddTextEntry(BLIP_PERSONAL_PLANE, "Personal Plane");
            API.AddTextEntry(BLIP_PERSONAL_BOAT, "Personal Boat");
            API.AddTextEntry(BLIP_PERSONAL_HELICOPTER, "Personal Helicopter");

            EventSystem.Attach("garage:update", new EventCallback(metadata =>
            {
                string vehicleLabel = metadata.Find<string>(0);

                Notify.Info($"Your new '{vehicleLabel}', is now ready.");

                return null;
            }));

            Instance.AttachNuiHandler("GarageVehicles", new AsyncEventCallback(async metadata =>
            {
                List<dynamic> vehicles = new List<dynamic>();

                try
                {
                    List<VehicleItem> srvVeh = await EventSystem.Request<List<VehicleItem>>("garage:get:list");

                    if (srvVeh.Count == 0)
                    {
                        Notify.Info("No vehicles returned from the Garage");
                        return vehicles;
                    }

                    if (srvVeh is null)
                    {
                        Notify.Info("No vehicles returned from the Garage");
                        return vehicles;
                    }

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
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "GetGarageVehicles");
                    return vehicles;
                }
            }));

            Instance.AttachNuiHandler("GarageSell", new AsyncEventCallback(async metadata =>
            {
                ExportMessage exportMessage = new ExportMessage();
                try
                {
                    string characterVehicleIdString = metadata.Find<string>(0);
                    int characterVehicleId = 0;

                    if (!int.TryParse(characterVehicleIdString, out characterVehicleId))
                    {
                        Logger.Debug("Vehicle information is invalid, if it happens again write up what you were doing on the forums.");
                        Notify.Error("Vehicle information is invalid, if it happens again write up what you were doing on the forums.");
                        return new { success = false };
                    }

                    Logger.Debug($"Attempting to sell Vehicle with ID {characterVehicleId}");

                    exportMessage = await EventSystem.Request<ExportMessage>("garage:sell:vehicle", characterVehicleId);
                    Logger.Debug($"Response: {exportMessage}");

                    if (!exportMessage.success)
                    {
                        Logger.Debug(exportMessage.error);
                        Notify.Error(exportMessage.error);
                    }

                    return exportMessage;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "GarageSell");
                    return exportMessage;
                }
            }));

            Instance.AttachNuiHandler("GarageVehicleRequest", new AsyncEventCallback(async metadata =>
            {
                string characterVehicleIdString = metadata.Find<string>(0);
                if (int.TryParse(characterVehicleIdString, out int characterVehicleId))
                {
                    string hash = metadata.Find<string>(1);
                    return await VehicleManager.GetModule().CreateVehicle(characterVehicleId, hash);
                }
                else
                {
                    Notify.Error($"Invalid information.");
                    return new { success = false };
                }
            }));
        }




    }
}
