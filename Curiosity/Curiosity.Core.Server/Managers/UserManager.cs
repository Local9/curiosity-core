using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Curiosity.Core.Server.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("user:getProfile", new AsyncEventCallback(async metadata =>
            {
                var player = PluginManager.PlayersList[metadata.Sender];
                var discordIdStr = player.Identifiers["discord"];
                var license = player.Identifiers["license"];
                ulong discordId = 0;

                if (!ulong.TryParse(discordIdStr, out discordId))
                {
                    player.Drop("Error creating login session, Discord ID not found.");
                    API.CancelEvent();
                    return null;
                }

                if (discordId == 0)
                {
                    player.Drop("Error creating login session, Discord ID not found.");
                    API.CancelEvent();
                    return null;
                }

                string exportResponse = Instance.ExportDictionary["curiosity-server"].GetUser(player.Handle);

                while (string.IsNullOrEmpty(exportResponse))
                {
                    await BaseScript.Delay(500);
                    exportResponse = Instance.ExportDictionary["curiosity-server"].GetUser(player.Handle);
                }

                CuriosityUser curiosityUser = JsonConvert.DeserializeObject<CuriosityUser>($"{exportResponse}");

                // Logger.Success($"[User] [{metadata.Sender}] [{curiosityUser.LatestName}#{curiosityUser.UserId}|{curiosityUser.Role}] Has successfully connected to the server");

                curiosityUser.Handle = metadata.Sender;

                return curiosityUser;
            }));

            EventSystem.GetModule().Attach("user:getSkills", new EventCallback(metadata =>
            {
                var player = PluginManager.PlayersList[metadata.Sender];
                string exportResponse = Instance.ExportDictionary["curiosity-server"].GetSkills(player.Handle);
                List<Skill> returnVal = JsonConvert.DeserializeObject<List<Skill>>(exportResponse);
                return returnVal;
            }));

            EventSystem.GetModule().Attach("user:getStats", new EventCallback(metadata =>
            {
                var player = PluginManager.PlayersList[metadata.Sender];
                string exportResponse = Instance.ExportDictionary["curiosity-server"].GetStats(player.Handle);
                List<Skill> returnVal = JsonConvert.DeserializeObject<List<Skill>>(exportResponse);
                return returnVal;
            }));
        }
    }
}
