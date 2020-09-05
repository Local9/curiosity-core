using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Events;
using System;

namespace Curiosity.Systems.Server.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("user:login", new AsyncEventCallback(async metadata =>
            {
                var player = CuriosityPlugin.PlayersList[metadata.Sender];
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

                CuriosityPlugin.ActiveUsers.Add(metadata.Sender, curiosityUser);

                return curiosityUser;
            }));

            Curiosity.EventRegistry["playerDropped"] += new Action<Player, string>(OnPlayerDropped);
        }

        static void OnPlayerDropped([FromSource]Player player, string reason)
        {
            int playerHandle = int.Parse(player.Handle);
            if (CuriosityPlugin.ActiveUsers.ContainsKey(playerHandle))
            {
                Logger.Info($"Player: {player.Name} disconnected ({reason})");
                CuriosityPlugin.ActiveUsers.Remove(playerHandle);
            }
        }
    }
}
