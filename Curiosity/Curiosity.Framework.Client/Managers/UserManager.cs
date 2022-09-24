using Curiosity.Framework.Client.Extensions;
using Curiosity.Framework.Client.Utils;
using Curiosity.Framework.Shared.SerializedModels;
using FxEvents;
using ScaleformUI;

namespace Curiosity.Framework.Client.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public User _user => Session.User; // refactor
        
        public async override void Begin()
        {
            GameInterface.Hud.FadeOut(0);
            await LoadTransition.OnUpAsync();
            ScreenInterface.DisableHud();
            ScreenInterface.CloseLoadingScreen();
            ScreenInterface.StartLoadingMessage("PM_WAIT");
            OnRequestCharactersAsync();
        }

        public async Task OnRequestCharactersAsync()
        {
            User user = await EventDispatcher.Get<User>("user:active", Game.Player.ServerId);

            if (user is null)
            {
                Logger.Error($"No user was returned from the server.");
                return;
            }

            Session.User = user;

            CharacterCreatorManager characterCreatorManager = CharacterCreatorManager.GetModule();

            Game.Player.ChangeModel(PedHash.FreemodeMale01);
            Game.PlayerPed.SetDefaultVariation();

            Game.PlayerPed.Position = new Vector3(0, 0, 0);

            LoadTransition.OnDownAsync();
            
            GameInterface.Hud.FadeIn(1000);

            Logger.Info($"User Database: [{_user.Handle}] {_user.Username}#{_user.UserID} with {_user.Characters.Count} Character(s).");
        }
    }
}
