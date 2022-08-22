﻿using CitizenFX.Core.UI;
using Curiosity.Framework.Client.Events;
using Curiosity.Framework.Client.Utils;
using Curiosity.Framework.Shared.Models;

namespace Curiosity.Framework.Client.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public User _user;

        public async override void Begin()
        {
            await BaseScript.Delay(5000);
            ScreenInterface.StartLoadingMessage("PM_WAIT");
            await BaseScript.Delay(5000);

            User user = await ClientGateway.Get<User>("user:active", Game.Player.ServerId);
            Logger.Trace($"User: {user}");
            if (user is null)
            {
                Logger.Error($"No user was returned from the server.");
                return;
            }

            PluginManager.Instance.SoundEngine.Disable();

            _user = user;

            Screen.LoadingPrompt.Hide();
            Logger.Debug($"User: {_user.Username}");
        }
    }
}
