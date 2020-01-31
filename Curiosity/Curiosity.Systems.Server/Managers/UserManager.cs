using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Events;

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



                CuriosityUser curiosityUser = await MySQL.Store.UserDatabase.Get(license, player, discordId);

                Logger.Info($"[User] [{curiosityUser.UserId}] [{curiosityUser.LastName}] Has connected to the server");

                Curiosity.ActiveUsers.Add(curiosityUser);

                return curiosityUser;
            }));
        }
    }
}
