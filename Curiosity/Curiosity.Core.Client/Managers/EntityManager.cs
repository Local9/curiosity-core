using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Extensions;
using Curiosity.Systems.Library.Events;

namespace Curiosity.Core.Client.Managers
{
    public class EntityManager : Manager<EntityManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("entity:player:vehicle", new AsyncEventCallback(async metadata =>
            {
                int vehId = API.NetworkGetEntityFromNetworkId(metadata.Find<int>(0));

                if (!API.DoesEntityExist(vehId)) return null;

                Vehicle vehicle = new Vehicle(vehId);

                await vehicle.FadeIn();

                Cache.PersonalVehicle = vehicle;
                Game.PlayerPed.Task.WarpIntoVehicle(Cache.PersonalVehicle, VehicleSeat.Driver);

                return Cache.PersonalVehicle.NetworkId;
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
