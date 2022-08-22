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

        Quaternion _characterCreatorSpawn = new Quaternion(405.9247f, -997.2114f, -100.00024f, 86.36787f);
        Quaternion _characterCreator = new Quaternion(402.8664f, -996.4108f, -100.00027f, -185.0f);

        public static RotatablePosition[] _cameraViews { get; } =
        {
            new RotatablePosition(402.8294f, -1002.45f, -98.80403f, 357.6219f, -7f, 0f),
            new RotatablePosition(402.8294f, -998.8467f, -98.80403f, 357.1697f, -7f, 0f),
            new RotatablePosition(402.8294f, -997.967f, -98.35f, 357.1697f, -7f, 0f)
        };

        AnimationQueue _animationQueue;
        MugshotBoardAttachment mugshotBoardAttachment = new();

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

            Game.PlayerPed.IsPositionFrozen = false;
            Game.PlayerPed.Position = new Vector3(_characterCreatorSpawn.X, _characterCreatorSpawn.Y, _characterCreatorSpawn.Z);
            Game.PlayerPed.Heading = _characterCreatorSpawn.W;

            await Common.MoveToMainThread();

            Game.PlayerPed.SetDefaultVariation();

            Game.PlayerPed.IsInvincible = true;

            Game.PlayerPed.IsVisible = true;

            mugshotBoardAttachment.Attach(Game.PlayerPed, _user);

            Instance.SoundEngine.Enable();
            await LoadTransition.OnDownAsync();

            _user.CameraQueue.View(new CameraBuilder()
                .WithMotionBlur(.5f)
                .WithInterpolation(_cameraViews[0], _cameraViews[1], 5000)
                );

            _animationQueue = new AnimationQueue(Game.PlayerPed.Handle);
            await _animationQueue.PlayDirectInQueue(new AnimationBuilder()
                .Select("mp_character_creation@customise@male_a", "intro")
                );
            
            _animationQueue.AddToQueue(new AnimationBuilder()
                .Select("mp_character_creation@customise@male_a", "loop")
                .WithFlags(AnimationFlags.Loop)
                .SkipTask()
                ).PlayQueue();

            var gameTime = GetGameTimer();
            while (true)
            {
                await BaseScript.Delay(100);
                if (GetGameTimer() - gameTime > 3000)
                {
                    break;
                }
            }

            DisplayHud(false);
            DisplayRadar(false);
        }
    }
}
