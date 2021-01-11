using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Core.Server.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("user:login", new AsyncEventCallback(async metadata =>
            {
                var player = PluginManager.PlayersList[metadata.Sender];

                if (player == null)
                {
                    return null;
                }

                CuriosityUser curiosityUser = await Database.Store.UserDatabase.Get(player);

                Logger.Debug($"[User] [{metadata.Sender}] [{curiosityUser.LatestName}#{curiosityUser.UserId}] Has connected to the server");

                curiosityUser.Handle = metadata.Sender;

                PluginManager.ActiveUsers.Add(metadata.Sender, curiosityUser);

                return curiosityUser;
            }));

            EventSystem.GetModule().Attach("user:getProfile", new AsyncEventCallback(async metadata =>
            {
                var player = PluginManager.PlayersList[metadata.Sender];

                if (player == null)
                {
                    return null;
                }

                CuriosityUser curiosityUser = await Database.Store.UserDatabase.Get(player);

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

            EventSystem.GetModule().Attach("user:license:weapon", new EventCallback(metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];



                return null;
            }));

            // Native Events
            Instance.EventRegistry["playerDropped"] += new Action<Player, string>(OnPlayerDropped);

            // Exports
            Instance.ExportDictionary.Add("GetUser", new Func<string, string>((playerHandle) =>
            {
                int handle = int.Parse(playerHandle);

                return JsonConvert.SerializeObject(PluginManager.ActiveUsers[handle]);
            }));
        }

        static void OnPlayerDropped([FromSource] Player player, string reason)
        {
            int playerHandle = int.Parse(player.Handle);
            if (PluginManager.ActiveUsers.ContainsKey(playerHandle))
            {
                string message = $"Player: {player.Name} disconnected ({reason})";
                Logger.Debug(message);
                ChatManager.OnLogMessage(message);

                PluginManager.ActiveUsers.Remove(playerHandle);
            }
        }
    }
}
