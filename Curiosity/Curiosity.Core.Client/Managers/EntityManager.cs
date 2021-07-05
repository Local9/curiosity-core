using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;

namespace Curiosity.Core.Client.Managers
{
    public class EntityManager : Manager<EntityManager>
    {
        bool IsBike => (Cache.PersonalVehicle == null) ? false : Cache.PersonalVehicle.Vehicle.ClassType == VehicleClass.Motorcycles || Cache.PersonalVehicle.Vehicle.ClassType == VehicleClass.Cycles;

        public override void Begin()
        {
            EventSystem.Attach("entity:nuke", new EventCallback(metadata =>
            {
                Vector3 pos = Cache.PlayerPed.Position;
                API.ClearAreaOfEverything(pos.X, pos.Y, pos.Z, 500f, false, false, false, false);
                return null;
            }));

            EventSystem.Attach("entity:deleteFromWorld", new AsyncEventCallback(async metadata =>
            {
                int networkId = metadata.Find<int>(0);

                if (!API.NetworkDoesEntityExistWithNetworkId(networkId)) return null;

                int entityHandle = API.NetworkGetEntityFromNetworkId(networkId);

                if (API.DoesEntityExist(entityHandle))
                {
                    int entityType = API.GetEntityType(entityHandle);

                    switch(entityType)
                    {
                        case 1:
                            Ped ped = new Ped(entityHandle);
                            if (ped.Exists())
                            {
                                if (ped.AttachedBlip is not null)
                                    ped.AttachedBlip.Delete();

                                int pedBlip = API.GetBlipFromEntity(ped.Handle);
                                API.RemoveBlip(ref pedBlip);

                                RemoveEntityBlip(ped);

                                await ped.FadeOut();
                                ped.MarkAsNoLongerNeeded();
                                ped.Delete();
                            }
                            break;
                        case 2:
                            Vehicle vehicle = new Vehicle(entityHandle);
                            if (vehicle.Exists())
                            {
                                if (vehicle.PassengerCount > 0)
                                {
                                    foreach(Ped p in vehicle.Passengers)
                                    {
                                        p.Task.WarpOutOfVehicle(vehicle);
                                    }
                                    
                                    while (vehicle.PassengerCount > 0)
                                    {
                                        await BaseScript.Delay(100);
                                    }
                                }

                                if (networkId == Cache.PersonalBoat.Vehicle.NetworkId)
                                    Cache.PersonalBoat = null;

                                if (networkId == Cache.PersonalTrailer.Vehicle.NetworkId)
                                    Cache.PersonalTrailer = null;

                                if (networkId == Cache.PersonalVehicle.Vehicle.NetworkId)
                                    Cache.PersonalVehicle = null;

                                if (networkId == Cache.PersonalPlane.Vehicle.NetworkId)
                                    Cache.PersonalPlane = null;

                                if (networkId == Cache.PersonalHelicopter.Vehicle.NetworkId)
                                    Cache.PersonalHelicopter = null;

                                if (vehicle.AttachedBlip is not null)
                                    vehicle.AttachedBlip.Delete();

                                int vehicleBlip = API.GetBlipFromEntity(vehicle.Handle);
                                API.RemoveBlip(ref vehicleBlip);

                                RemoveEntityBlip(vehicle);

                                await vehicle.FadeOut();

                                vehicle.Position = new Vector3(10000, 10000, 0);
                                await BaseScript.Delay(1500);

                                vehicle.MarkAsNoLongerNeeded();
                                vehicle.Delete();
                            }
                            break;
                        default:
                            API.SetEntityAsMissionEntity(entityHandle, false, false);
                            API.DeleteEntity(ref entityHandle);
                            break;
                    }
                }

                return null;
            }));
        }

        public void RemoveEntityBlip(Entity ent)
        {
            int pedBlipHandle = ent.State.Get($"{StateBagKey.BLIP_ID}") ?? 0;
            if (pedBlipHandle > 0)
            {
                if (API.DoesBlipExist(pedBlipHandle))
                    API.RemoveBlip(ref pedBlipHandle);
            }
        }
    }
}
