using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Events;

namespace Curiosity.Core.Client.Managers
{
    public class EntityManager : Manager<EntityManager>
    {
        bool IsBike => (Cache.PersonalVehicle == null) ? false : Cache.PersonalVehicle.Vehicle.ClassType == VehicleClass.Motorcycles || Cache.PersonalVehicle.Vehicle.ClassType == VehicleClass.Cycles;

        public override void Begin()
        {
            EventSystem.Attach("entity:player:vehicle", new AsyncEventCallback(async metadata =>
            {
                int vehId = API.NetworkGetEntityFromNetworkId(metadata.Find<int>(0));

                if (!API.DoesEntityExist(vehId)) return null;

                if (Cache.PersonalVehicle != null)
                {
                    if (Cache.PersonalVehicle.Vehicle.Exists())
                    {
                        EventSystem.GetModule().Send("entity:delete", Cache.PersonalVehicle.Vehicle.NetworkId);

                        await Cache.PersonalVehicle.Vehicle.FadeOut();
                        Cache.PersonalVehicle.Vehicle.Delete();
                        Cache.PersonalVehicle = null;
                    }
                }

                Vehicle vehicle = new Vehicle(vehId);

                await vehicle.FadeIn();

                Cache.PersonalVehicle = new State.VehicleState(vehicle);

                if (Cache.PersonalVehicle.Vehicle.AttachedBlip != null)
                {
                    Blip b = Cache.PersonalVehicle.Vehicle.AttachBlip();
                    b.Sprite = (ScreenInterface.VehicleClassBlips.ContainsKey(Cache.PersonalVehicle.Vehicle.ClassType)) ? ScreenInterface.VehicleClassBlips[Cache.PersonalVehicle.Vehicle.ClassType] : BlipSprite.PersonalVehicleCar;
                    b.Scale = 0.85f;
                    b.Color = BlipColor.White;
                    b.Priority = 10;
                    b.Name = "Personal Vehicle";
                }

                Cache.PlayerPed.Task.WarpIntoVehicle(Cache.PersonalVehicle.Vehicle, VehicleSeat.Driver);

                Cache.Player.User.SendEvent("vehicle:log:player", vehicle.NetworkId);

                return null;
            }));

            EventSystem.Attach("entity:player:trailer", new AsyncEventCallback(async metadata =>
            {
                int vehId = API.NetworkGetEntityFromNetworkId(metadata.Find<int>(0));

                if (!API.DoesEntityExist(vehId)) return null;

                if (Cache.PersonalTrailer != null)
                {
                    if (Cache.PersonalTrailer.Exists())
                    {
                        EventSystem.GetModule().Send("entity:delete", Cache.PersonalTrailer.NetworkId);

                        await Cache.PersonalTrailer.FadeOut();
                        Cache.PersonalTrailer.Delete();
                    }
                }

                Vehicle vehicle = new Vehicle(vehId);

                await vehicle.FadeIn();

                Cache.PersonalTrailer = vehicle;

                if (Cache.PersonalTrailer.AttachedBlip != null)
                {
                    Blip b = Cache.PersonalTrailer.AttachBlip();
                    b.Sprite = (BlipSprite)479;
                    b.Scale = 0.85f;
                    b.Color = BlipColor.White;
                    b.Priority = 10;
                    b.Name = "Personal Trailer";
                }

                Cache.Player.User.SendEvent("vehicle:log:player:trailer", vehicle.NetworkId);

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
                                await ped.FadeOut();
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

                                await vehicle.FadeOut();

                                vehicle.Position = new Vector3(10000, 10000, 0);
                                await BaseScript.Delay(1500);

                                vehicle.Delete();
                            }
                            break;
                        default:
                            API.DeleteEntity(ref entityHandle);
                            break;
                    }
                }

                return null;
            }));
        }
    }
}
