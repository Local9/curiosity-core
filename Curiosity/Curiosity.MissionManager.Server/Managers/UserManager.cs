using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Server.Diagnostics;
using Curiosity.MissionManager.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Shared.Entity;
using Newtonsoft.Json;
using System;

namespace Curiosity.MissionManager.Server.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public override async void Begin()
        {
            EventSystem.GetModule().Attach("user:login", new AsyncEventCallback(async metadata =>
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

                // Logger.Success($"{exportResponse}");

                CuriosityUser curiosityUser = JsonConvert.DeserializeObject<CuriosityUser>($"{exportResponse}");

                Logger.Success($"[User] [{metadata.Sender}] [{curiosityUser.LatestName}#{curiosityUser.UserId}|{curiosityUser.Role}] Has successfully connected to the server");

                curiosityUser.Handle = metadata.Sender;

                PluginManager.ActiveUsers.Add(metadata.Sender, curiosityUser);

                return curiosityUser;
            }));

            Instance.EventRegistry["playerDropped"] += new Action<Player, string>(OnPlayerDropped);

            EventSystem.Attach("user:job", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                PluginManager.ActiveUsers[metadata.Sender].CurrentJob = metadata.Find<string>(0);

                return null;
            }));
        }

        static void OnPlayerDropped([FromSource]Player player, string reason)
        {
            int playerHandle = int.Parse(player.Handle);
            if (PluginManager.ActiveUsers.ContainsKey(playerHandle))
            {
                bool userRemoved = PluginManager.ActiveUsers.Remove(playerHandle);
                bool userHadMission = MissionManager.ActiveMissions.ContainsKey(playerHandle);
                bool missionRemoved = MissionManager.ActiveMissions.TryRemove(playerHandle, out MissionData old);
                Logger.Debug($"Player: {player.Name} disconnected ({reason}), UserRemoved: {userRemoved}, HadMission: {userHadMission}, MissionRemoved: {missionRemoved}");
            }
        }
    }
}
