using Curiosity.Framework.Client.Events;
using Curiosity.Framework.Client.Extensions;
using Curiosity.Framework.Client.Utils;
using Curiosity.Framework.Shared;
using Curiosity.Framework.Shared.SerializedModels;
using ScaleformUI;
using System.Drawing;

namespace Curiosity.Framework.Client.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public User _user;
        Quaternion _cityHall = new Quaternion(-542.1675f, -216.1688f, -206.1688f, 0f);

        Quaternion _characterCreatorSpawn = new Quaternion(405.9247f, -997.2114f, -100.00024f, 86.36787f);
        Quaternion _characterCreator = new Quaternion(402.8841f, -996.4642f, -100.00024f, -185.0f);

        // cameras
        const string DEFAULT_SCRIPTED_CAMERA = "DEFAULT_SCRIPTED_CAMERA";
        Camera _mainCamera;

        // Menu Items
        UIMenu _menuBase = new UIMenu("", "", true);
        UIMenu _menuParents = new UIMenu("", "", true);
        UIMenu _menuDetails = new UIMenu("", "", true);
        UIMenu _menuAppearance = new UIMenu("", "", true);
        UIMenu _menuApparel = new UIMenu("", "", true);
        UIMenu _menuStats = new UIMenu("", "", true);

        // Lists
        List<dynamic> _arcSop = new List<dynamic> { "Standard", "High", "Low" };
        List<dynamic> _occ = new List<dynamic> { "Standard", "Great", "Tight" };
        List<dynamic> _nas = new List<dynamic> { "Standard", "Great", "Small" };

        // Menu List Items

        UIMenuListItem _mLstBrow = null;
        UIMenuListItem _mLstEyes = null;
        UIMenuListItem _mLstNose = null;
        UIMenuListItem _mLstNosePro = null;
        UIMenuListItem _mLstNosePun = null;
        UIMenuListItem _mLstCheek = null;
        UIMenuListItem _mLstCheekShape = null;
        UIMenuListItem _mLstLips = null;
        UIMenuListItem _mListJaw = null;
        UIMenuListItem _mLstChin = null;
        UIMenuListItem _mLstChinShape = null;
        UIMenuListItem _mListNeck = null;

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

            Ped playerPed = Game.PlayerPed;

            playerPed.IsPositionFrozen = false;
            playerPed.Position = new Vector3(_characterCreatorSpawn.X, _characterCreatorSpawn.Y, _characterCreatorSpawn.Z);
            playerPed.Heading = _characterCreatorSpawn.W;

            await Common.MoveToMainThread();

            playerPed.SetDefaultVariation();
            playerPed.SetRandomFacialMood();

            playerPed.IsInvincible = true;
            playerPed.IsVisible = true;
            playerPed.BlockPermanentEvents = true;

            // swap this out
            mugshotBoardAttachment.Attach(playerPed, _user, topLine: "FACE_N_CHAR");

            Instance.SoundEngine.Enable();
            await LoadTransition.OnDownAsync();

            RenderScriptCams(true, true, 0, false, false);

            _mainCamera = new Camera(CreateCam(DEFAULT_SCRIPTED_CAMERA, true));
            _mainCamera.IsActive = true;
            _mainCamera.Position = new Vector3(402.7553f, -1000.622f, -98.48412f);
            _mainCamera.Rotation = new Vector3(-6.716503f, 0f, -0.276376f);
            _mainCamera.FieldOfView = 36.95373f;
            _mainCamera.StopShaking();
            N_0xf55e4046f6f831dc(_mainCamera.Handle, 3f);
            N_0xe111a7c0d200cbc5(_mainCamera.Handle, 1f);
            SetCamDofFnumberOfLens(_mainCamera.Handle, 1.2f);
            SetCamDofMaxNearInFocusDistanceBlendLevel(_mainCamera.Handle, 1f);

            World.RenderingCamera = _mainCamera;

            Camera cam = new Camera(CreateCam(DEFAULT_SCRIPTED_CAMERA, true));
            cam.Position = new Vector3(402.7391f, -1003.981f, -98.43439f);
            cam.Rotation = new Vector3(-3.589798f, 0f, -0.276381f);
            cam.FieldOfView = 36.95373f;
            cam.StopShaking();
            N_0xf55e4046f6f831dc(cam.Handle, 7f);
            N_0xe111a7c0d200cbc5(cam.Handle, 1f);
            SetCamDofFnumberOfLens(cam.Handle, 1.2f);
            SetCamDofMaxNearInFocusDistanceBlendLevel(cam.Handle, 1f);
            cam.InterpTo(_mainCamera, 5000, 1, 1);

            Interface.Hud.FadeIn(800);
            await BaseScript.Delay(3000);

            cam.Delete();

            await playerPed.TaskWalkInToCharacterCreationRoom(GetLineupOrCreationAnimation(true, false, playerPed.Gender));

            OnCharacterCreationMenuAsync(playerPed.Gender);
        }

        public async void OnCharacterCreationMenuAsync(Gender gender)
        {
            await Interface.Hud.FadeIn(800);
            Point offset = new Point(50, 50);
            Interface.Hud.MenuPool.MouseEdgeEnabled = false;
            _menuBase = new("Character Creator", "Create a new Character", offset)
            {
                ControlDisablingEnabled = true
            };
            Interface.Hud.MenuPool.Add(_menuBase);
            UIMenuListItem mLstCharacterSex = new UIMenuListItem("Sex", new List<dynamic> { "Male", "Female" }, (int)gender, "Select character sex");
            _menuBase.AddItem(mLstCharacterSex);
            
            _menuParents = Interface.Hud.MenuPool.AddSubMenu(
                    _menuBase,
                    GetLabelText("FACE_HERI"),
                    GetLabelText("FACE_MM_H3")
                );
            _menuParents.ControlDisablingEnabled = true;
            _menuDetails = Interface.Hud.MenuPool.AddSubMenu(
                _menuBase,
                GetLabelText("FACE_FEAT"),
                GetLabelText("FACE_MM_H4")
            );
            _menuDetails.ControlDisablingEnabled = true;
            _menuAppearance = Interface.Hud.MenuPool.AddSubMenu(
                _menuBase,
                GetLabelText("FACE_APP"),
                GetLabelText("FACE_MM_H6")
            );
            _menuAppearance.ControlDisablingEnabled = true;
            _menuApparel = Interface.Hud.MenuPool.AddSubMenu(
                _menuBase,
                GetLabelText("FACE_APPA"),
                GetLabelText("FACE_APPA_H")
            );
            _menuApparel.ControlDisablingEnabled = true;
            _menuStats = Interface.Hud.MenuPool.AddSubMenu(
                _menuBase,
                GetLabelText("FACE_STATS"),
                GetLabelText("FACE_MM_H5")
            );

            InstructionalButton btnLookLeftOrRight = new InstructionalButton(
                Control.LookLeftRight,
                "Look Right/Left"
            );
            InstructionalButton btnLookLeft = new InstructionalButton(
                Control.FrontendLb,
                "Look Left"
            );
            InstructionalButton btnLookRight = new InstructionalButton(
                Control.FrontendRb,
                "Look Right"
            );
            InstructionalButton button4 = new InstructionalButton(
                InputGroup.INPUTGROUP_LOOK,
                "Change details"
            );
            InstructionalButton button5 = new InstructionalButton(
                InputGroup.INPUTGROUP_LOOK,
                "Manage Panels",
                ScaleformUI.PadCheck.Keyboard
            );
            _menuBase.InstructionalButtons.Add(btnLookRight);
            _menuBase.InstructionalButtons.Add(btnLookLeft);
            _menuParents.InstructionalButtons.Add(btnLookRight);
            _menuParents.InstructionalButtons.Add(btnLookLeft);
            _menuAppearance.InstructionalButtons.Add(btnLookRight);
            _menuAppearance.InstructionalButtons.Add(btnLookLeft);
            _menuAppearance.InstructionalButtons.Add(button5);
            _menuDetails.InstructionalButtons.Add(btnLookRight);
            _menuDetails.InstructionalButtons.Add(btnLookLeft);
            _menuDetails.InstructionalButtons.Add(button4);

            Instance.AttachTickHandler(OnCharacterCreationWarningAsync);
            Instance.AttachTickHandler(OnCharacterCreationMenuControlsAsync);

            if (!_menuBase.Visible)
                _menuBase.Visible = true;
        }

        float _gridPanelCoordX;
        float _gridPanelCoordY;
        bool _isPedLookingLeft;
        bool _isPedLookingRight;

        int _frontendLeftBumper = (int)Control.FrontendLb; // 205
        int _frontendRightBumper = (int)Control.FrontendRb; // 206

        bool IsControlLeftBumperPressed => (IsControlPressed(0, _frontendLeftBumper) || IsDisabledControlPressed(0, _frontendLeftBumper)) && IsInputDisabled(2)
            || (IsControlPressed(2, _frontendLeftBumper) || IsDisabledControlPressed(2, _frontendLeftBumper)) && !IsInputDisabled(2);

        bool IsControlRightBumperPressed => (IsControlPressed(0, _frontendRightBumper) || IsDisabledControlPressed(0, _frontendRightBumper)) && IsInputDisabled(2)
                    || (IsControlPressed(2, _frontendRightBumper) || IsDisabledControlPressed(2, _frontendRightBumper)) && !IsInputDisabled(2);

        public async Task OnCharacterCreationMenuControlsAsync()
        {
            Ped playerPed = Game.PlayerPed;
            if (_menuBase.Visible || _menuDetails.Visible || _menuAppearance.Visible || _menuParents.Visible)
            {
                if (IsControlLeftBumperPressed)
                {
                    if (!_isPedLookingLeft)
                    {
                        _isPedLookingLeft = true;
                        playerPed.TaskLookLeft(GetLineupOrCreationAnimation(true, false, playerPed.Gender));
                    }
                }
                else if (IsControlRightBumperPressed)
                {
                    if (!_isPedLookingRight)
                    {
                        _isPedLookingRight = true;
                        playerPed.TaskLookRight(GetLineupOrCreationAnimation(true, false, playerPed.Gender));
                    }
                }
                else
                {
                    if (_isPedLookingRight)
                        playerPed.TaskStopLookingRight(GetLineupOrCreationAnimation(true, false, playerPed.Gender));
                    else if (_isPedLookingLeft)
                        playerPed.TaskStopLookingLeft(GetLineupOrCreationAnimation(true, false, playerPed.Gender));
                    
                    _isPedLookingLeft = _isPedLookingRight = false;
                }
            }
            if (!IsInputDisabled(2))
            {
                // Grid Coord Requests for Facial Updates
            }
        }

        public async Task OnCharacterCreationWarningAsync()
        {
            for (int i = 0; i < 32; i++)
                Game.DisableAllControlsThisFrame(i);

            if (_menuBase.Visible && _menuBase.HasControlJustBeenPressed(UIMenu.MenuControls.Back))
            {
                Interface.Hud.MenuPool.CloseAllMenus();
                ScaleformUI.ScaleformUI.Warning.ShowWarningWithButtons(
                    "Cancel Character Creation",
                    "Are you sure you want to Cancel Character Creation?",
                    "All changes will be lost and you will be returned to character selection.",
                    new List<InstructionalButton>()
                    {
                        new InstructionalButton(Control.FrontendCancel, "No"),
                        new InstructionalButton(Control.FrontendAccept, "Yes"),
                    }
                );
                ScaleformUI.ScaleformUI.Warning.OnButtonPressed += async (action) =>
                {
                    if (action.GamepadButton == Control.FrontendCancel)
                    { 
                        // TODO: Deal with pause menu
                        Instance.DetachTickHandler(OnCharacterCreationWarningAsync);
                        Instance.DetachTickHandler(OnCharacterCreationMenuControlsAsync);

                        OnCharacterCreationMenuAsync(Game.PlayerPed.Gender);
                    }
                    else if (action.GamepadButton == Control.FrontendAccept)
                    {
                        await Interface.Hud.FadeOut(1000);

                        Instance.DetachTickHandler(OnCharacterCreationWarningAsync);
                        Instance.DetachTickHandler(OnCharacterCreationMenuControlsAsync);

                        _menuBase.Visible = false;
                        Interface.Hud.MenuPool.CloseAllMenus();
                        
                        RenderScriptCams(false, false, 300, false, false);
                    }
                };
            }
        }

        string GetLineupOrCreationAnimation(bool lineup, bool alternateAnimation, Gender gender)
        {
            if (lineup)
                return gender == Gender.Male ? "mp_character_creation@customise@male_a" : "mp_character_creation@customise@female_a";

            if (!lineup && alternateAnimation)
                return gender == Gender.Male ? "mp_character_creation@lineup@male_b" : "mp_character_creation@lineup@female_b";
            
            if (!lineup)
                return gender == Gender.Male ? "mp_character_creation@lineup@male_a" : "mp_character_creation@lineup@female_a";

            return "mp_character_creation@lineup@male_a";
        }
    }
}
