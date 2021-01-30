using CitizenFX.Core;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;

namespace Curiosity.Core.Server.Managers
{
    public class EntityManager : Manager<EntityManager>
    {
        public static EntityManager EntityInstance;

        public override void Begin()
        {
            EntityInstance = this;

            EventSystem.GetModule().Attach("entity:delete", new EventCallback(metadata =>
            {
                int networkId = metadata.Find<int>(0);

                NetworkDeleteEntity(networkId);

                return null;
            }));
        }

        public void NetworkDeleteEntity(int networkId)
        {
            foreach (Player player in PluginManager.PlayersList)
            {
                int playerHandle = int.Parse(player.Handle);
                EventSystem.GetModule().Send("entity:deleteFromWorld", playerHandle, networkId);
            }
        }
    }
}
