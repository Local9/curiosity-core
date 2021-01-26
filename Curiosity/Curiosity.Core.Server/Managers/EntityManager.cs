using CitizenFX.Core;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;

namespace Curiosity.Core.Server.Managers
{
    public class EntityManager : Manager<EntityManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("entity:s:delete", new EventCallback(metadata =>
            {
                int networkId = metadata.Find<int>(0);

                foreach(Player player in PluginManager.PlayersList)
                {
                    int playerHandle = int.Parse(player.Handle);
                    EventSystem.GetModule().Send("entity:c:delete", playerHandle, networkId);
                }

                return null;
            }));
        }
    }
}
