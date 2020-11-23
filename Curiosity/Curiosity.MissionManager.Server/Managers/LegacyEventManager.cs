using CitizenFX.Core;
using Curiosity.MissionManager.Server.Events;
using Curiosity.Systems.Library.Events;

namespace Curiosity.MissionManager.Server.Managers
{
    public class LegacyEventManager : Manager<LegacyEventManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("vehicle:delete", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;
                int networkId = metadata.Find<int>(0);

                BaseScript.TriggerClientEvent("curiosity:Player:Vehicle:Delete:NetworkId", networkId);

                return true;
            }));
        }
    }
}
