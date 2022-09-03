using CitizenFX.Core.UI;
using Curiosity.Framework.Client.Datasets;
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
        Ped _playerPed = Game.PlayerPed;
        Quaternion _cityHall = new Quaternion(-542.1675f, -216.1688f, -206.1688f, 0f);

        Quaternion _characterCreatorSpawn = new Quaternion(405.9247f, -997.2114f, -100.00024f, 86.36787f);

        // Character Data
        CharacterSkin _characterSkin;

        // cameras
        const string DEFAULT_SCRIPTED_CAMERA = "DEFAULT_SCRIPTED_CAMERA";
        Camera _mainCamera;

        // Menu Items
        UIMenu _menuBase = new UIMenu("", "", true);
        UIMenu _menuParents = new UIMenu("", "", true);
        UIMenu _menuDetails = new UIMenu("", "", true);
        UIMenu _menuAppearance = new UIMenu("", "", true);
        UIMenu _menuApparel = new UIMenu("", "", true);
        UIMenu _menuAdvancedApparel = new UIMenu("", "", true);
        UIMenu _menuStats = new UIMenu("", "", true);

        // Lists
        List<dynamic> _lstParentMother = CharacterCreatorData.FacesMother;
        List<dynamic> _lstParentFather = CharacterCreatorData.FacesFather;

        // Menu List Items

        UIMenuListItem _mLstEyeBrowProfile = null;
        UIMenuListItem _mLstEyes = null;
        UIMenuListItem _mLstNose = null;
        UIMenuListItem _mLstNoseProfile = null;
        UIMenuListItem _mLstNoseTip = null;
        UIMenuListItem _mLstCheekBones = null;
        UIMenuListItem _mLstCheekShape = null;
        UIMenuListItem _mLstLipThickness = null;
        UIMenuListItem _mLstJawProfile = null;
        UIMenuListItem _mLstChinProfile = null;
        UIMenuListItem _mLstChinShape = null;
        UIMenuListItem _mLstNeckThickness = null;

        // heritageWindow
        UIMenuHeritageWindow _heritageWindow;
        UIMenuListItem _mliParentMother;
        UIMenuListItem _mliParentFather;
        UIMenuSliderItem _msiResemblance;
        UIMenuSliderItem _msiSkinBlend;

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

            CreateCharacterClass();

            OnCreateNewCharacter(_characterSkin);

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

            if (_characterSkin is null)
                CreateCharacterClass();
        }

        async Task OnLoadCharacterCreatorInteriorAsync()
        {
            if (IsValidInterior(94722))
                LoadInterior(94722);

            while (!IsInteriorReady(94722))
                await BaseScript.Delay(1000);
        }

        public async Task OnCreateNewCharacter(CharacterSkin characterSkin, bool reload = false)
        {
            ScreenInterface.StartLoadingMessage("PM_WAIT");

            await OnLoadCharacterCreatorInteriorAsync();
            SetupCharacterCreator();

            DisplayHud(false);
            DisplayRadar(false);

            _user.ActiveCharacter = new Character();

            NetworkResurrectLocalPlayer(_characterCreatorSpawn.X, _characterCreatorSpawn.Y, _characterCreatorSpawn.Z, _characterCreatorSpawn.W, true, false);

            _playerPed = Game.PlayerPed;

            _playerPed.Position = new Vector3(_characterCreatorSpawn.X, _characterCreatorSpawn.Y, _characterCreatorSpawn.Z);
            _playerPed.Heading = _characterCreatorSpawn.W;

            await Common.MoveToMainThread();

            _playerPed.SetDefaultVariation();
            _playerPed.SetRandomFacialMood();

            _playerPed.IsInvincible = true;
            _playerPed.IsVisible = true;
            _playerPed.BlockPermanentEvents = true;

            // swap this out
            mugshotBoardAttachment.Attach(_playerPed, _user, topLine: "FACE_N_CHAR");

            Instance.SoundEngine.Enable();
            
            if (!reload)
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
            await BaseScript.Delay(2500);

            cam.Delete();

            Gender gender = (Gender)_characterSkin.Gender;
            await _playerPed.TaskWalkInToCharacterCreationRoom(GetLineupOrCreationAnimation(true, false, gender));
            OnCharacterCreationMenuAsync(gender);
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

            #region Character Sex
            Logger.Debug($"Character Sex Selected: {gender}");
            UIMenuListItem mLstCharacterSex = new UIMenuListItem("Sex", new List<dynamic> { "Male", "Female" }, (int)gender, "Select character sex, any changes will be lost.");
            _menuBase.AddItem(mLstCharacterSex);

            mLstCharacterSex.OnListChanged += async (item, index) =>
            {
                Interface.Hud.MenuPool.CloseAllMenus();
                Screen.Effects.Start(ScreenEffect.MpCelebWin);
                await Interface.Hud.FadeOut(1000);
                _menuBase.Clear();
                await BaseScript.Delay(1500);
                Screen.Effects.Stop(ScreenEffect.MpCelebWin);

                mugshotBoardAttachment.Reset();

                Gender newGender = (Gender)index;

                Logger.Debug($"Character Sex: {newGender}");

                CreateCharacterClass(false, false, newGender);

                Logger.Debug($"Character Skin Sex: {_characterSkin.Gender}");

                Instance.DetachTickHandler(OnCharacterCreationWarningAsync);
                Instance.DetachTickHandler(OnCharacterCreationMenuControlsAsync);

                OnCreateNewCharacter(_characterSkin, true);
            };
            #endregion

            _menuParents = Interface.Hud.MenuPool.AddSubMenu(_menuBase, GetLabelText("FACE_HERI"), GetLabelText("FACE_MM_H3"));
            _menuParents.ControlDisablingEnabled = true;
            _menuDetails = Interface.Hud.MenuPool.AddSubMenu(_menuBase, GetLabelText("FACE_FEAT"), GetLabelText("FACE_MM_H4"));
            _menuDetails.ControlDisablingEnabled = true;
            _menuAppearance = Interface.Hud.MenuPool.AddSubMenu(_menuBase, GetLabelText("FACE_APP"), GetLabelText("FACE_MM_H6"));
            _menuAppearance.ControlDisablingEnabled = true;
            _menuApparel = Interface.Hud.MenuPool.AddSubMenu(_menuBase, GetLabelText("FACE_APPA"), GetLabelText("FACE_APPA_H"));
            _menuApparel.ControlDisablingEnabled = true;
            _menuAdvancedApparel = Interface.Hud.MenuPool.AddSubMenu(_menuBase, $"Adv. {GetLabelText("FACE_APPA")}", GetLabelText("FACE_APPA_H"));
            _menuAdvancedApparel.ControlDisablingEnabled = true;
            _menuStats = Interface.Hud.MenuPool.AddSubMenu(_menuBase, GetLabelText("FACE_STATS"), GetLabelText("FACE_MM_H5"));
            _menuStats.ControlDisablingEnabled = true;

            InstructionalButton btnLookLeftOrRight = new InstructionalButton(Control.LookLeftRight, "Look Right/Left");
            InstructionalButton btnRandomise = new InstructionalButton(Control.FrontendDelete, "Randomise");
            InstructionalButton btnLookLeft = new InstructionalButton(Control.FrontendLb, "Look Left");
            InstructionalButton btnLookRight = new InstructionalButton(Control.FrontendRb, "Look Right");
            InstructionalButton button4 = new InstructionalButton(InputGroup.INPUTGROUP_LOOK, "Change details");
            InstructionalButton button5 = new InstructionalButton(InputGroup.INPUTGROUP_LOOK, "Manage Panels", ScaleformUI.PadCheck.Keyboard);
            
            _menuBase.InstructionalButtons.Add(btnLookRight);
            _menuBase.InstructionalButtons.Add(btnLookLeft);
            _menuParents.InstructionalButtons.Add(btnLookRight);
            _menuParents.InstructionalButtons.Add(btnLookLeft);
            _menuParents.InstructionalButtons.Add(btnRandomise);
            _menuAppearance.InstructionalButtons.Add(btnLookRight);
            _menuAppearance.InstructionalButtons.Add(btnLookLeft);
            _menuAppearance.InstructionalButtons.Add(button5);
            _menuDetails.InstructionalButtons.Add(btnLookRight);
            _menuDetails.InstructionalButtons.Add(btnLookLeft);
            _menuDetails.InstructionalButtons.Add(button4);

            #region Parents

            _heritageWindow = new UIMenuHeritageWindow(_characterSkin.Face.Mother, _characterSkin.Face.Father);
            _menuParents.AddWindow(_heritageWindow);
            List<dynamic> lista = new List<dynamic>();
            for (int i = 0; i < 101; i++)
                lista.Add(i);
            _mliParentMother = new UIMenuListItem("Mother", _lstParentMother, _characterSkin.Face.Mother);
            _mliParentFather = new UIMenuListItem("Father", _lstParentFather, _characterSkin.Face.Father);
            
            _msiResemblance = new UIMenuSliderItem(GetLabelText("FACE_H_DOM"), "", true)
            {
                Multiplier = 2,
                Value = (int)Math.Round(_characterSkin.Face.Resemblance * 100)
            };
            
            _msiSkinBlend = new UIMenuSliderItem(GetLabelText("FACE_H_STON"), "", true)
            {
                Multiplier = 2,
                Value = (int)Math.Round(_characterSkin.Face.SkinBlend * 100)
            };

            _menuParents.AddItem(_mliParentMother);
            _menuParents.AddItem(_mliParentFather);
            _menuParents.AddItem(_msiResemblance);
            _menuParents.AddItem(_msiSkinBlend);

            _menuParents.OnListChange += (_sender, _listItem, _newIndex) =>
            {
                if (_listItem == _mliParentMother)
                {
                    _characterSkin.Face.Mother = _newIndex;
                    _heritageWindow.Index(_characterSkin.Face.Mother, _characterSkin.Face.Father);
                }
                else if (_listItem == _mliParentFather)
                {
                    _characterSkin.Face.Father = _newIndex;
                    _heritageWindow.Index(_characterSkin.Face.Mother, _characterSkin.Face.Father);
                }

                UpdateFace(Game.PlayerPed.Handle, _characterSkin);
            };
            
            _menuParents.OnSliderChange += async (_sender, _item, _newIndex) =>
            {
                if (_item == _msiResemblance)
                    _characterSkin.Face.Resemblance = _newIndex / 100f;
                else if (_item == _msiSkinBlend)
                    _characterSkin.Face.SkinBlend = _newIndex / 100f;

                UpdateFace(Game.PlayerPed.Handle, _characterSkin);
            };

            #endregion

            #region Facial Details
            string _detailStandard = "Standard";
            string _detailBrowUp = GetLabelText("FACE_F_UP_B");
            string _detailBrowDown = GetLabelText("FACE_F_DOWN_B");
            string _detailEyeSquint = GetLabelText("FACE_F_SQUINT");
            string _detailEyeWideOpen = GetLabelText("FACE_F_WIDE_E");
            string _detailNoseNarrow = GetLabelText("FACE_F_NAR_N");
            string _detailNoseWide = GetLabelText("FACE_F_WIDE_N");
            string _detailNoseProfileShort = GetLabelText("FACE_F_SHORT");
            string _detailNoseProfileLong = GetLabelText("FACE_F_LONG");
            string _detailNoseTipPointUp = GetLabelText("FACE_F_TIPU");
            string _detailNoseTipPointDown = GetLabelText("FACE_F_TIPD");
            string _detailCheekBoneIn = GetLabelText("FACE_F_IN_C");
            string _detailCheekBoneOut = GetLabelText("FACE_F_OUT_C");
            string _detailCheekShapeGaunt = GetLabelText("FACE_F_GAUNT");
            string _detailCheekShapePuffed = GetLabelText("FACE_F_PUFF");
            string _detailLipThin = GetLabelText("FACE_F_THIN");
            string _detailLipThick = GetLabelText("FACE_F_FAT");
            string _detailJawNarrow = GetLabelText("FACE_F_NAR_J");
            string _detailJawWide = GetLabelText("FACE_F_WIDE_J");
            string _detailChinIn = GetLabelText("FACE_F_IN_CH");
            string _detailChinOut = GetLabelText("FACE_F_OUT_CH");
            string _detailChinShapeSquared = GetLabelText("FACE_F_SQ_CH");
            string _detailChinShapePointed = GetLabelText("FACE_F_PTD");

            _mLstEyeBrowProfile = new UIMenuListItem(GetLabelText("FACE_F_BROW"), new List<dynamic> { _detailStandard, _detailBrowUp, _detailBrowDown }, 0);
            _mLstEyes = new UIMenuListItem(GetLabelText("FACE_F_EYES"), new List<dynamic> { _detailStandard, _detailEyeSquint, _detailEyeWideOpen }, 0);
            _mLstNose = new UIMenuListItem(GetLabelText("FACE_F_NOSE"), new List<dynamic> { _detailStandard, _detailNoseNarrow, _detailNoseWide }, 0);
            _mLstNoseProfile = new UIMenuListItem(GetLabelText("FACE_F_NOSEP"), new List<dynamic>() { _detailStandard, _detailNoseProfileShort, _detailNoseProfileLong }, 0);
            _mLstNoseTip = new UIMenuListItem(GetLabelText("FACE_F_NOSET"), new List<dynamic>() { _detailStandard, _detailNoseTipPointUp, _detailNoseTipPointDown }, 0);
            _mLstCheekBones = new UIMenuListItem(GetLabelText("FACE_F_CHEEK"), new List<dynamic>() { _detailStandard, _detailCheekBoneIn, _detailCheekBoneOut }, 0);
            _mLstCheekShape = new UIMenuListItem(GetLabelText("FACE_F_CHEEKS"), new List<dynamic>() { _detailStandard, _detailCheekShapeGaunt, _detailCheekShapePuffed }, 0);
            _mLstLipThickness = new UIMenuListItem(GetLabelText("FACE_F_LIPS"), new List<dynamic>() { _detailStandard, _detailLipThin, _detailLipThick }, 0);
            _mLstJawProfile = new UIMenuListItem(GetLabelText("FACE_F_JAW"), new List<dynamic>() { _detailStandard, _detailJawNarrow, _detailJawWide }, 0);
            _mLstChinProfile = new UIMenuListItem(GetLabelText("FACE_F_CHIN"), new List<dynamic>() { _detailStandard, _detailChinIn, _detailChinOut }, 0);
            _mLstChinShape = new UIMenuListItem(GetLabelText("FACE_F_CHINS"), new List<dynamic>() { _detailStandard, _detailChinShapeSquared, _detailChinShapePointed }, 0);
            _mLstNeckThickness = new UIMenuListItem($"Neck", new List<dynamic>() { _detailStandard, _detailJawNarrow, _detailJawWide }, 0);

            UIMenuGridPanel GridEyeBrowProfile = new(_detailBrowUp, GetLabelText("FACE_F_IN_B"), GetLabelText("FACE_F_OUT_B"), _detailBrowDown,
                new PointF(_characterSkin.Face.Features[7], _characterSkin.Face.Features[6]));
            UIMenuGridPanel GridEyes = new(_detailEyeSquint, _detailEyeWideOpen, new PointF(_characterSkin.Face.Features[11], 0));
            UIMenuGridPanel GridNose = new(GetLabelText("FACE_F_UP_N"), _detailNoseNarrow, _detailNoseWide, GetLabelText("FACE_F_DOWN_N"),
                new PointF(_characterSkin.Face.Features[0], _characterSkin.Face.Features[1]));
            UIMenuGridPanel GridNoseProfile = new(GetLabelText("FACE_F_CROOK"), _detailNoseProfileShort, _detailNoseProfileLong, GetLabelText("FACE_F_CURV"),
                new PointF(_characterSkin.Face.Features[2], _characterSkin.Face.Features[3]));
            UIMenuGridPanel GridNoseTip = new(_detailNoseTipPointUp, GetLabelText("FACE_F_BRL"), GetLabelText("FACE_F_BRR"), _detailNoseTipPointDown,
                new PointF(_characterSkin.Face.Features[5], _characterSkin.Face.Features[4]));
            UIMenuGridPanel GridCheekBones = new(GetLabelText("FACE_F_UP_CHEE"), _detailCheekBoneIn, _detailCheekBoneOut, GetLabelText("FACE_F_DOWN_C"),
                new PointF(_characterSkin.Face.Features[9], _characterSkin.Face.Features[8]));
            UIMenuGridPanel GridCheekShape = new(_detailCheekShapeGaunt, _detailCheekShapePuffed, new PointF(_characterSkin.Face.Features[10], 0));
            UIMenuGridPanel GridLips = new(_detailLipThin, _detailLipThick, new PointF(_characterSkin.Face.Features[12], 0));
            UIMenuGridPanel GridJawProfile = new(GetLabelText("FACE_F_RND"), _detailJawNarrow, _detailJawWide, GetLabelText("FACE_F_SQ_J"),
                new PointF(_characterSkin.Face.Features[13], _characterSkin.Face.Features[14]));
            UIMenuGridPanel GridChinProfile = new(GetLabelText("FACE_F_UP_CHIN"), _detailChinIn, _detailChinOut, GetLabelText("FACE_F_DOWN_CH"),
                new PointF(_characterSkin.Face.Features[16], _characterSkin.Face.Features[15]));
            UIMenuGridPanel GridChinShape = new(GetLabelText("FACE_F_RDD"), _detailChinShapeSquared, _detailChinShapePointed, GetLabelText("FACE_F_BUM"), 
                new PointF(_characterSkin.Face.Features[18], _characterSkin.Face.Features[17]));
            UIMenuGridPanel GridNeck = new(_detailJawNarrow, _detailJawWide, new PointF(_characterSkin.Face.Features[19], 0));

            _mLstEyeBrowProfile.AddPanel(GridEyeBrowProfile);
            _mLstEyes.AddPanel(GridEyes);
            _mLstNose.AddPanel(GridNose);
            _mLstNoseProfile.AddPanel(GridNoseProfile);
            _mLstNoseTip.AddPanel(GridNoseTip);
            _mLstCheekBones.AddPanel(GridCheekBones);
            _mLstCheekShape.AddPanel(GridCheekShape);
            _mLstLipThickness.AddPanel(GridLips);
            _mLstJawProfile.AddPanel(GridJawProfile);
            _mLstChinProfile.AddPanel(GridChinProfile);
            _mLstChinShape.AddPanel(GridChinShape);
            _mLstNeckThickness.AddPanel(GridNeck);
            _menuDetails.AddItem(_mLstEyeBrowProfile);
            _menuDetails.AddItem(_mLstEyes);
            _menuDetails.AddItem(_mLstNose);
            _menuDetails.AddItem(_mLstNoseProfile);
            _menuDetails.AddItem(_mLstNoseTip);
            _menuDetails.AddItem(_mLstCheekBones);
            _menuDetails.AddItem(_mLstCheekShape);
            _menuDetails.AddItem(_mLstLipThickness);
            _menuDetails.AddItem(_mLstJawProfile);
            _menuDetails.AddItem(_mLstChinProfile);
            _menuDetails.AddItem(_mLstChinShape);
            _menuDetails.AddItem(_mLstNeckThickness);

            _menuDetails.OnGridPanelChange += (menu, panel, value) =>
            {
                if (menu == _mLstEyeBrowProfile)
                {
                    _characterSkin.Face.Features[7] = Common.Denormalize(value.X, -1f, 1f);
                    _characterSkin.Face.Features[6] = Common.Denormalize(value.Y, -1f, 1f);
                }
                else if (menu == _mLstEyes)
                {
                    _characterSkin.Face.Features[11] = Common.Denormalize(-value.X, -1f, 1f);
                }
                else if (menu == _mLstNose)
                {
                    _characterSkin.Face.Features[0] = Common.Denormalize(value.X, -1f, 1f);
                    _characterSkin.Face.Features[1] = Common.Denormalize(value.Y, -1f, 1f);
                }
                else if (menu == _mLstNoseProfile)
                {
                    _characterSkin.Face.Features[2] = Common.Denormalize(-value.X, -1f, 1f);
                    _characterSkin.Face.Features[3] = Common.Denormalize(value.Y, -1f, 1f);
                }
                else if (menu == _mLstNoseTip)
                {
                    _characterSkin.Face.Features[5] = Common.Denormalize(-value.X, -1f, 1f);
                    _characterSkin.Face.Features[4] = Common.Denormalize(value.Y, -1f, 1f);
                }
                else if (menu == _mLstCheekBones)
                {
                    _characterSkin.Face.Features[9] = Common.Denormalize(value.X, -1f, 1f);
                    _characterSkin.Face.Features[8] = Common.Denormalize(value.Y, -1f, 1f);
                }
                else if (menu == _mLstCheekShape)
                {
                    _characterSkin.Face.Features[10] = Common.Denormalize(-value.X, -1f, 1f);
                }
                else if (menu == _mLstNeckThickness)
                {
                    _characterSkin.Face.Features[19] = Common.Denormalize(value.X, -1f, 1f);
                }
                else if (menu == _mLstLipThickness)
                {
                    _characterSkin.Face.Features[12] = Common.Denormalize(-value.X, -1f, 1f);
                }
                else if (menu == _mLstJawProfile)
                {
                    _characterSkin.Face.Features[13] = Common.Denormalize(value.X, -1f, 1f);
                    _characterSkin.Face.Features[14] = Common.Denormalize(value.Y, -1f, 1f);
                }
                else if (menu == _mLstChinProfile)
                {
                    _characterSkin.Face.Features[16] = Common.Denormalize(value.X, -1f, 1f);
                    _characterSkin.Face.Features[15] = Common.Denormalize(value.Y, -1f, 1f);
                }
                else if (menu == _mLstChinShape)
                {
                    _characterSkin.Face.Features[17] = Common.Denormalize(-value.X, -1f, 1f);
                    _characterSkin.Face.Features[18] = Common.Denormalize(value.Y, -1f, 1f);
                }
                UpdateFace(_playerPed.Handle, _characterSkin);
            };

            int _oldIndexEyeBrowProfile = 0;
            int _oldIndexEyes = 0;
            int _oldIndexNosePosition = 0;
            int _oldIndexNoseProfile = 0;
            int _oldIndexNoseThickness = 0;
            int _oldIndexCheekbones = 0;
            int _oldIndexCheekShape = 0;
            int _oldIndexLipThickness = 0;
            int _oldIndexJawProfile = 0;
            int _oldIndexChinProfile = 0;
            int _oldIndexChinShape = 0;
            int _oldIndexNeckThickness = 0;

            _menuDetails.OnListChange += async (_sender, _listItem, _newIndex) =>
            {
                string itemValue = $"{_listItem.Items[_newIndex]}";
                
                if (_listItem == _mLstEyeBrowProfile)
                {
                    if (!(IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24)))
                    {
                        if (_oldIndexEyeBrowProfile != _newIndex)
                        {
                            if (itemValue == _detailBrowUp)
                            {
                                _characterSkin.Face.Features[6] = Common.Denormalize(0f, -1f, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF((_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.X, 0.00001f);
                            }
                            else if (itemValue == _detailBrowDown)
                            {
                                _characterSkin.Face.Features[6] = Common.Denormalize(1, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF((_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.X, 0.999999f);
                            }
                            else if (itemValue == _detailStandard)
                            {
                                _characterSkin.Face.Features[6] = Common.Denormalize(0.5f, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF((_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.X, 0.5f);
                            }

                            _oldIndexEyeBrowProfile = _newIndex;
                        }
                    }
                    PointF var = (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition;
                    _characterSkin.Face.Features[7] = Common.Denormalize(var.X, -1, 1);
                    _characterSkin.Face.Features[6] = Common.Denormalize(var.Y, -1, 1);
                }

                if (_listItem == _mLstEyes)
                {
                    if (!(IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24)))
                    {
                        if (_oldIndexEyes != _newIndex)
                        {
                            if (itemValue == _detailEyeSquint)
                            {
                                _characterSkin.Face.Features[11] = Common.Denormalize(0f, -1f, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.00001f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailEyeWideOpen)
                            {
                                _characterSkin.Face.Features[11] = Common.Denormalize(1, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.999999f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailStandard)
                            {
                                _characterSkin.Face.Features[11] = Common.Denormalize(0.5f, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.5f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }

                            _oldIndexEyes = _newIndex;
                        }
                    }
                    PointF var = (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition;
                    _characterSkin.Face.Features[11] = Common.Denormalize(-var.X, -1, 1);
                }

                if (_listItem == _mLstNose)
                {
                    if (!(IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24)))
                    {
                        if (_oldIndexNosePosition != _newIndex)
                        {
                            if (itemValue == _detailNoseWide)
                            {
                                _characterSkin.Face.Features[0] = Common.Denormalize(1, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.999999f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailNoseNarrow)
                            {
                                _characterSkin.Face.Features[0] = Common.Denormalize(0f, -1f, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.00001f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailStandard)
                            {
                                _characterSkin.Face.Features[0] = Common.Denormalize(0.5f, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.5f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }

                            _oldIndexNosePosition = _newIndex;
                        }
                    }

                    PointF var = (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition;
                    _characterSkin.Face.Features[0] = Common.Denormalize(var.X, -1, 1);
                    _characterSkin.Face.Features[1] = Common.Denormalize(var.Y, -1, 1);
                }

                if (_listItem == _mLstNoseProfile)
                {
                    if (!(IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24)))
                    {
                        if (_oldIndexNoseProfile != _newIndex)
                        {
                            if (itemValue == _detailNoseProfileShort)
                            {
                                _characterSkin.Face.Features[2] = Common.Denormalize(1, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.00001f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailNoseProfileLong)
                            {
                                _characterSkin.Face.Features[2] = Common.Denormalize(0f, -1f, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.999999f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailStandard)
                            {
                                _characterSkin.Face.Features[2] = Common.Denormalize(0.5f, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.5f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }

                            _oldIndexNoseProfile = _newIndex;
                        }
                    }

                    PointF var = (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition;
                    _characterSkin.Face.Features[3] = Common.Denormalize(var.Y, -1, 1);
                    _characterSkin.Face.Features[2] = Common.Denormalize(var.X, -1, 1);
                }

                if (_listItem == _mLstNoseTip)
                {
                    if (!(IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24)))
                    {
                        if (_oldIndexNoseThickness != _newIndex)
                        {
                            if (itemValue == _detailNoseTipPointUp)
                            {
                                _characterSkin.Face.Features[5] = Common.Denormalize(0f, -1f, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF((_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.X, 0.00001f);
                            }
                            else if (itemValue == _detailNoseTipPointDown)
                            {
                                _characterSkin.Face.Features[5] = Common.Denormalize(1, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF((_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.X, 0.999999f);
                            }
                            else if (itemValue == _detailStandard)
                            {
                                _characterSkin.Face.Features[5] = Common.Denormalize(0.5f, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF((_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.X, 0.5f);
                            }

                            _oldIndexNoseThickness = _newIndex;
                        }
                    }
                    PointF var = (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition;
                    _characterSkin.Face.Features[5] = Common.Denormalize(-var.X, -1, 1);
                    _characterSkin.Face.Features[4] = Common.Denormalize(var.Y, -1, 1);
                }

                if (_listItem == _mLstCheekBones)
                {
                    if (!(IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24)))
                    {
                        if (_oldIndexCheekbones != _newIndex)
                        {
                            if (itemValue == _detailCheekBoneIn)
                            {
                                _characterSkin.Face.Features[8] = Common.Denormalize(0f, -1f, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.00001f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailCheekBoneOut)
                            {
                                _characterSkin.Face.Features[8] = Common.Denormalize(1, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.999999f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailStandard)
                            {
                                _characterSkin.Face.Features[8] = Common.Denormalize(0.5f, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.5f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            
                            _oldIndexCheekbones = _newIndex;
                        }
                    }

                    PointF var = (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition;
                    _characterSkin.Face.Features[9] = Common.Denormalize(var.X, -1, 1);
                    _characterSkin.Face.Features[8] = Common.Denormalize(var.Y, -1, 1);
                }

                if (_listItem == _mLstCheekShape)
                {
                    if (!(IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24)))
                    {
                        if (_oldIndexCheekShape != _newIndex)
                        {
                            if (itemValue == _detailCheekShapeGaunt)
                            {
                                _characterSkin.Face.Features[10] = 0.00001f;
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.00001f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailCheekShapePuffed)
                            {
                                _characterSkin.Face.Features[10] = 0.999999f;
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.999999f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailStandard)
                            {
                                _characterSkin.Face.Features[10] = 0.5f;
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.5f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }

                            _oldIndexCheekShape = _newIndex;
                        }
                    }

                    PointF var = (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition;
                    _characterSkin.Face.Features[10] = Common.Denormalize(-var.X, -1, 1);
                }

                if (_listItem == _mLstLipThickness)
                {
                    if (!(IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24)))
                    {
                        if (_oldIndexLipThickness != _newIndex)
                        {
                            if (itemValue == _detailLipThin)
                            {
                                _characterSkin.Face.Features[12] = Common.Denormalize(0f, -1f, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.00001f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailLipThick)
                            {
                                _characterSkin.Face.Features[12] = Common.Denormalize(1, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.999999f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailStandard)
                            {
                                _characterSkin.Face.Features[12] = Common.Denormalize(0.5f, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.5f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }

                            _oldIndexLipThickness = _newIndex;
                        }
                    }

                    PointF var = (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition;
                    _characterSkin.Face.Features[12] = Common.Denormalize(-var.X, -1, 1);
                }

                if (_listItem == _mLstJawProfile)
                {
                    if (!(IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24)))
                    {
                        if (_oldIndexJawProfile != _newIndex)
                        {
                            if (itemValue == _detailJawNarrow)
                            {
                                _characterSkin.Face.Features[14] = Common.Denormalize(0f, -1f, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.00001f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailJawWide)
                            {
                                _characterSkin.Face.Features[14] = Common.Denormalize(1, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.999999f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailStandard)
                            {
                                _characterSkin.Face.Features[14] = Common.Denormalize(0.5f, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.5f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }

                            _oldIndexJawProfile = _newIndex;
                        }
                    }

                    PointF var = (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition;
                    _characterSkin.Face.Features[13] = Common.Denormalize(-var.X, -1, 1);
                    _characterSkin.Face.Features[14] = Common.Denormalize(var.Y, -1, 1);
                }

                if (_listItem == _mLstChinProfile)
                {
                    if (!(IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24)))
                    {
                        if (_oldIndexChinProfile != _newIndex)
                        {
                            if (itemValue == _detailChinIn)
                            {
                                _characterSkin.Face.Features[15] = Common.Denormalize(0f, -1f, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.00001f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailChinOut)
                            {
                                _characterSkin.Face.Features[15] = Common.Denormalize(1, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.999999f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailStandard)
                            {
                                _characterSkin.Face.Features[15] = Common.Denormalize(0.5f, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.5f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }

                            _oldIndexChinProfile = _newIndex;
                        }
                    }

                    PointF var = (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition;
                    _characterSkin.Face.Features[16] = Common.Denormalize(var.X, -1, 1);
                    _characterSkin.Face.Features[15] = Common.Denormalize(var.Y, -1, 1);
                }

                if (_listItem == _mLstChinShape)
                {
                    if (!(IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24)))
                    {
                        if (_oldIndexChinShape != _newIndex)
                        {
                            if (itemValue == _detailChinShapeSquared)
                            {
                                _characterSkin.Face.Features[17] = Common.Denormalize(0f, -1f, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.00001f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailChinShapePointed)
                            {
                                _characterSkin.Face.Features[17] = Common.Denormalize(1, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.999999f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailStandard)
                            {
                                _characterSkin.Face.Features[17] = Common.Denormalize(0.5f, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.5f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }

                            _oldIndexChinShape = _newIndex;
                        }
                    }

                    PointF var = (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition;
                    _characterSkin.Face.Features[18] = Common.Denormalize(-var.X, -1, 1);
                    _characterSkin.Face.Features[17] = Common.Denormalize(var.Y, -1, 1);
                }

                if (_listItem == _mLstNeckThickness)
                {
                    if (!(IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24)))
                    {
                        if (_oldIndexNeckThickness != _newIndex)
                        {
                            if (itemValue == _detailJawNarrow)
                            {
                                _characterSkin.Face.Features[19] = Common.Denormalize(0f, -1f, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.00001f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailJawWide)
                            {
                                _characterSkin.Face.Features[19] = Common.Denormalize(1, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.999999f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailStandard)
                            {
                                _characterSkin.Face.Features[19] = Common.Denormalize(0.5f, -1, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.5f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            
                            _oldIndexNeckThickness = _newIndex;
                        }
                    }

                    PointF var = (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition;
                    _characterSkin.Face.Features[19] = Common.Denormalize(var.X, -1, 1);
                }

                UpdateFace(_playerPed.Handle, _characterSkin);
            };

            #endregion

            #region Save and Exit

            UIMenuItem miSaveAndExit = new UIMenuItem(
                "Save Character",
                "This will save the character and throw you into the deepend.",
                HudColor.HUD_COLOUR_FREEMODE_DARK, HudColor.HUD_COLOUR_FREEMODE);
            
            miSaveAndExit.SetRightBadge(BadgeIcon.TICK);
            _menuBase.AddItem(miSaveAndExit);
            
            miSaveAndExit.Activated += async (selectedItem, index) =>
            {
                await Interface.Hud.FadeOut(800);
                _menuBase.Visible = false;
                Interface.Hud.MenuPool.CloseAllMenus();
                Game.PlayerPed.Detach();

                RemoveAnimDict("mp_character_creation@lineup@male_a");
                RemoveAnimDict("mp_character_creation@lineup@male_b");
                RemoveAnimDict("mp_character_creation@lineup@female_a");
                RemoveAnimDict("mp_character_creation@lineup@female_b");
                RemoveAnimDict("mp_character_creation@customise@male_a");
                RemoveAnimDict("mp_character_creation@customise@female_a");
            };

            #endregion

            Instance.AttachTickHandler(OnCharacterCreationMenuControlsAsync);

            if (!_menuBase.Visible)
                _menuBase.Visible = true;

            if (Screen.LoadingPrompt.IsActive)
                Screen.LoadingPrompt.Hide();
        }

        float _gridPanelCoordX;
        float _gridPanelCoordY;
        bool _isPedLookingLeft;
        bool _isPedLookingRight;

        int _frontendLeftBumper = (int)Control.FrontendLb; // 205
        int _frontendRightBumper = (int)Control.FrontendRb; // 206
        int _frontendDelete = (int)Control.FrontendDelete; // 206

        bool IsControlLeftBumperPressed => (IsControlPressed(0, _frontendLeftBumper) || IsDisabledControlPressed(0, _frontendLeftBumper)) && IsInputDisabled(2)
            || (IsControlPressed(2, _frontendLeftBumper) || IsDisabledControlPressed(2, _frontendLeftBumper)) && !IsInputDisabled(2);

        bool IsControlRightBumperPressed => (IsControlPressed(0, _frontendRightBumper) || IsDisabledControlPressed(0, _frontendRightBumper)) && IsInputDisabled(2)
                    || (IsControlPressed(2, _frontendRightBumper) || IsDisabledControlPressed(2, _frontendRightBumper)) && !IsInputDisabled(2);

        bool IsControlFrontendDeletePressed => (IsControlPressed(0, _frontendDelete) || IsDisabledControlPressed(0, _frontendDelete)) && IsInputDisabled(2)
            || (IsControlPressed(2, _frontendDelete) || IsDisabledControlPressed(2, _frontendDelete)) && !IsInputDisabled(2);

        public async Task OnCharacterCreationMenuControlsAsync()
        {
            _playerPed = Game.PlayerPed;
            if (_menuBase.Visible || _menuDetails.Visible || _menuAppearance.Visible || _menuParents.Visible)
            {
                if (IsControlFrontendDeletePressed && _menuParents.Visible)
                {
                    RandomiseCharacterParents();
                    await BaseScript.Delay(500);
                }
                else if (IsControlLeftBumperPressed)
                {
                    if (!_isPedLookingLeft)
                    {
                        _isPedLookingLeft = true;
                        _playerPed.TaskLookLeft(GetLineupOrCreationAnimation(true, false, _playerPed.Gender));
                    }
                }
                else if (IsControlRightBumperPressed)
                {
                    if (!_isPedLookingRight)
                    {
                        _isPedLookingRight = true;
                        _playerPed.TaskLookRight(GetLineupOrCreationAnimation(true, false, _playerPed.Gender));
                    }
                }
                else
                {
                    if (_isPedLookingRight)
                        _playerPed.TaskStopLookingRight(GetLineupOrCreationAnimation(true, false, _playerPed.Gender));
                    else if (_isPedLookingLeft)
                        _playerPed.TaskStopLookingLeft(GetLineupOrCreationAnimation(true, false, _playerPed.Gender));
                    
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
                        new InstructionalButton(Control.FrontendDelete, "No"),
                        new InstructionalButton(Control.FrontendAccept, "Yes"),
                    }
                );
                ScaleformUI.ScaleformUI.Warning.OnButtonPressed += async (action) =>
                {
                    if (action.GamepadButton == Control.FrontendDelete)
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

        async void CreateCharacterClass(bool randomise = false, bool randomGender = true, Gender gender = Gender.Male)
        {
            if (!randomise)
            {
                if (_characterSkin is null)
                    _characterSkin = new();

                if (randomGender)
                    _characterSkin.Gender = Common.RANDOM.Next(2);
                else
                    _characterSkin.Gender = (int)gender;

                Logger.Debug($"Skin Gender: {_characterSkin.Gender}");

                _characterSkin.Model = (uint)(_characterSkin.Gender == 0 ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);
                Model model = (int)_characterSkin.Model;
                await model.Request(1000);
                Game.Player.ChangeModel(model);
                model.MarkAsNoLongerNeeded();
                await Common.MoveToMainThread();
            }

            _playerPed = Game.PlayerPed;
            RandomiseCharacterParents();
        }

        private void RandomiseCharacterParents()
        {
            _characterSkin.Face.SkinBlend = GetRandomFloatInRange(.5f, 1f);
            _characterSkin.Face.Resemblance = GetRandomFloatInRange(.5f, 1f);
            _characterSkin.Face.Mother = Common.RANDOM.Next(CharacterCreatorData.FacesMother.Count);
            _characterSkin.Face.Father = Common.RANDOM.Next(CharacterCreatorData.FacesFather.Count);
            _characterSkin.Face.Features = Enumerable.Repeat(Common.Normalize(0f, -1, 1), 20).ToArray();

            if (_heritageWindow is not null)
                _heritageWindow.Index(_characterSkin.Face.Mother, _characterSkin.Face.Father);

            if (_mliParentMother is not null) // TODO: Update Scaleform to allow List Update when Visible from Code Behind
                _mliParentMother.Index = _characterSkin.Face.Mother;

            if (_mliParentFather is not null) // TODO: Update Scaleform to allow List Update when Visible from Code Behind
                _mliParentFather.Index = _characterSkin.Face.Father;

            if (_msiResemblance is not null) // TODO: Update Scaleform to allow Slider Update when Visible from Code Behind
                _msiResemblance.Value = (int)Math.Round(_characterSkin.Face.Resemblance * 100);

            if (_msiSkinBlend is not null) // TODO: Update Scaleform to allow Slider Update when Visible from Code Behind
                _msiSkinBlend.Value = (int)Math.Round(_characterSkin.Face.SkinBlend * 100);

            UpdateFace(_playerPed.Handle, _characterSkin);
        }

        public static void UpdateFace(int Handle, CharacterSkin skin)
        {
            SetPedHeadBlendData(
                Handle,
                skin.Face.Mother,
                skin.Face.Father,
                0,
                skin.Face.Mother,
                skin.Face.Father,
                0,
                skin.Face.Resemblance,
                skin.Face.SkinBlend,
                0f,
                false
            );

            //SetPedHeadOverlay(Handle, 0, skin.blemishes.style, skin.blemishes.opacity);
            //SetPedHeadOverlay(
            //    Handle,
            //    1,
            //    skin.facialHair.beard.style,
            //    skin.facialHair.beard.opacity
            //);
            //SetPedHeadOverlayColor(
            //    Handle,
            //    1,
            //    1,
            //    skin.facialHair.beard.color[0],
            //    skin.facialHair.beard.color[1]
            //);
            //SetPedHeadOverlay(
            //    Handle,
            //    2,
            //    skin.facialHair.eyebrow.style,
            //    skin.facialHair.eyebrow.opacity
            //);
            //SetPedHeadOverlayColor(
            //    Handle,
            //    2,
            //    1,
            //    skin.facialHair.eyebrow.color[0],
            //    skin.facialHair.eyebrow.color[1]
            //);
            //SetPedHeadOverlay(Handle, 3, skin.ageing.style, skin.ageing.opacity);
            //SetPedHeadOverlay(Handle, 4, skin.makeup.style, skin.makeup.opacity);
            //SetPedHeadOverlay(Handle, 5, skin.blusher.style, skin.blusher.opacity);
            //SetPedHeadOverlayColor(Handle, 5, 2, skin.blusher.color[0], skin.blusher.color[1]);
            //SetPedHeadOverlay(Handle, 6, skin.complexion.style, skin.complexion.opacity);
            //SetPedHeadOverlay(Handle, 7, skin.skinDamage.style, skin.skinDamage.opacity);
            //SetPedHeadOverlay(Handle, 8, skin.lipstick.style, skin.lipstick.opacity);
            //SetPedHeadOverlayColor(Handle, 8, 2, skin.lipstick.color[0], skin.lipstick.color[1]);
            //SetPedHeadOverlay(Handle, 9, skin.freckles.style, skin.freckles.opacity);
            //SetPedEyeColor(Handle, skin.eye.style);
            //SetPedComponentVariation(Handle, 2, skin.hair.style, 0, 0);
            //SetPedHairColor(Handle, skin.hair.color[0], skin.hair.color[1]);
            //SetPedPropIndex(Handle, 2, skin.ears.style, skin.ears.color, false);
            for (int i = 0; i < skin.Face.Features.Length; i++)
                SetPedFaceFeature(Handle, i, skin.Face.Features[i]);
        }
    }
}
