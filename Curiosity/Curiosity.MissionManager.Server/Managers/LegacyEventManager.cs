using CitizenFX.Core;
using Curiosity.MissionManager.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;

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
            EventSystem.GetModule().Attach("vehicle:repair", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;

                string exportResponse = Instance.ExportDictionary["curiosity-server"].GetUser(senderHandle);

                while (string.IsNullOrEmpty(exportResponse))
                {
                    await BaseScript.Delay(500);
                    exportResponse = Instance.ExportDictionary["curiosity-server"].GetUser(senderHandle);
                }

                CuriosityUser curiosityUser = JsonConvert.DeserializeObject<CuriosityUser>($"{exportResponse}");

                if (PluginManager.ActiveUsers.TryUpdate(senderHandle, curiosityUser, curiosityUser))
                {
                    if (curiosityUser.Wallet < 100)
                    {

                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                return true;
            }));
        }
    }
}
