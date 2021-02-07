using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;

namespace Curiosity.Core.Server.Managers
{
    public class EntityManager : Manager<EntityManager>
    {
        public static EntityManager EntityInstance;
        const string ENTITY_VEHICLE_DELETE = "delete:vehicle";
        const string ENTITY_VEHICLE_REPAIR = "repair:vehicle";

        public override void Begin()
        {
            EntityInstance = this;

            EventSystem.GetModule().Attach("delete:entity", new EventCallback(metadata =>
            {
                int networkId = metadata.Find<int>(0);

                NetworkDeleteEntity(networkId);

                return null;
            }));

            EventSystem.GetModule().Attach("entity:damage", new EventCallback(metadata =>
            {

                //BaseScript.TriggerClientEvent("c:mm:damage", networkId, x, y, z, force, radius, fromEntity, numberOfHits);

                int networkId = metadata.Find<int>(0);
                float x = metadata.Find<float>(1);
                float y = metadata.Find<float>(2);
                float z = metadata.Find<float>(3);
                float force = metadata.Find<float>(4);
                float radius = metadata.Find<float>(5);
                int fromEntity = metadata.Find<int>(6);
                int numberOfHits = metadata.Find<int>(7);

                

                return null;
            }));

            EventSystem.GetModule().Attach(ENTITY_VEHICLE_DELETE, new EventCallback(metadata =>
            {
                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];

                Vector3 position = Vector3.Zero;
                position.X = metadata.Find<float>(0);
                position.Y = metadata.Find<float>(1);
                position.Z = metadata.Find<float>(2);

                bool result = ConfigManager.ConfigInstance.IsNearLocation(position, ENTITY_VEHICLE_DELETE);

                Logger.Debug($"{user.LatestName} delete vehicle; {result}");

                if (result)
                    user.Send(ENTITY_VEHICLE_DELETE);

                return null;
            }));

            EventSystem.GetModule().Attach(ENTITY_VEHICLE_REPAIR, new AsyncEventCallback(async metadata =>
            {
                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];

                Vector3 position = Vector3.Zero;
                position.X = metadata.Find<float>(0);
                position.Y = metadata.Find<float>(1);
                position.Z = metadata.Find<float>(2);

                bool result = ConfigManager.ConfigInstance.IsNearLocation(position, ENTITY_VEHICLE_REPAIR);

                Logger.Debug($"{user.LatestName} repair vehicle; {result}");

                if (result)
                {
                    if (user.Character.Cash < 100)
                    {
                        return null;
                    }

                    user.Character.Cash = await Database.Store.BankDatabase.Adjust(user.Character.CharacterId, -100);

                    user.Send(ENTITY_VEHICLE_REPAIR);
                }

                return null;
            }));
        }

        public void NetworkDeleteEntity(int networkId)
        {

            int vehId = API.NetworkGetEntityFromNetworkId(networkId);

            if (API.DoesEntityExist(vehId))
                API.DeleteEntity(vehId);

            //foreach (Player player in PluginManager.PlayersList)
            //{
            //    int playerHandle = int.Parse(player.Handle);
            //    EventSystem.GetModule().Send("entity:deleteFromWorld", playerHandle, networkId);
            //}
        }
    }
}
