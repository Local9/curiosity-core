using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.Systems.Library.Events;

namespace Curiosity.MissionManager.Client.Managers
{
    public class EntityManager : Manager<EntityManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("entity:c:delete", new AsyncEventCallback(async metadata =>
            {
                int networkId = metadata.Find<int>(0);
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
                                await vehicle.FadeOut();
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
