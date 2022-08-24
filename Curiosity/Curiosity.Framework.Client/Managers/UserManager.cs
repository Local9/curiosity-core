using Curiosity.Framework.Client.Events;
using Curiosity.Framework.Client.Extensions;
using Curiosity.Framework.Client.Utils;
using Curiosity.Framework.Shared;
using Curiosity.Framework.Shared.Extensions;
using Curiosity.Framework.Shared.SerializedModels;

namespace Curiosity.Framework.Client.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public User _user;
        Quaternion _cityHall = new Quaternion(-542.1675f, -216.1688f, -206.1688f, 0f);

        Quaternion _characterCreatorSpawn = new Quaternion(405.9247f, -997.2114f, -100.00024f, 86.36787f);
        Quaternion _characterCreator = new Quaternion(402.8841f, -996.4642f, -100.00024f, -185.0f);

        public static RotatablePosition[] _cameraViews { get; } =
        {
            new RotatablePosition(402.7553f, -1000.622f, -98.48412f, -6.716503f, 0f, -0.276376f),
            new RotatablePosition(402.7391f, -1003.981f, -98.43439f, -3.589798f, 0f, -0.276381f),
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

        void SetupCharacterCreator()
        {
            RequestAnimDict("mp_character_creation@lineup@male_a");
            RequestAnimDict("mp_character_creation@lineup@male_b");
            RequestAnimDict("mp_character_creation@lineup@female_a");
            RequestAnimDict("mp_character_creation@lineup@female_b");
            RequestAnimDict("mp_character_creation@customise@male_a");
            RequestAnimDict("mp_character_creation@customise@female_a");

            if (N_0x544810ed9db6bbe6() != true)
                return;
            
            RequestScriptAudioBank("Mugshot_Character_Creator", false);
            RequestScriptAudioBank("DLC_GTAO/MUGSHOT_ROOM", false);
        }

        async Task OnLoadCharacterCreatorInteriorAsync()
        {
            if (IsValidInterior(94722))
                LoadInterior(94722);

            while (!IsInteriorReady(94722))
                await BaseScript.Delay(1000);
        }

        public async Task OnCreateNewCharacter()
        {
            ScreenInterface.StartLoadingMessage("PM_WAIT");

            await OnLoadCharacterCreatorInteriorAsync();
            SetupCharacterCreator();

            DisplayHud(false);
            DisplayRadar(false);

            _user.ActiveCharacter = new Character();

            Model model = "mp_m_freemode_01";
            await Game.Player.ChangeModel(model);
            model.MarkAsNoLongerNeeded();

            NetworkResurrectLocalPlayer(_characterCreatorSpawn.X, _characterCreatorSpawn.Y, _characterCreatorSpawn.Z, _characterCreatorSpawn.W, true, false);

            Game.PlayerPed.IsPositionFrozen = false;
            Game.PlayerPed.Position = new Vector3(_characterCreatorSpawn.X, _characterCreatorSpawn.Y, _characterCreatorSpawn.Z);
            Game.PlayerPed.Heading = _characterCreatorSpawn.W;

            await Common.MoveToMainThread();

            Game.PlayerPed.SetDefaultVariation();
            Game.PlayerPed.SetRandomFacialMood();

            Game.PlayerPed.IsInvincible = true;
            Game.PlayerPed.IsVisible = true;
            Game.PlayerPed.BlockPermanentEvents = true;

            mugshotBoardAttachment.Attach(Game.PlayerPed, _user, topLine: "FACE_N_CHAR");

            Instance.SoundEngine.Enable();
            await LoadTransition.OnDownAsync();

            _user.CameraQueue.View(new CameraBuilder()
                .WithMotionBlur(.5f)
                .WithInterpolation(_cameraViews[0], _cameraViews[1], 5000)
                .WithFieldOfView(36.95373f)
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

            if (Game.PlayerPed.IsInRangeOf(_characterCreator.AsVector(), 1f))
            {
                Game.PlayerPed.Position = _characterCreator.AsVector();
                Game.PlayerPed.Heading = _characterCreator.W;
            }

            while (!_user.ActiveCharacter.IsRegistered)
            {
                await BaseScript.Delay(100);
            }
        }
    }
}
