using CitizenFX.Core.UI;
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
            OnRequestCharactersAsync();
        }

        public async Task OnRequestCharactersAsync()
        {
            User user = await ClientGateway.Get<User>("user:active", Game.Player.ServerId);

            if (user is null)
            {
                Logger.Error($"No user was returned from the server.");
                return;
            }

            PluginManager.Instance.SoundEngine.Disable();

            _user = user;

            Screen.LoadingPrompt.Hide();
            Logger.Trace($"User Database: [{user.Handle}] {user.Username}#{user.UserID} with {user.Characters.Count} Character(s).");
        }
    }
}
