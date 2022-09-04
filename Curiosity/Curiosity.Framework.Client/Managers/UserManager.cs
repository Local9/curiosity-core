using CitizenFX.Core.UI;
using Curiosity.Framework.Client.Datasets;
using Curiosity.Framework.Client.Events;
using Curiosity.Framework.Client.Extensions;
using Curiosity.Framework.Client.Utils;
using Curiosity.Framework.Shared;
using Curiosity.Framework.Shared.SerializedModels;
using ScaleformUI;
using System.Drawing;
using System.Xml.Linq;

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
        UIMenu _menuFeatures = new UIMenu("", "", true);
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

            await CreateCharacterClass();

            OnCreateNewCharacter(_characterSkin);

            // goto character selection
            // if new character make one
            // else load selected character

            Logger.Trace($"User Database: [{user.Handle}] {user.Username}#{user.UserID} with {user.Characters.Count} Character(s).");
        }

        async void SetupCharacterCreator()
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
                await CreateCharacterClass();
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

            GameInterface.Hud.FadeIn(800);
            await BaseScript.Delay(2500);

            cam.Delete();

            Gender gender = (Gender)_characterSkin.Gender;
            await _playerPed.TaskWalkInToCharacterCreationRoom(GetLineupOrCreationAnimation(true, false, gender));
            OnCharacterCreationMenuAsync(gender);
        }

        public async void OnCharacterCreationMenuAsync(Gender gender)
        {
            await GameInterface.Hud.FadeIn(800);
            Point offset = new Point(50, 50);
            GameInterface.Hud.MenuPool.MouseEdgeEnabled = false;
            _menuBase = new("Character Creator", "Create a new Character", offset)
            {
                ControlDisablingEnabled = true
            };
            GameInterface.Hud.MenuPool.Add(_menuBase);

            #region Character Sex
            Logger.Debug($"Character Sex Selected: {gender}");
            UIMenuListItem mLstCharacterSex = new UIMenuListItem("Sex", new List<dynamic> { "Male", "Female" }, (int)gender, "Select character sex, any changes will be lost.");
            _menuBase.AddItem(mLstCharacterSex);

            mLstCharacterSex.OnListChanged += async (item, index) =>
            {
                GameInterface.Hud.MenuPool.CloseAllMenus();
                Screen.Effects.Start(ScreenEffect.MpCelebWin);
                await GameInterface.Hud.FadeOut(1000);
                _menuBase.Clear();
                await BaseScript.Delay(1500);
                Screen.Effects.Stop(ScreenEffect.MpCelebWin);

                mugshotBoardAttachment.Reset();

                Gender newGender = (Gender)index;

                Logger.Debug($"Character Sex: {newGender}");

                await CreateCharacterClass(false, false, newGender);

                Logger.Debug($"Character Skin Sex: {_characterSkin.Gender}");

                Instance.DetachTickHandler(OnCharacterCreationWarningAsync);
                Instance.DetachTickHandler(OnCharacterCreationMenuControlsAsync);

                OnCreateNewCharacter(_characterSkin, true);
            };
            #endregion

            _menuParents = GameInterface.Hud.MenuPool.AddSubMenu(_menuBase, GetLabelText("FACE_HERI"), GetLabelText("FACE_MM_H3"));
            _menuParents.ControlDisablingEnabled = true;
            _menuFeatures = GameInterface.Hud.MenuPool.AddSubMenu(_menuBase, GetLabelText("FACE_FEAT"), GetLabelText("FACE_MM_H4"));
            _menuFeatures.ControlDisablingEnabled = true;
            _menuAppearance = GameInterface.Hud.MenuPool.AddSubMenu(_menuBase, GetLabelText("FACE_APP"), GetLabelText("FACE_MM_H6"));
            _menuAppearance.ControlDisablingEnabled = true;
            _menuApparel = GameInterface.Hud.MenuPool.AddSubMenu(_menuBase, GetLabelText("FACE_APPA"), GetLabelText("FACE_APPA_H"));
            _menuApparel.ControlDisablingEnabled = true;
            _menuAdvancedApparel = GameInterface.Hud.MenuPool.AddSubMenu(_menuBase, $"Adv. {GetLabelText("FACE_APPA")}", GetLabelText("FACE_APPA_H"));
            _menuAdvancedApparel.ControlDisablingEnabled = true;
            _menuStats = GameInterface.Hud.MenuPool.AddSubMenu(_menuBase, GetLabelText("FACE_STATS"), GetLabelText("FACE_MM_H5"));
            _menuStats.ControlDisablingEnabled = true;

            InstructionalButton btnLookLeftOrRight = new InstructionalButton(Control.LookLeftRight, "Look Right/Left");
            InstructionalButton btnRandomise = new InstructionalButton(Control.FrontendDelete, "Randomise");
            InstructionalButton btnLookLeft = new InstructionalButton(Control.FrontendLb, "Look Left");
            InstructionalButton btnLookRight = new InstructionalButton(Control.FrontendRb, "Look Right");
            InstructionalButton button4 = new InstructionalButton(InputGroup.INPUTGROUP_LOOK, "Change details");
            InstructionalButton btnMouse = new InstructionalButton(InputGroup.INPUTGROUP_LOOK, "Manage Panels", ScaleformUI.PadCheck.Keyboard);
            
            _menuBase.InstructionalButtons.Add(btnLookRight);
            _menuBase.InstructionalButtons.Add(btnLookLeft);

            _menuParents.InstructionalButtons.Add(btnLookRight);
            _menuParents.InstructionalButtons.Add(btnLookLeft);
            _menuParents.InstructionalButtons.Add(btnRandomise);

            _menuAppearance.InstructionalButtons.Add(btnLookRight);
            _menuAppearance.InstructionalButtons.Add(btnLookLeft);
            _menuAppearance.InstructionalButtons.Add(btnMouse);
            _menuAppearance.InstructionalButtons.Add(btnRandomise);

            _menuFeatures.InstructionalButtons.Add(btnLookRight);
            _menuFeatures.InstructionalButtons.Add(btnLookLeft);
            _menuFeatures.InstructionalButtons.Add(button4);
            _menuFeatures.InstructionalButtons.Add(btnRandomise);

            #region Appearance

            UIMenuListItem _mlstAppearanceHair = new(GetLabelText("FACE_HAIR"), CharacterCreatorData.HairMale, _characterSkin.Hair.Style);
            UIMenuListItem _mlstAppearanceEyeBrows = new(GetLabelText("FACE_F_EYEBR"), CharacterCreatorData.Eyebrows, _characterSkin.Face.Eyebrow.Style,
                    "Change your appearance, use the ~y~mouse~w~ to edit the panels.");
            UIMenuColorPanel _cpEyebrowColor1 = new("First Colour", ColorPanelType.Hair);
            //UIMenuColorPanel _cpEyebrowColor2 = new("Second Colour", ColorPanelType.Hair);
            UIMenuPercentagePanel _ppEyebrowOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceEyeBrows.AddPanel(_ppEyebrowOpacity);
            _mlstAppearanceEyeBrows.AddPanel(_cpEyebrowColor1);
            //_mlstAppearanceEyeBrows.AddPanel(_cpEyebrowColor2);
            UIMenuListItem _mlstAppearanceBeard = new(GetLabelText("FACE_F_BEARD"), CharacterCreatorData.Beards, _characterSkin.Face.Beard.Style,
                    "Change your appearance, use the ~y~mouse~w~ to edit the panels.");
            UIMenuColorPanel _cpBeardColor1 = new("First Colour", ColorPanelType.Hair);
            UIMenuColorPanel _cpBeardColor2 = new("Second Colour", ColorPanelType.Hair);
            UIMenuPercentagePanel _ppBeardOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceBeard.AddPanel(_ppBeardOpacity);
            _mlstAppearanceBeard.AddPanel(_cpBeardColor1);
            // _mlstAppearanceBeard.AddPanel(_cpBeardColor2);
            UIMenuListItem _mlstAppearanceSkinBlemishes = new(GetLabelText("FACE_F_SKINB"), CharacterCreatorData.Blemishes, _characterSkin.Face.Blemishes.Style,
                    "Change your appearance, use the ~y~mouse~w~ to edit the panels.");
            UIMenuPercentagePanel _ppBlemOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceSkinBlemishes.AddPanel(_ppBlemOpacity);
            UIMenuListItem _mlstAppearanceSkinAgeing = new(GetLabelText("FACE_F_SKINA"), CharacterCreatorData.Ageing, _characterSkin.Age.Style,
                    "Change your appearance, use the ~y~mouse~w~ to edit the panels.");
            UIMenuPercentagePanel _ppAgeOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceSkinAgeing.AddPanel(_ppAgeOpacity);
            UIMenuListItem _mlstAppearanceSkinComplexion = new(GetLabelText("FACE_F_SKC"), CharacterCreatorData.Complexions, _characterSkin.Face.Complexion.Style,
                    "Change your appearance, use the ~y~mouse~w~ to edit the panels.");
            UIMenuPercentagePanel _ppCompexionOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceSkinComplexion.AddPanel(_ppCompexionOpacity);
            UIMenuListItem _mlstAppearanceSkinMoles = new(GetLabelText("FACE_F_MOLE"), CharacterCreatorData.MolesAndFreckles, _characterSkin.Face.Freckles.Style,
                    "Change your appearance, use the ~y~mouse~w~ to edit the panels.");
            UIMenuPercentagePanel _ppFreckleOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceSkinMoles.AddPanel(_ppFreckleOpacity);
            UIMenuListItem _mlstAppearanceSkinDamage = new(GetLabelText("FACE_F_SUND"), CharacterCreatorData.SkinDamage, _characterSkin.Face.SkinDamage.Style,
                    "Change your appearance, use the ~y~mouse~w~ to edit the panels.");
            UIMenuPercentagePanel _ppSkinDamageOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceSkinDamage.AddPanel(_ppSkinDamageOpacity);
            UIMenuListItem _mlstAppearanceEyeColor = new(GetLabelText("FACE_APP_EYE"), CharacterCreatorData.EyeColours, _characterSkin.Face.Eye.Style,
                    "Change your appearance, use the ~y~mouse~w~ to edit the panels.");
            UIMenuListItem _mlstAppearanceEyeMakeup = new(GetLabelText("FACE_F_EYEM"), CharacterCreatorData.Makeup, _characterSkin.Face.Makeup.Style,
                    "Change your appearance, use the ~y~mouse~w~ to edit the panels.");
            UIMenuPercentagePanel _ppMakeupOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceEyeMakeup.AddPanel(_ppMakeupOpacity);
            UIMenuListItem _mlstAppearanceBlusher = new(GetLabelText("FACE_F_BLUSH"), CharacterCreatorData.BlusherFemale, _characterSkin.Face.Blusher.Style,
                    "Change your appearance, use the ~y~mouse~w~ to edit the panels.");
            UIMenuColorPanel _cpBlushColor1 = new("First Colour", ColorPanelType.Makeup);
            UIMenuColorPanel _cpBlushColor2 = new("Second Colour", ColorPanelType.Makeup);
            UIMenuPercentagePanel _ppBlushOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceBlusher.AddPanel(_ppBlushOpacity);
            _mlstAppearanceBlusher.AddPanel(_cpBlushColor1);
            //_mlstAppearanceBlusher.AddPanel(_cpBlushColor2);
            UIMenuListItem _mlstAppearanceLipStick = new(GetLabelText("FACE_F_LIPST"), CharacterCreatorData.Lipstick, _characterSkin.Face.Lipstick.Style,
                "Change your appearance, use the ~y~mouse~w~ to edit the panels.");
            UIMenuColorPanel _cpLipColor1 = new UIMenuColorPanel("First Colour", ColorPanelType.Makeup);
            UIMenuColorPanel _cpLipColor2 = new UIMenuColorPanel("Second Colour", ColorPanelType.Makeup);
            UIMenuPercentagePanel _ppLipOpacity = new UIMenuPercentagePanel("Opacity", "0%", "100%");
            _mlstAppearanceLipStick.AddPanel(_ppLipOpacity);
            _mlstAppearanceLipStick.AddPanel(_cpLipColor1);
            //_mlstAppearanceLipStick.AddPanel(_cpLipColor2);

            _menuAppearance.OnColorPanelChange += (_menu, _panel, _index) =>
            {
                if (_menu == _mlstAppearanceHair)
                {
                    if (_panel == _menu.Panels[1])
                        _characterSkin.Hair.Color[0] = _index;
                    //else if (_panel == _menu.Panels[2])
                    //    _characterSkin.Hair.Color[1] = _index;
                }
                else if (_menu == _mlstAppearanceEyeBrows)
                {
                    if (_panel == _menu.Panels[1])
                        _characterSkin.Face.Eyebrow.Color[0] = _index;
                    //else if (_panel == _menu.Panels[2])
                    //    _characterSkin.Face.Eyebrow.Color[1] = _index;
                }
                else if (_menu == _mlstAppearanceBeard)
                {
                    if (_panel == _menu.Panels[1])
                        _characterSkin.Face.Beard.Color[0] = _index;
                    //else if (_panel == _menu.Panels[2])
                    //    _characterSkin.Face.Beard.Color[1] = _index;
                }
                else if (_menu == _mlstAppearanceBlusher)
                {
                    if (_panel == _menu.Panels[1])
                        _characterSkin.Face.Blusher.Color[0] = _index;
                    //else if (_panel == _menu.Panels[2])
                    //    _characterSkin.Face.Blusher.Color[1] = _index;
                }
                else if (_menu == _mlstAppearanceLipStick)
                {
                    if (_panel == _menu.Panels[1])
                        _characterSkin.Face.Lipstick.Color[0] = _index;
                    //else if (_panel == _menu.Panels[2])
                    //    _characterSkin.Face.Lipstick.Color[1] = _index;
                }
                UpdateFace(_playerPed.Handle, _characterSkin);
            };

            _menuAppearance.OnPercentagePanelChange += (_menu, _panel, _index) =>
            {
                var pct = _index / 100;
                if (_menu == _mlstAppearanceEyeBrows)
                {
                    if (_panel == _menu.Panels[0])
                        _characterSkin.Face.Eyebrow.Opacity = pct;
                }
                else if (_menu == _mlstAppearanceBeard)
                {
                    if (_panel == _menu.Panels[0])
                        _characterSkin.Face.Beard.Opacity = pct;
                }
                else if (_menu == _mlstAppearanceBlusher)
                {
                    if (_panel == _menu.Panels[0])
                        _characterSkin.Face.Blusher.Opacity = pct;
                }
                else if (_menu == _mlstAppearanceLipStick)
                {
                    if (_panel == _menu.Panels[0])
                        _characterSkin.Face.Lipstick.Opacity = pct;
                }
                else if (_menu == _mlstAppearanceSkinBlemishes)
                {
                    if (_panel == _menu.Panels[0])
                        _characterSkin.Face.Blemishes.Opacity = pct;
                }
                else if (_menu == _mlstAppearanceSkinAgeing)
                {
                    if (_panel == _menu.Panels[0])
                        _characterSkin.Age.Opacity = pct;
                }
                else if (_menu == _mlstAppearanceSkinComplexion)
                {
                    if (_panel == _menu.Panels[0])
                        _characterSkin.Face.Complexion.Opacity = pct;
                }
                else if (_menu == _mlstAppearanceSkinMoles)
                {
                    if (_panel == _menu.Panels[0])
                        _characterSkin.Face.Freckles.Opacity = pct;
                }
                else if (_menu == _mlstAppearanceSkinDamage)
                {
                    if (_panel == _menu.Panels[0])
                        _characterSkin.Face.SkinDamage.Opacity = pct;
                }
                else if (_menu == _mlstAppearanceEyeMakeup)
                {
                    if (_panel == _menu.Panels[0])
                        _characterSkin.Face.Makeup.Opacity = pct;
                }
                UpdateFace(_playerPed.Handle, _characterSkin);
            };

            _menuAppearance.OnListChange += (_sender, _listItem, _newIndex) =>
            {
                if (_listItem == _mlstAppearanceHair)
                    _characterSkin.Hair.Style = _newIndex;
                else if (_listItem == _mlstAppearanceEyeBrows)
                    _characterSkin.Face.Eyebrow.Style = _newIndex;
                else if (_listItem == _mlstAppearanceBeard)
                    _characterSkin.Face.Beard.Style =
                        (string)_listItem.Items[_newIndex] == GetLabelText("FACE_F_P_OFF")
                            ? 255
                            : _newIndex - 1;
                else if (_listItem == _mlstAppearanceBlusher)
                    _characterSkin.Face.Blusher.Style =
                        (string)_listItem.Items[_newIndex] == GetLabelText("FACE_F_P_OFF")
                            ? 255
                            : _newIndex - 1;
                else if (_listItem == _mlstAppearanceLipStick)
                    _characterSkin.Face.Lipstick.Style =
                        (string)_listItem.Items[_newIndex] == GetLabelText("FACE_F_P_OFF")
                            ? 255
                            : _newIndex - 1;
                else if (_listItem == _mlstAppearanceSkinBlemishes)
                    _characterSkin.Face.Blemishes.Style =
                        (string)_listItem.Items[_newIndex] == GetLabelText("FACE_F_P_OFF")
                            ? 255
                            : _newIndex - 1;
                else if (_listItem == _mlstAppearanceSkinAgeing)
                    _characterSkin.Age.Style =
                        (string)_listItem.Items[_newIndex] == GetLabelText("FACE_F_P_OFF")
                            ? 255
                            : _newIndex - 1;
                else if (_listItem == _mlstAppearanceSkinComplexion)
                    _characterSkin.Face.Complexion.Style =
                        (string)_listItem.Items[_newIndex] == GetLabelText("FACE_F_P_OFF")
                            ? 255
                            : _newIndex - 1;
                else if (_listItem == _mlstAppearanceSkinMoles)
                    _characterSkin.Face.Freckles.Style =
                        (string)_listItem.Items[_newIndex] == GetLabelText("FACE_F_P_OFF")
                            ? 255
                            : _newIndex - 1;
                else if (_listItem == _mlstAppearanceSkinDamage)
                    _characterSkin.Face.SkinDamage.Style =
                        (string)_listItem.Items[_newIndex] == GetLabelText("FACE_F_P_OFF")
                            ? 255
                            : _newIndex - 1;
                else if (_listItem == _mlstAppearanceEyeColor)
                    _characterSkin.Face.Eye.Style = _newIndex;
                else if (_listItem == _mlstAppearanceEyeMakeup)
                    _characterSkin.Face.Makeup.Style =
                        (string)_listItem.Items[_newIndex] == GetLabelText("FACE_F_P_OFF")
                            ? 255
                            : _newIndex - 1;

                UpdateFace(_playerPed.Handle, _characterSkin);
            };

            #endregion

            #region Menu Change States

            GameInterface.Hud.MenuPool.OnMenuStateChanged += async (_oldMenu, _newMenu, _state) =>
            {
                switch (_state)
                {
                    case MenuState.ChangeForward:
                        if (_newMenu == _menuParents || _newMenu == _menuFeatures || _newMenu == _menuAppearance)
                            AnimateGameplayCamZoom(true, _mainCamera);

                        if (_newMenu == _menuApparel || _newMenu == _menuAdvancedApparel)
                            _playerPed.TaskCreatorClothes(GetLineupOrCreationAnimation(true, false, (Gender)_characterSkin.Gender));

                        if (_newMenu == _menuAppearance)
                        {
                            _menuAppearance.Clear();

                            List<dynamic> hairList = _characterSkin.IsMale ? CharacterCreatorData.HairMale : CharacterCreatorData.HairFemale;
                            _mlstAppearanceHair = new(GetLabelText("FACE_HAIR"), hairList, _characterSkin.Hair.Style);
                            _menuAppearance.AddItem(_mlstAppearanceHair);
                            _menuAppearance.AddItem(_mlstAppearanceEyeBrows);
                            
                            if (_characterSkin.IsMale)
                                _menuAppearance.AddItem(_mlstAppearanceBeard);
                            
                            _menuAppearance.AddItem(_mlstAppearanceSkinBlemishes);
                            _menuAppearance.AddItem(_mlstAppearanceSkinAgeing);
                            _menuAppearance.AddItem(_mlstAppearanceSkinComplexion);
                            _menuAppearance.AddItem(_mlstAppearanceSkinMoles);
                            _menuAppearance.AddItem(_mlstAppearanceSkinDamage);
                            _menuAppearance.AddItem(_mlstAppearanceEyeColor);
                            _menuAppearance.AddItem(_mlstAppearanceEyeMakeup);
                            _menuAppearance.AddItem(_mlstAppearanceLipStick);

                            UIMenuColorPanel HairColor1 = new UIMenuColorPanel("First Colour", ColorPanelType.Hair);
                            // UIMenuColorPanel HairColor2 = new UIMenuColorPanel("Second Colour", ColorPanelType.Hair);
                            _mlstAppearanceHair.AddPanel(HairColor1);
                            // _mlstAppearanceHair.AddPanel(HairColor2);
                            
                            HairColor1.CurrentSelection = _characterSkin.Hair.Color[0];
                            // HairColor2.CurrentSelection = _characterSkin.Hair.Color[1];

                            _cpEyebrowColor1.CurrentSelection = _characterSkin.Face.Eyebrow.Color[0];
                            // _cpEyebrowColor2.CurrentSelection = _characterSkin.Face.Eyebrow.Color[1];
                            _ppEyebrowOpacity.Percentage = _characterSkin.Face.Eyebrow.Opacity * 100;
                            if (_characterSkin.IsMale)
                            {
                                _cpBeardColor1.CurrentSelection = _characterSkin.Face.Beard.Color[0];
                                //_cpBeardColor2.CurrentSelection = _characterSkin.Face.Beard.Color[1];
                                _ppBeardOpacity.Percentage = _characterSkin.Face.Beard.Opacity * 100;
                            }
                            _ppBlemOpacity.Percentage = _characterSkin.Face.Blemishes.Opacity * 100;
                            _ppAgeOpacity.Percentage = _characterSkin.Age.Opacity * 100;
                            _ppCompexionOpacity.Percentage = _characterSkin.Face.Complexion.Opacity * 100;
                            _ppFreckleOpacity.Percentage = _characterSkin.Face.Freckles.Opacity * 100;
                            _ppSkinDamageOpacity.Percentage = _characterSkin.Face.SkinDamage.Opacity * 100;
                            _ppMakeupOpacity.Percentage = _characterSkin.Face.Makeup.Opacity * 100;
                            _cpLipColor1.CurrentSelection = _characterSkin.Face.Lipstick.Color[0];
                            // _cpLipColor2.CurrentSelection = _characterSkin.Face.Lipstick.Color[1];
                            _ppLipOpacity.Percentage = _characterSkin.Face.Lipstick.Opacity * 100;
                        }
                        break;
                    case MenuState.ChangeBackward when _oldMenu == _menuParents || _oldMenu == _menuFeatures || _oldMenu == _menuAppearance:
                        AnimateGameplayCamZoom(false, _mainCamera);
                        break;
                    case MenuState.ChangeBackward:
                        if (_oldMenu == _menuApparel || _oldMenu == _menuAdvancedApparel)
                            _playerPed.TaskClothesALoop(GetLineupOrCreationAnimation(true, false, (Gender)_characterSkin.Gender));
                        break;
                }
            };

            #endregion

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
            UIMenuGridPanel GridEyes = new(_detailEyeWideOpen, _detailEyeSquint, new PointF(_characterSkin.Face.Features[11], 0));
            UIMenuGridPanel GridNose = new(GetLabelText("FACE_F_UP_N"), _detailNoseNarrow, _detailNoseWide, GetLabelText("FACE_F_DOWN_N"),
                new PointF(_characterSkin.Face.Features[0], _characterSkin.Face.Features[1]));
            UIMenuGridPanel GridNoseProfile = new(GetLabelText("FACE_F_CROOK"), _detailNoseProfileShort, _detailNoseProfileLong, GetLabelText("FACE_F_CURV"),
                new PointF(_characterSkin.Face.Features[2], _characterSkin.Face.Features[3]));
            UIMenuGridPanel GridNoseTip = new(_detailNoseTipPointUp, GetLabelText("FACE_F_BRL"), GetLabelText("FACE_F_BRR"), _detailNoseTipPointDown,
                new PointF(_characterSkin.Face.Features[5], _characterSkin.Face.Features[4]));
            UIMenuGridPanel GridCheekBones = new(GetLabelText("FACE_F_UP_CHEE"), _detailCheekBoneIn, _detailCheekBoneOut, GetLabelText("FACE_F_DOWN_C"),
                new PointF(_characterSkin.Face.Features[9], _characterSkin.Face.Features[8]));
            UIMenuGridPanel GridCheekShape = new(_detailCheekShapePuffed, _detailCheekShapeGaunt, new PointF(_characterSkin.Face.Features[10], 0));
            UIMenuGridPanel GridLips = new(_detailLipThick, _detailLipThin, new PointF(_characterSkin.Face.Features[12], 0));
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
            _menuFeatures.AddItem(_mLstEyeBrowProfile);
            _menuFeatures.AddItem(_mLstEyes);
            _menuFeatures.AddItem(_mLstNose);
            _menuFeatures.AddItem(_mLstNoseProfile);
            _menuFeatures.AddItem(_mLstNoseTip);
            _menuFeatures.AddItem(_mLstCheekBones);
            _menuFeatures.AddItem(_mLstCheekShape);
            _menuFeatures.AddItem(_mLstLipThickness);
            _menuFeatures.AddItem(_mLstJawProfile);
            _menuFeatures.AddItem(_mLstChinProfile);
            _menuFeatures.AddItem(_mLstChinShape);
            _menuFeatures.AddItem(_mLstNeckThickness);

            _menuFeatures.OnGridPanelChange += (menu, panel, value) =>
            {
                if (menu == _mLstEyeBrowProfile)
                {
                    _characterSkin.Face.Features[7] = Common.Denormalize(value.X, -1f, 1f);
                    _characterSkin.Face.Features[6] = Common.Denormalize(value.Y, -1f, 1f);
                }
                else if (menu == _mLstEyes)
                {
                    _characterSkin.Face.Features[11] = Common.Denormalize(value.X, -1f, 1f);
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
                    _characterSkin.Face.Features[10] = Common.Denormalize(value.X, -1f, 1f);
                }
                else if (menu == _mLstNeckThickness)
                {
                    _characterSkin.Face.Features[19] = Common.Denormalize(value.X, -1f, 1f);
                }
                else if (menu == _mLstLipThickness)
                {
                    _characterSkin.Face.Features[12] = Common.Denormalize(value.X, -1f, 1f);
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

            _menuFeatures.OnListChange += (_sender, _listItem, _newIndex) =>
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
                            if (itemValue == _detailEyeWideOpen)
                            {
                                _characterSkin.Face.Features[11] = Common.Denormalize(0f, -1f, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.00001f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailEyeSquint)
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
                    _characterSkin.Face.Features[11] = Common.Denormalize(var.X, -1, 1);
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
                            if (itemValue == _detailCheekShapePuffed)
                            {
                                _characterSkin.Face.Features[10] = 0.00001f;
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.00001f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailCheekShapeGaunt)
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
                    _characterSkin.Face.Features[10] = Common.Denormalize(var.X, -1f, 1f);
                }

                if (_listItem == _mLstLipThickness)
                {
                    if (!(IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24)))
                    {
                        if (_oldIndexLipThickness != _newIndex)
                        {
                            if (itemValue == _detailLipThick)
                            {
                                _characterSkin.Face.Features[12] = Common.Denormalize(0f, -1f, 1);
                                (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(0.00001f, (_listItem.Panels[0] as UIMenuGridPanel).CirclePosition.Y);
                            }
                            else if (itemValue == _detailLipThin)
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
                    _characterSkin.Face.Features[12] = Common.Denormalize(var.X, -1, 1);
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
                await GameInterface.Hud.FadeOut(800);
                _menuBase.Visible = false;
                GameInterface.Hud.MenuPool.CloseAllMenus();
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
            if (_menuBase.Visible || _menuFeatures.Visible || _menuAppearance.Visible || _menuParents.Visible)
            {
                if (IsControlFrontendDeletePressed)
                {
                    if (_menuParents.Visible)
                        RandomiseCharacterParents(true);

                    if (_menuFeatures.Visible)
                        RandomiseCharacterFeatures(true);

                    if (_menuAppearance.Visible)
                        RandomiseCharacterAppearance(true);

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

            if (!IsUsingKeyboard(2))
            {
                if (_menuFeatures.Visible)
                {
                    if (_mLstEyeBrowProfile.Selected)
                    {
                        PointF var = (_mLstEyeBrowProfile.Panels[0] as UIMenuGridPanel).CirclePosition;
                        _gridPanelCoordX = var.X;
                        _gridPanelCoordY = var.Y;

                        if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) || IsControlPressed(2, 5)
                            || IsDisabledControlPressed(2, 5) || IsControlPressed(2, 4) || IsDisabledControlPressed(2, 4)
                            || IsControlPressed(2, 3) || IsDisabledControlPressed(2, 3)
                        )
                        {
                            if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2))
                            {
                                _gridPanelCoordX += 0.03f;
                                if (_gridPanelCoordX > 1f)
                                    _gridPanelCoordX = 1f;
                            }

                            _characterSkin.Face.Features[7] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2))
                            {
                                _gridPanelCoordX -= 0.03f;
                                if (_gridPanelCoordX < 0)
                                    _gridPanelCoordX = 0;
                            }

                            _characterSkin.Face.Features[7] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (IsControlPressed(2, 4) || IsDisabledControlPressed(2, 4) && !IsInputDisabled(2))
                            {
                                _gridPanelCoordY += 0.03f;
                                if (_gridPanelCoordY > 1f)
                                    _gridPanelCoordY = 1f;
                            }

                            _characterSkin.Face.Features[6] = Common.Denormalize(_gridPanelCoordY, -1, 1);

                            if (IsControlPressed(2, 3) || IsDisabledControlPressed(2, 3) && !IsInputDisabled(2))
                            {
                                _gridPanelCoordY -= 0.03f;
                                if (_gridPanelCoordY < 0)
                                    _gridPanelCoordY = 0;
                            }

                            _characterSkin.Face.Features[6] = Common.Denormalize(_gridPanelCoordY, -1, 1);
                            (_mLstEyeBrowProfile.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(
                                _gridPanelCoordX,
                                _gridPanelCoordY
                            );
                        }
                    }

                    if (_mLstEyes.Selected)
                    {
                        PointF var = (_mLstEyes.Panels[0] as UIMenuGridPanel).CirclePosition;
                        _gridPanelCoordX = var.X;
                        _gridPanelCoordY = var.Y;

                        if (
                            IsControlPressed(2, 6)
                            || IsDisabledControlPressed(2, 6)
                            || IsControlPressed(2, 5)
                            || IsDisabledControlPressed(2, 5)
                        )
                        {
                            if (
                                IsControlPressed(2, 6)
                                || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX += 0.03f;
                                if (_gridPanelCoordX > 1f)
                                    _gridPanelCoordX = 1f;
                            }

                            _characterSkin.Face.Features[11] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 5)
                                || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX -= 0.03f;
                                if (_gridPanelCoordX < 0)
                                    _gridPanelCoordX = 0;
                            }

                            _characterSkin.Face.Features[11] = Common.Denormalize(_gridPanelCoordX, -1, 1);
                            (_mLstEyes.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(
                                _gridPanelCoordX,
                                .5f
                            );
                        }
                    }

                    if (_mLstCheekShape.Selected)
                    {
                        PointF var = (_mLstCheekShape.Panels[0] as UIMenuGridPanel).CirclePosition;
                        _gridPanelCoordX = var.X;
                        _gridPanelCoordY = var.Y;

                        if (
                            IsControlPressed(2, 6)
                            || IsDisabledControlPressed(2, 6)
                            || IsControlPressed(2, 5)
                            || IsDisabledControlPressed(2, 5)
                        )
                        {
                            if (
                                IsControlPressed(2, 6)
                                || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX += 0.03f;
                                if (_gridPanelCoordX > 1f)
                                    _gridPanelCoordX = 1f;
                            }

                            _characterSkin.Face.Features[10] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 5)
                                || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX -= 0.03f;
                                if (_gridPanelCoordX < 0)
                                    _gridPanelCoordX = 0;
                            }

                            _characterSkin.Face.Features[10] = Common.Denormalize(_gridPanelCoordX, -1, 1);
                            (_mLstCheekShape.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(
                                _gridPanelCoordX,
                                .5f
                            );
                        }
                    }

                    if (_mLstCheekBones.Selected)
                    {
                        PointF var = (_mLstCheekBones.Panels[0] as UIMenuGridPanel).CirclePosition;
                        _gridPanelCoordX = var.X;
                        _gridPanelCoordY = var.Y;

                        if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) || IsControlPressed(2, 5)
                            || IsDisabledControlPressed(2, 5) || IsControlPressed(2, 4) || IsDisabledControlPressed(2, 4)
                            || IsControlPressed(2, 3) || IsDisabledControlPressed(2, 3)
                        )
                        {
                            if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2))
                            {
                                _gridPanelCoordX += 0.03f;
                                if (_gridPanelCoordX > 1f)
                                    _gridPanelCoordX = 1f;
                            }

                            _characterSkin.Face.Features[9] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2))
                            {
                                _gridPanelCoordX -= 0.03f;
                                if (_gridPanelCoordX < 0)
                                    _gridPanelCoordX = 0;
                            }

                            _characterSkin.Face.Features[9] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (IsControlPressed(2, 4) || IsDisabledControlPressed(2, 4) && !IsInputDisabled(2))
                            {
                                _gridPanelCoordY += 0.03f;
                                if (_gridPanelCoordY > 1f)
                                    _gridPanelCoordY = 1f;
                            }

                            _characterSkin.Face.Features[8] = Common.Denormalize(_gridPanelCoordY, -1, 1);

                            if (IsControlPressed(2, 3) || IsDisabledControlPressed(2, 3) && !IsInputDisabled(2))
                            {
                                _gridPanelCoordY -= 0.03f;
                                if (_gridPanelCoordY < 0)
                                    _gridPanelCoordY = 0;
                            }

                            _characterSkin.Face.Features[8] = Common.Denormalize(_gridPanelCoordY, -1, 1);
                            (_mLstCheekBones.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(
                                _gridPanelCoordX,
                                _gridPanelCoordY
                            );
                        }
                    }

                    if (_mLstLipThickness.Selected)
                    {
                        PointF var = (_mLstLipThickness.Panels[0] as UIMenuGridPanel).CirclePosition;
                        _gridPanelCoordX = var.X;
                        _gridPanelCoordY = var.Y;

                        if (
                            IsControlPressed(2, 6)
                            || IsDisabledControlPressed(2, 6)
                            || IsControlPressed(2, 5)
                            || IsDisabledControlPressed(2, 5)
                        )
                        {
                            if (
                                IsControlPressed(2, 6)
                                || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX += 0.03f;
                                if (_gridPanelCoordX > 1f)
                                    _gridPanelCoordX = 1f;
                            }

                            _characterSkin.Face.Features[12] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 5)
                                || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX -= 0.03f;
                                if (_gridPanelCoordX < 0)
                                    _gridPanelCoordX = 0;
                            }

                            _characterSkin.Face.Features[12] = Common.Denormalize(_gridPanelCoordX, -1, 1);
                            (_mLstLipThickness.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(
                                _gridPanelCoordX,
                                .5f
                            );
                        }
                    }

                    if (_mLstNeckThickness.Selected)
                    {
                        PointF var = (_mLstNeckThickness.Panels[0] as UIMenuGridPanel).CirclePosition;
                        _gridPanelCoordX = var.X;
                        _gridPanelCoordY = var.Y;

                        if (
                            IsControlPressed(2, 6)
                            || IsDisabledControlPressed(2, 6)
                            || IsControlPressed(2, 5)
                            || IsDisabledControlPressed(2, 5)
                        )
                        {
                            if (
                                IsControlPressed(2, 6)
                                || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX += 0.03f;
                                if (_gridPanelCoordX > 1f)
                                    _gridPanelCoordX = 1f;
                            }

                            _characterSkin.Face.Features[19] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 5)
                                || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX -= 0.03f;
                                if (_gridPanelCoordX < 0)
                                    _gridPanelCoordX = 0;
                            }

                            _characterSkin.Face.Features[19] = Common.Denormalize(_gridPanelCoordX, -1, 1);
                            (_mLstNeckThickness.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(
                                _gridPanelCoordX,
                                .5f
                            );
                        }
                    }

                    if (_mLstNose.Selected)
                    {
                        PointF var = (_mLstNose.Panels[0] as UIMenuGridPanel).CirclePosition;
                        _gridPanelCoordX = var.X;
                        _gridPanelCoordY = var.Y;

                        if (
                            IsControlPressed(2, 6)
                            || IsDisabledControlPressed(2, 6)
                            || IsControlPressed(2, 5)
                            || IsDisabledControlPressed(2, 5)
                            || IsControlPressed(2, 4)
                            || IsDisabledControlPressed(2, 4)
                            || IsControlPressed(2, 3)
                            || IsDisabledControlPressed(2, 3)
                        )
                        {
                            if (
                                IsControlPressed(2, 6)
                                || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX += 0.03f;
                                if (_gridPanelCoordX > 1f)
                                    _gridPanelCoordX = 1f;
                            }

                            _characterSkin.Face.Features[0] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 5)
                                || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX -= 0.03f;
                                if (_gridPanelCoordX < 0)
                                    _gridPanelCoordX = 0;
                            }

                            _characterSkin.Face.Features[0] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 4)
                                || IsDisabledControlPressed(2, 4) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY += 0.03f;
                                if (_gridPanelCoordY > 1f)
                                    _gridPanelCoordY = 1f;
                            }

                            _characterSkin.Face.Features[1] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 3)
                                || IsDisabledControlPressed(2, 3) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY -= 0.03f;
                                if (_gridPanelCoordY < 0)
                                    _gridPanelCoordY = 0;
                            }

                            _characterSkin.Face.Features[1] = Common.Denormalize(_gridPanelCoordY, -1, 1);
                            (_mLstNose.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(
                                _gridPanelCoordX,
                                _gridPanelCoordY
                            );
                        }
                    }

                    if (_mLstNoseProfile.Selected)
                    {
                        PointF var = (_mLstNoseProfile.Panels[0] as UIMenuGridPanel).CirclePosition;
                        _gridPanelCoordX = var.X;
                        _gridPanelCoordY = var.Y;

                        if (
                            IsControlPressed(2, 6)
                            || IsDisabledControlPressed(2, 6)
                            || IsControlPressed(2, 5)
                            || IsDisabledControlPressed(2, 5)
                            || IsControlPressed(2, 4)
                            || IsDisabledControlPressed(2, 4)
                            || IsControlPressed(2, 3)
                            || IsDisabledControlPressed(2, 3)
                        )
                        {
                            if (
                                IsControlPressed(2, 6)
                                || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX += 0.03f;
                                if (_gridPanelCoordX > 1f)
                                    _gridPanelCoordX = 1f;
                            }

                            _characterSkin.Face.Features[2] = Common.Denormalize(-_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 5)
                                || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX -= 0.03f;
                                if (_gridPanelCoordX < 0)
                                    _gridPanelCoordX = 0;
                            }

                            _characterSkin.Face.Features[2] = Common.Denormalize(-_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 4)
                                || IsDisabledControlPressed(2, 4) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY += 0.03f;
                                if (_gridPanelCoordY > 1f)
                                    _gridPanelCoordY = 1f;
                            }

                            _characterSkin.Face.Features[3] = Common.Denormalize(_gridPanelCoordY, -1, 1);

                            if (
                                IsControlPressed(2, 3)
                                || IsDisabledControlPressed(2, 3) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY -= 0.03f;
                                if (_gridPanelCoordY < 0)
                                    _gridPanelCoordY = 0;
                            }

                            _characterSkin.Face.Features[3] = Common.Denormalize(_gridPanelCoordY, -1, 1);
                            (_mLstNoseProfile.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(
                                _gridPanelCoordX,
                                _gridPanelCoordY
                            );
                        }
                    }

                    if (_mLstNoseTip.Selected)
                    {
                        PointF var = (_mLstNoseTip.Panels[0] as UIMenuGridPanel).CirclePosition;
                        _gridPanelCoordX = var.X;
                        _gridPanelCoordY = var.Y;

                        if (
                            IsControlPressed(2, 6)
                            || IsDisabledControlPressed(2, 6)
                            || IsControlPressed(2, 5)
                            || IsDisabledControlPressed(2, 5)
                            || IsControlPressed(2, 4)
                            || IsDisabledControlPressed(2, 4)
                            || IsControlPressed(2, 3)
                            || IsDisabledControlPressed(2, 3)
                        )
                        {
                            if (
                                IsControlPressed(2, 6)
                                || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX += 0.03f;
                                if (_gridPanelCoordX > 1f)
                                    _gridPanelCoordX = 1f;
                            }

                            _characterSkin.Face.Features[5] = Common.Denormalize(-_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 5)
                                || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX -= 0.03f;
                                if (_gridPanelCoordX < 0)
                                    _gridPanelCoordX = 0;
                            }

                            _characterSkin.Face.Features[5] = Common.Denormalize(-_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 4)
                                || IsDisabledControlPressed(2, 4) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY += 0.03f;
                                if (_gridPanelCoordY > 1f)
                                    _gridPanelCoordY = 1f;
                            }

                            _characterSkin.Face.Features[4] = Common.Denormalize(_gridPanelCoordY, -1, 1);

                            if (
                                IsControlPressed(2, 3)
                                || IsDisabledControlPressed(2, 3) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY -= 0.03f;
                                if (_gridPanelCoordY < 0)
                                    _gridPanelCoordY = 0;
                            }

                            _characterSkin.Face.Features[4] = Common.Denormalize(_gridPanelCoordY, -1, 1);
                            (_mLstNoseTip.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(
                                _gridPanelCoordX,
                                _gridPanelCoordY
                            );
                        }
                    }

                    if (_mLstCheekShape.Selected)
                    {
                        PointF var = (_mLstCheekShape.Panels[0] as UIMenuGridPanel).CirclePosition;
                        _gridPanelCoordX = var.X;
                        _gridPanelCoordY = var.Y;

                        if (
                            IsControlPressed(2, 6)
                            || IsDisabledControlPressed(2, 6)
                            || IsControlPressed(2, 5)
                            || IsDisabledControlPressed(2, 5)
                            || IsControlPressed(2, 4)
                            || IsDisabledControlPressed(2, 4)
                            || IsControlPressed(2, 3)
                            || IsDisabledControlPressed(2, 3)
                        )
                        {
                            if (
                                IsControlPressed(2, 6)
                                || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX += 0.03f;
                                if (_gridPanelCoordX > 1f)
                                    _gridPanelCoordX = 1f;
                            }

                            _characterSkin.Face.Features[9] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 5)
                                || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX -= 0.03f;
                                if (_gridPanelCoordX < 0)
                                    _gridPanelCoordX = 0;
                            }

                            _characterSkin.Face.Features[9] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 4)
                                || IsDisabledControlPressed(2, 4) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY += 0.03f;
                                if (_gridPanelCoordY > 1f)
                                    _gridPanelCoordY = 1f;
                            }

                            _characterSkin.Face.Features[8] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 3)
                                || IsDisabledControlPressed(2, 3) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY -= 0.03f;
                                if (_gridPanelCoordY < 0)
                                    _gridPanelCoordY = 0;
                            }

                            _characterSkin.Face.Features[8] = Common.Denormalize(_gridPanelCoordY, -1, 1);
                            (_mLstCheekShape.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(
                                _gridPanelCoordX,
                                _gridPanelCoordY
                            );
                        }
                    }

                    if (_mLstJawProfile.Selected)
                    {
                        PointF var = (_mLstJawProfile.Panels[0] as UIMenuGridPanel).CirclePosition;
                        _gridPanelCoordX = var.X;
                        _gridPanelCoordY = var.Y;

                        if (
                            IsControlPressed(2, 6)
                            || IsDisabledControlPressed(2, 6)
                            || IsControlPressed(2, 5)
                            || IsDisabledControlPressed(2, 5)
                            || IsControlPressed(2, 4)
                            || IsDisabledControlPressed(2, 4)
                            || IsControlPressed(2, 3)
                            || IsDisabledControlPressed(2, 3)
                        )
                        {
                            if (
                                IsControlPressed(2, 6)
                                || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX += 0.03f;
                                if (_gridPanelCoordX > 1f)
                                    _gridPanelCoordX = 1f;
                            }

                            _characterSkin.Face.Features[13] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 5)
                                || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX -= 0.03f;
                                if (_gridPanelCoordX < 0)
                                    _gridPanelCoordX = 0;
                            }

                            _characterSkin.Face.Features[13] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 4)
                                || IsDisabledControlPressed(2, 4) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY += 0.03f;
                                if (_gridPanelCoordY > 1f)
                                    _gridPanelCoordY = 1f;
                            }

                            _characterSkin.Face.Features[14] = Common.Denormalize(_gridPanelCoordY, -1, 1);

                            if (
                                IsControlPressed(2, 3)
                                || IsDisabledControlPressed(2, 3) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY -= 0.03f;
                                if (_gridPanelCoordY < 0)
                                    _gridPanelCoordY = 0;
                            }

                            _characterSkin.Face.Features[14] = Common.Denormalize(_gridPanelCoordY, -1, 1);
                            (_mLstJawProfile.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(
                                _gridPanelCoordX,
                                _gridPanelCoordY
                            );
                        }
                    }

                    if (_mLstChinProfile.Selected)
                    {
                        PointF var = (_mLstChinProfile.Panels[0] as UIMenuGridPanel).CirclePosition;
                        _gridPanelCoordX = var.X;
                        _gridPanelCoordY = var.Y;

                        if (
                            IsControlPressed(2, 6)
                            || IsDisabledControlPressed(2, 6)
                            || IsControlPressed(2, 5)
                            || IsDisabledControlPressed(2, 5)
                            || IsControlPressed(2, 4)
                            || IsDisabledControlPressed(2, 4)
                            || IsControlPressed(2, 3)
                            || IsDisabledControlPressed(2, 3)
                        )
                        {
                            if (
                                IsControlPressed(2, 6)
                                || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX += 0.03f;
                                if (_gridPanelCoordX > 1f)
                                    _gridPanelCoordX = 1f;
                            }

                            _characterSkin.Face.Features[16] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 5)
                                || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX -= 0.03f;
                                if (_gridPanelCoordX < 0)
                                    _gridPanelCoordX = 0;
                            }

                            _characterSkin.Face.Features[16] = Common.Denormalize(_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 4)
                                || IsDisabledControlPressed(2, 4) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY += 0.03f;
                                if (_gridPanelCoordY > 1f)
                                    _gridPanelCoordY = 1f;
                            }

                            _characterSkin.Face.Features[15] = Common.Denormalize(_gridPanelCoordY, -1, 1);

                            if (
                                IsControlPressed(2, 3)
                                || IsDisabledControlPressed(2, 3) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY -= 0.03f;
                                if (_gridPanelCoordY < 0)
                                    _gridPanelCoordY = 0;
                            }

                            _characterSkin.Face.Features[15] = Common.Denormalize(_gridPanelCoordY, -1, 1);
                            (_mLstChinProfile.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(
                                _gridPanelCoordX,
                                _gridPanelCoordY
                            );
                        }
                    }

                    if (_mLstChinShape.Selected)
                    {
                        PointF var = (_mLstChinShape.Panels[0] as UIMenuGridPanel).CirclePosition;
                        _gridPanelCoordX = var.X;
                        _gridPanelCoordY = var.Y;

                        if (
                            IsControlPressed(2, 6)
                            || IsDisabledControlPressed(2, 6)
                            || IsControlPressed(2, 5)
                            || IsDisabledControlPressed(2, 5)
                            || IsControlPressed(2, 4)
                            || IsDisabledControlPressed(2, 4)
                            || IsControlPressed(2, 3)
                            || IsDisabledControlPressed(2, 3)
                        )
                        {
                            if (
                                IsControlPressed(2, 6)
                                || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX += 0.03f;
                                if (_gridPanelCoordX > 1f)
                                    _gridPanelCoordX = 1f;
                            }

                            _characterSkin.Face.Features[18] = Common.Denormalize(-_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 5)
                                || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordX -= 0.03f;
                                if (_gridPanelCoordX < 0)
                                    _gridPanelCoordX = 0;
                            }

                            _characterSkin.Face.Features[18] = Common.Denormalize(-_gridPanelCoordX, -1, 1);

                            if (
                                IsControlPressed(2, 4)
                                || IsDisabledControlPressed(2, 4) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY += 0.03f;
                                if (_gridPanelCoordY > 1f)
                                    _gridPanelCoordY = 1f;
                            }

                            _characterSkin.Face.Features[17] = Common.Denormalize(_gridPanelCoordY, -1, 1);

                            if (
                                IsControlPressed(2, 3)
                                || IsDisabledControlPressed(2, 3) && !IsInputDisabled(2)
                            )
                            {
                                _gridPanelCoordY -= 0.03f;
                                if (_gridPanelCoordY < 0)
                                    _gridPanelCoordY = 0;
                            }

                            _characterSkin.Face.Features[17] = Common.Denormalize(_gridPanelCoordY, -1, 1);
                            (_mLstChinShape.Panels[0] as UIMenuGridPanel).CirclePosition = new PointF(
                                _gridPanelCoordX,
                                _gridPanelCoordY
                            );
                        }
                    }

                    UpdateFace(_playerPed.Handle, _characterSkin);
                }
            }
        }

        public async Task OnCharacterCreationWarningAsync()
        {
            for (int i = 0; i < 32; i++)
                Game.DisableAllControlsThisFrame(i);

            if (_menuBase.Visible && _menuBase.HasControlJustBeenPressed(UIMenu.MenuControls.Back))
            {
                GameInterface.Hud.MenuPool.CloseAllMenus();
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
                        await GameInterface.Hud.FadeOut(1000);

                        Instance.DetachTickHandler(OnCharacterCreationWarningAsync);
                        Instance.DetachTickHandler(OnCharacterCreationMenuControlsAsync);

                        _menuBase.Visible = false;
                        GameInterface.Hud.MenuPool.CloseAllMenus();
                        
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

        async Task CreateCharacterClass(bool randomise = false, bool randomGender = true, Gender gender = Gender.Male)
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
            SetupCharacter();
            await BaseScript.Delay(1000);
        }

        private void RandomiseCharacterParents(bool update = false)
        {
            _characterSkin.Face.SkinBlend = GetRandomFloatInRange(.5f, 1f);
            _characterSkin.Face.Resemblance = GetRandomFloatInRange(.5f, 1f);
            _characterSkin.Face.Mother = Common.RANDOM.Next(CharacterCreatorData.FacesMother.Count);
            _characterSkin.Face.Father = Common.RANDOM.Next(CharacterCreatorData.FacesFather.Count);
            RandomiseCharacterFeatures();

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

            if (update)
                UpdateFace(_playerPed.Handle, _characterSkin);
        }

        private void RandomiseCharacterFeatures(bool update = false)
        {
            float[] featureArray = new float[20];

            for(int i = 0; i < 20; i++)
            {
                featureArray[i] = Common.Normalize((float)Common.RANDOM.NextDouble(), -1, 1);
            }

            _characterSkin.Face.Features = featureArray;

            if (update)
                UpdateFace(_playerPed.Handle, _characterSkin);
        }

        private void RandomiseCharacterAppearance(bool update = false)
        {
            bool isMale = (Gender)_characterSkin.Gender == Gender.Male;

            _characterSkin.Age = new(Common.RANDOM.Next(0, CharacterCreatorData.Ageing.Count), (float)Common.RANDOM.NextDouble());

            if (isMale)
            {
                _characterSkin.Face.Beard = new(Common.RANDOM.Next(0, CharacterCreatorData.Beards.Count), (float)Common.RANDOM.NextDouble(),
                    new int[2] { Common.RANDOM.Next(0, 63), 0 });
            }

            if (!isMale)
            {
                _characterSkin.Face.Blusher = new(Common.RANDOM.Next(0, CharacterCreatorData.BlusherFemale.Count), (float)Common.RANDOM.NextDouble(),
                    new int[2] { Common.RANDOM.Next(0, 63), Common.RANDOM.Next(0, 63) });
                _characterSkin.Face.Lipstick = new(Common.RANDOM.Next(0, CharacterCreatorData.Lipstick.Count), (float)Common.RANDOM.NextDouble(),
                    new int[2] { Common.RANDOM.Next(0, 63), Common.RANDOM.Next(0, 63) });
            }

            _characterSkin.Face.Blemishes = new(Common.RANDOM.Next(0, CharacterCreatorData.Blemishes.Count), (float)Common.RANDOM.NextDouble());
            _characterSkin.Face.Makeup = new(0, 1f);

            _characterSkin.Face.Eyebrow = new(Common.RANDOM.Next(0, CharacterCreatorData.Eyebrows.Count), (float)Common.RANDOM.NextDouble(),
                new int[2] { Common.RANDOM.Next(0, 63), 0 });

            _characterSkin.Face.Complexion = new(Common.RANDOM.Next(0, CharacterCreatorData.Complexions.Count), (float)Common.RANDOM.NextDouble());
            _characterSkin.Face.SkinDamage = new(Common.RANDOM.Next(0, CharacterCreatorData.SkinDamage.Count), (float)Common.RANDOM.NextDouble());
            _characterSkin.Face.Freckles = new(Common.RANDOM.Next(0, CharacterCreatorData.MolesAndFreckles.Count), (float)Common.RANDOM.NextDouble());

            int hairCount = isMale ? CharacterCreatorData.HairMale.Count : CharacterCreatorData.HairFemale.Count;
            _characterSkin.Hair = new(Common.RANDOM.Next(0, hairCount), new int[2] { Common.RANDOM.Next(0, 63), 0 });

            _characterSkin.Face.Eye = new(Common.RANDOM.Next(0, CharacterCreatorData.EyeColours.Count));
            _characterSkin.Ears = new(255, 0);

            if (update)
                UpdateFace(_playerPed.Handle, _characterSkin);
        }

        private void SetupCharacter()
        {
            RandomiseCharacterParents();
            RandomiseCharacterAppearance();
            UpdateFace(_playerPed.Handle, _characterSkin);
        }

        public void UpdateFace(int Handle, CharacterSkin skin)
        {
            bool isMale = (Gender)skin.Gender == Gender.Male;

            SetPedHeadBlendData(Handle, skin.Face.Mother, skin.Face.Father, 0, skin.Face.Mother, skin.Face.Father, 0, skin.Face.Resemblance,skin.Face.SkinBlend, 0f,false);

            SetPedHeadOverlay(Handle, 0, skin.Face.Blemishes.Style, skin.Face.Blemishes.Opacity);
           
            if (isMale)
            {
                SetPedHeadOverlay(Handle, 1, skin.Face.Beard.Style, skin.Face.Beard.Opacity);
                SetPedHeadOverlayColor(Handle, 1, 1, skin.Face.Beard.Color[0], skin.Face.Beard.Color[1]);
            }

            SetPedHeadOverlay(Handle, 2, skin.Face.Eyebrow.Style, skin.Face.Eyebrow.Opacity);
            SetPedHeadOverlayColor(Handle, 2, 1, skin.Face.Eyebrow.Color[0], skin.Face.Eyebrow.Color[1]);
            SetPedHeadOverlay(Handle, 3, skin.Age.Style, skin.Age.Opacity);
            SetPedHeadOverlay(Handle, 4, skin.Face.Makeup.Style, skin.Face.Makeup.Opacity);
            SetPedHeadOverlay(Handle, 5, skin.Face.Blusher.Style, skin.Face.Blusher.Opacity);
            SetPedHeadOverlayColor(Handle, 5, 2, skin.Face.Blusher.Color[0], skin.Face.Blusher.Color[1]);
            SetPedHeadOverlay(Handle, 8, skin.Face.Lipstick.Style, skin.Face.Lipstick.Opacity);
            SetPedHeadOverlayColor(Handle, 8, 2, skin.Face.Lipstick.Color[0], skin.Face.Lipstick.Color[1]);
            SetPedHeadOverlay(Handle, 6, skin.Face.Complexion.Style, skin.Face.Complexion.Opacity);
            SetPedHeadOverlay(Handle, 7, skin.Face.SkinDamage.Style, skin.Face.SkinDamage.Opacity);
            SetPedHeadOverlay(Handle, 9, skin.Face.Freckles.Style, skin.Face.Freckles.Opacity);
            
            SetPedEyeColor(Handle, skin.Face.Eye.Style);
            
            SetPedComponentVariation(Handle, 2, skin.Hair.Style, 0, 0);
            SetPedHairColor(Handle, skin.Hair.Color[0], skin.Hair.Color[1]);
            
            SetPedPropIndex(Handle, 2, skin.Ears.Style, skin.Ears.Color, false);

            for (int i = 0; i < skin.Face.Features.Length; i++)
                SetPedFaceFeature(Handle, i, skin.Face.Features[i]);
        }

        private static Camera ncamm = new Camera(CreateCam("DEFAULT_SCRIPTED_CAMERA", false));

        async void AnimateGameplayCamZoom(bool toggle, Camera ncam)
        {
            if (toggle)
            {
                SetGenericeCameraSettings(ncam.Handle, 3f, 1f, 1.2f, 1f);
                ncamm = new Camera(CreateCam("DEFAULT_SCRIPTED_CAMERA", false));
                ncamm.Position = new Vector3(402.6746f, -1000.129f, -98.46554f);
                ncamm.Rotation = new Vector3(0.861356f, 0f, -2.348183f);
                _playerPed.IsVisible = true;
                ncamm.FieldOfView = 15.00255f;
                ncamm.IsActive = true;
                SetGenericeCameraSettings(ncamm.Handle, 3.8f, 1f, 1.2f, 1f);
                ncam.InterpTo(ncamm, 300, 1, 1);
                Game.PlaySound("Zoom_In", "MUGSHOT_CHARACTER_CREATION_SOUNDS");
                while (ncam.IsInterpolating)
                    await BaseScript.Delay(0);
            }
            else
            {
                SetGenericeCameraSettings(ncamm.Handle, 3.8f, 1f, 1.2f, 1f);
                SetGenericeCameraSettings(ncam.Handle, 3f, 1f, 1.2f, 1f);
                ncamm.InterpTo(ncam, 300, 1, 1);
                ncamm.Delete();
                Game.PlaySound("Zoom_Out", "MUGSHOT_CHARACTER_CREATION_SOUNDS");
                while (ncam.IsInterpolating)
                    await BaseScript.Delay(0);
            }
        }

        static void SetGenericeCameraSettings(int cameraHandle, float fParam1, float fParam2, float dofLens, float dofBlend)
        {
            N_0xf55e4046f6f831dc(cameraHandle, fParam1);
            N_0xe111a7c0d200cbc5(cameraHandle, fParam2);
            SetCamDofFnumberOfLens(cameraHandle, dofLens);
            SetCamDofMaxNearInFocusDistanceBlendLevel(cameraHandle, dofBlend);
        }
    }
}
