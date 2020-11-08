﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Server.Diagnostics;
using Curiosity.MissionManager.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.MissionManager.Server.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public override void Begin()
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

                CuriosityUser curiosityUser = await Database.Store.UserDatabase.Get(player, discordId);

                Logger.Info($"[User] [{metadata.Sender}] [{curiosityUser.LatestName}#{curiosityUser.UserId}] Has connected to the server");

                curiosityUser.Handle = metadata.Sender;

                PluginManager.ActiveUsers.Add(metadata.Sender, curiosityUser);

                return curiosityUser;
            }));

            Instance.EventRegistry["playerDropped"] += new Action<Player, string>(OnPlayerDropped);
        }

        static void OnPlayerDropped([FromSource]Player player, string reason)
        {
            int playerHandle = int.Parse(player.Handle);
            if (PluginManager.ActiveUsers.ContainsKey(playerHandle))
            {
                Logger.Info($"Player: {player.Name} disconnected ({reason})");
                PluginManager.ActiveUsers.Remove(playerHandle);
            }
        }
    }
}
