using CitizenFX.Core;
using Curiosity.Global.Shared.net;
using System;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle
{
    class DeleteVehicle
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Player:Vehicle:Delete", new Action<string>(OnDelete));
        }

        static void OnDelete(string encodedNetworkId)
        {
            try
            {
                if (string.IsNullOrEmpty(encodedNetworkId)) return;

                string decryptedNetworkId = Encode.BytesToStringConverted(System.Convert.FromBase64String(encodedNetworkId));

                int vehicleId = NetworkGetEntityFromNetworkId(int.Parse(decryptedNetworkId));

                if (vehicleId > 0)
                {

                    if (DoesEntityExist(vehicleId))
                    {
                        Vehicle veh = new Vehicle(vehicleId);

                        Blip blip = veh.AttachedBlip;
                        if (blip.Exists())
                            blip.Delete();

                        if (Client.CurrentVehicle.Handle == veh.Handle)
                            Client.CurrentVehicle = null;

                        FreezeEntityPosition(vehicleId, true);
                        SetEntityAsMissionEntity(vehicleId, false, false);
                        NetworkFadeOutEntity(vehicleId, true, false);
                        SetEntityCoords(vehicleId, -2000f, -6000f, 0f, false, false, false, true);
                        SetEntityAsNoLongerNeeded(ref vehicleId);
                        DeleteEntity(ref vehicleId);
                    }

                    //if (vehicle.Exists())
                    //{
                    //    if (vehicle.AttachedBlip.Exists())
                    //    {
                    //        vehicle.AttachedBlip.Delete();
                    //    }

                    //    if (NetworkRequestControlOfEntity(vehicle.Handle))
                    //    {
                    //        vehicle.IsPersistent = false;
                    //        vehicle.IsPositionFrozen = false;
                    //        vehicle.Position = new Vector3(-2000f, -6000f, 0f);

                    //        int objectHandle = vehicle.Handle;
                    //        int objectHandleToDelete = vehicle.Handle;

                    //        SetEntityAsNoLongerNeeded(ref objectHandle);
                    //        DeleteEntity(ref objectHandleToDelete);

                    //        if (vehicle.Exists())
                    //            vehicle.Delete();
                    //    }

                    //    NetworkFadeOutEntity(vehicleId, true, false);
                    //    vehicle.IsEngineRunning = false;
                    //    vehicle.Delete();
                    //    vehicle.MarkAsNoLongerNeeded();

                    //    if (vehicle.Exists())
                    //    {
                    //        DeleteEntity(ref vehicleId);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnDelete -> {ex.Message}");
            }
        }
    }
}
