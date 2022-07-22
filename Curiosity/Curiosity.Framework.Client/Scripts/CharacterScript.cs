using Curiosity.Framework.Client.Events;
using Curiosity.Framework.Shared.Models;

namespace Curiosity.Framework.Client.Scripts
{
    internal class CharacterScript : ScriptBase
    {
        static User _user = new();

        public async void Init()
        {
            User user = await ClientGateway.Get<User>("user:active", Game.Player.ServerId);
            Logger.Trace($"User: {user}");
            if (user is null)
            {
                Logger.Error($"No user was returned from the server.");
                return;
            }

            PluginManager.Instance.SoundEngine.Disable();

            Logger.Debug($"User: {_user.Username}, Characters: {_user?.Characters?.Count}");
        }
    }
}
