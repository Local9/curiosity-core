using Curiosity.Framework.Client.Events;
using Curiosity.Framework.Client.Extensions;
using Curiosity.Framework.Client.Utils;
using Curiosity.Framework.Shared;
using Curiosity.Framework.Shared.Models;

namespace Curiosity.Framework.Client.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public User _user;
        Quaternion _cityHall = new Quaternion(-542.1675f, -216.1688f, -206.1688f, 0f);

        Quaternion _characterCreator = new Quaternion(402.8664f, -996.4108f, -100.00027f, -185.0f);
        Quaternion _characterCreatorSpawn = new Quaternion(402.8841f, -996.4642f, -99.00024f, 86.36787f);

        Camera _camera;
        Vector3 _cameraStartPosition = new Vector3(402.8664f, -997.5515f, -98.5f);
        Vector3 _cameraStartRotation = new Vector3(-185f, 0f, 0f);
        float _cameraStartFov = 50.0f;
        Vector3 _cameraPointAtCoord = new Vector3(402.8664f, -996.4108f, -98.5f);

        public async override void Begin()
        {
            ScreenInterface.StartLoadingMessage("PM_WAIT");
            await BaseScript.Delay(5000);

            ShutdownLoadingScreen();
            ShutdownLoadingScreenNui();

            ScreenInterface.StartLoadingMessage("PM_WAIT");
            OnRequestCharactersAsync();

            DisableMultiplayerChat(true);
        }

        public async Task OnRequestCharactersAsync()
        {
            await LoadTransition.OnUpAsync();

            User user = await ClientGateway.Get<User>("user:active", Game.Player.ServerId);

            if (user is null)
            {
                Logger.Error($"No user was returned from the server.");
                return;
            }

            _user = user;

            // lets act as if we don't have a character for now

            OnCreateNewCharacter();

            // goto character selection
            // if new character make one
            // else load selected character

            Logger.Trace($"User Database: [{user.Handle}] {user.Username}#{user.UserID} with {user.Characters.Count} Character(s).");
        }

        public async Task OnCreateNewCharacter()
        {
            ScreenInterface.StartLoadingMessage("PM_WAIT");

            Model model = "mp_m_freemode_01";
            await Game.Player.ChangeModel(model);
            model.MarkAsNoLongerNeeded();

            NetworkResurrectLocalPlayer(_characterCreatorSpawn.X, _characterCreatorSpawn.Y, _characterCreatorSpawn.Z, _characterCreatorSpawn.W, true, false);

            Game.PlayerPed.Position = new Vector3(_characterCreatorSpawn.X, _characterCreatorSpawn.Y, _characterCreatorSpawn.Z);
            Game.PlayerPed.Heading = _characterCreatorSpawn.W;

            await Common.MoveToMainThread();

            Game.PlayerPed.SetDefaultVariation();

            Game.PlayerPed.IsInvincible = true;
            Game.PlayerPed.IsPositionFrozen = true;

            Game.PlayerPed.IsVisible = true;

            Instance.SoundEngine.Enable();
            await LoadTransition.OnDownAsync();

            _camera = World.CreateCamera(_cameraStartPosition, _cameraStartRotation, _cameraStartFov);
            _camera.PointAt(_cameraPointAtCoord);
            _camera.IsActive = true;
            RenderScriptCams(true, false, 0, true, false);

            DisplayHud(false);
            DisplayRadar(false);
        }
    }
}
