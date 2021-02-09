using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Core.Client.Interface;

namespace Curiosity.Core.Client.Managers
{
    public class EntityManager : Manager<EntityManager>
    {
        bool IsBike => (Cache.PersonalVehicle == null) ? false : Cache.PersonalVehicle.ClassType == VehicleClass.Motorcycles || Cache.PersonalVehicle.ClassType == VehicleClass.Cycles;

        public override void Begin()
        {
            EventSystem.Attach("entity:player:vehicle", new AsyncEventCallback(async metadata =>
            {
                int vehId = API.NetworkGetEntityFromNetworkId(metadata.Find<int>(0));

                if (!API.DoesEntityExist(vehId)) return null;

                if (Cache.PersonalVehicle != null)
                {
                    if (Cache.PersonalVehicle.Exists())
                    {
                        EventSystem.GetModule().Send("entity:delete", Cache.PersonalVehicle.NetworkId);

                        await Cache.PersonalVehicle.FadeOut();
                        Cache.PersonalVehicle.Delete();
                    }
                }

                Vehicle vehicle = new Vehicle(vehId);

                await vehicle.FadeIn();

                Cache.PersonalVehicle = vehicle;

                if (Cache.PersonalVehicle.AttachedBlip != null)
                {
                    Blip b = Cache.PersonalVehicle.AttachBlip();
                    b.Sprite = (ScreenInterface.VehicleClassBlips.ContainsKey(Cache.PersonalVehicle.ClassType)) ? ScreenInterface.VehicleClassBlips[Cache.PersonalVehicle.ClassType] : BlipSprite.PersonalVehicleCar;
                    b.Scale = 0.85f;
                    b.Color = BlipColor.White;
                    b.Priority = 10;
                    b.Name = "Personal Vehicle";
                }

                Game.PlayerPed.Task.WarpIntoVehicle(Cache.PersonalVehicle, VehicleSeat.Driver);

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
