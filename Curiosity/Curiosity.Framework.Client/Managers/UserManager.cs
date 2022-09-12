using CitizenFX.Core.UI;
using Curiosity.Framework.Client.Datasets;
using Curiosity.Framework.Client.Extensions;
using Curiosity.Framework.Client.Utils;
using Curiosity.Framework.Shared;
using Curiosity.Framework.Shared.Enums;
using Curiosity.Framework.Shared.Extensions;
using Curiosity.Framework.Shared.SerializedModels;
using FxEvents;
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
        UIMenu _menuFeatures = new UIMenu("", "", true);
        UIMenu _menuAppearance = new UIMenu("", "", true);
        UIMenu _menuApparel = new UIMenu("", "", true);
        UIMenu _menuAdvancedApparel = new UIMenu("", "", true);
        UIMenu _menuStats = new UIMenu("", "", true);

        // Lists
        List<dynamic> _lstParentMother = CharacterCreatorData.FacesMother;
        List<dynamic> _lstParentFather = CharacterCreatorData.FacesFather;

        // Menu List Items
        // Fetures
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
        // Appearance
        UIMenuListItem _mlstAppearanceHair;
        UIMenuListItem _mlstAppearanceEyeBrows;
        UIMenuListItem _mlstAppearanceBeard;
        UIMenuListItem _mlstAppearanceSkinBlemishes;
        UIMenuListItem _mlstAppearanceSkinAgeing;
        UIMenuListItem _mlstAppearanceSkinComplexion;
        UIMenuListItem _mlstAppearanceSkinMoles;
        UIMenuListItem _mlstAppearanceSkinDamage;
        UIMenuListItem _mlstAppearanceEyeColor;
        UIMenuListItem _mlstAppearanceEyeMakeup;
        UIMenuListItem _mlstAppearanceBlusher;
        UIMenuListItem _mlstAppearanceLipStick;
        // heritageWindow
        UIMenuHeritageWindow _heritageWindow;
        UIMenuListItem _mliParentMother;
        UIMenuListItem _mliParentFather;
        UIMenuSliderItem _msiResemblance;
        UIMenuSliderItem _msiSkinBlend;

        MugshotBoardAttachment mugshotBoardAttachment = new();
        const int MAX_CREATOR_COLOR = 63;
        float _finalHeading;

        public async override void Begin()
        {
            ScreenInterface.StartLoadingMessage("PM_WAIT");
            await BaseScript.Delay(5000);

            ScreenInterface.StartLoadingMessage("PM_WAIT");
            OnRequestCharactersAsync();
        }

        public async Task OnRequestCharactersAsync()
        {
            await LoadTransition.OnUpAsync();

            User user = await EventDispatcher.Get<User>("user:active", Game.Player.ServerId);

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

            Logger.Info($"User Database: [{user.Handle}] {user.Username}#{user.UserID} with {user.Characters.Count} Character(s).");
        }

        async Task SetupCharacterCreator()
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
                await BaseScript.Delay(100);
        }

        public async Task OnCreateNewCharacter(CharacterSkin characterSkin, bool reload = false)
        {
            ScreenInterface.StartLoadingMessage("PM_WAIT");
            await SetupCharacterCreator();

            DisplayHud(false);
            DisplayRadar(false);

            _user.ActiveCharacter = new Character();
            _user.ActiveCharacter.Stats = new CharacterStats();

            NetworkResurrectLocalPlayer(_characterCreatorSpawn.X, _characterCreatorSpawn.Y, _characterCreatorSpawn.Z, _characterCreatorSpawn.W, true, false);

            _playerPed = Game.PlayerPed;

            _playerPed.Position = new Vector3(_characterCreatorSpawn.X, _characterCreatorSpawn.Y, _characterCreatorSpawn.Z);
            _playerPed.Heading = _characterCreatorSpawn.W;

            await OnLoadCharacterCreatorInteriorAsync();
            await Common.MoveToMainThread();

            _playerPed.SetDefaultVariation();
            _playerPed.SetRandomFacialMood();

            _playerPed.IsInvincible = true;
            _playerPed.IsVisible = true;
            _playerPed.BlockPermanentEvents = true;

            // swap this out
            mugshotBoardAttachment.Attach(_playerPed, _user, topLine: "FACE_N_CHAR");

            Instance.SoundEngine.Enable();

            SetTimecycleModifier("default");
            SetTimecycleModifierStrength(1f);

            if (!reload)
            {
                await LoadTransition.OnWaitAsync();

                SetNuiFocus(false, false);
                ShutdownLoadingScreen();
                ShutdownLoadingScreenNui();

                GameInterface.Hud.FadeIn(800);
                await LoadTransition.OnDownAsync();
            }

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

            if (Screen.Fading.IsFadedOut)
                GameInterface.Hud.FadeIn(800);

            await BaseScript.Delay(2500);

            cam.Delete();

            Gender gender = (Gender)_characterSkin.Gender;
            await _playerPed.TaskWalkInToCharacterCreationRoom(GetLineupOrCreationAnimation(true, false));
            OnCharacterCreationMenuAsync(gender);

            _finalHeading = _playerPed.Heading;
        }

        public async void OnCharacterCreationMenuAsync(Gender gender)
        {
            await GameInterface.Hud.FadeIn(800);
            Point offset = new Point(50, 50);
            GameInterface.Hud.MenuPool.MouseEdgeEnabled = false;
            _menuBase = new("New Character", "Create a new Character", offset)
            {
                ControlDisablingEnabled = true
            };
            GameInterface.Hud.MenuPool.Add(_menuBase);

            #region Character Sex

            string male = GetLabelText("hash_51c60924_fjjdpzc_collision");
            string female = GetLabelText("hash_2165a502_ifrwtom_collision");
            
            UIMenuListItem mLstCharacterSex = new UIMenuListItem("Sex", new List<dynamic> { male, female }, (int)gender, "Select character sex, any changes will be lost.");
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

                await CreateCharacterClass(false, false, newGender);

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
            InstructionalButton btnRightStickMouse = new InstructionalButton(InputGroup.INPUTGROUP_LOOK, "Change details");
            InstructionalButton btnChangeOpacity = new InstructionalButton(InputGroup.INPUTGROUP_LOOK, "Change Opacity");
            InstructionalButton btnMouse = new InstructionalButton(InputGroup.INPUTGROUP_LOOK, "Manage Panels", ScaleformUI.PadCheck.Keyboard);
            InstructionalButton btnTriggers = new InstructionalButton(InputGroup.INPUTGROUP_FRONTEND_TRIGGERS, "Change Color");
            InstructionalButton btnRotate = new InstructionalButton(InputGroup.INPUTGROUP_FRONTEND_TRIGGERS, "Rotate");

            _menuBase.InstructionalButtons.Add(btnLookRight);
            _menuBase.InstructionalButtons.Add(btnLookLeft);

            _menuParents.InstructionalButtons.Add(btnLookRight);
            _menuParents.InstructionalButtons.Add(btnLookLeft);
            _menuParents.InstructionalButtons.Add(btnRandomise);

            _menuAppearance.InstructionalButtons.Add(btnLookRight);
            _menuAppearance.InstructionalButtons.Add(btnLookLeft);
            _menuAppearance.InstructionalButtons.Add(btnChangeOpacity);
            _menuAppearance.InstructionalButtons.Add(btnTriggers);
            _menuAppearance.InstructionalButtons.Add(btnRandomise);

            //_menuAdvancedApparel.InstructionalButtons.Add(btnRotate);

            _menuFeatures.InstructionalButtons.Add(btnLookRight);
            _menuFeatures.InstructionalButtons.Add(btnLookLeft);
            _menuFeatures.InstructionalButtons.Add(btnRightStickMouse);
            _menuFeatures.InstructionalButtons.Add(btnRandomise);

            #region Appearance

            _mlstAppearanceHair = new(GetLabelText("FACE_HAIR"), CharacterCreatorData.HairMale, _characterSkin.Hair.Style);
            _mlstAppearanceEyeBrows = new(GetLabelText("FACE_F_EYEBR"), CharacterCreatorData.Eyebrows, _characterSkin.Face.Eyebrow.Style);
            UIMenuColorPanel _cpEyebrowColor1 = new("Colour", ColorPanelType.Hair);
            //UIMenuColorPanel _cpEyebrowColor2 = new("Second Colour", ColorPanelType.Hair);
            UIMenuPercentagePanel _ppEyebrowOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceEyeBrows.AddPanel(_ppEyebrowOpacity);
            _mlstAppearanceEyeBrows.AddPanel(_cpEyebrowColor1);
            //_mlstAppearanceEyeBrows.AddPanel(_cpEyebrowColor2);
            _mlstAppearanceBeard = new(GetLabelText("FACE_F_BEARD"), CharacterCreatorData.Beards, _characterSkin.Face.Beard.Style);
            UIMenuColorPanel _cpBeardColor1 = new("Colour", ColorPanelType.Hair);
            //UIMenuColorPanel _cpBeardColor2 = new("Second Colour", ColorPanelType.Hair);
            UIMenuPercentagePanel _ppBeardOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceBeard.AddPanel(_ppBeardOpacity);
            _mlstAppearanceBeard.AddPanel(_cpBeardColor1);
            // _mlstAppearanceBeard.AddPanel(_cpBeardColor2);
            _mlstAppearanceSkinBlemishes = new(GetLabelText("FACE_F_SKINB"), CharacterCreatorData.Blemishes, _characterSkin.Face.Blemishes.Style);
            UIMenuPercentagePanel _ppBlemOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceSkinBlemishes.AddPanel(_ppBlemOpacity);
            _mlstAppearanceSkinAgeing = new(GetLabelText("FACE_F_SKINA"), CharacterCreatorData.Ageing, _characterSkin.Age.Style);
            UIMenuPercentagePanel _ppAgeOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceSkinAgeing.AddPanel(_ppAgeOpacity);
            _mlstAppearanceSkinComplexion = new(GetLabelText("FACE_F_SKC"), CharacterCreatorData.Complexions, _characterSkin.Face.Complexion.Style);
            UIMenuPercentagePanel _ppCompexionOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceSkinComplexion.AddPanel(_ppCompexionOpacity);
            _mlstAppearanceSkinMoles = new(GetLabelText("FACE_F_MOLE"), CharacterCreatorData.MolesAndFreckles, _characterSkin.Face.Freckles.Style);
            UIMenuPercentagePanel _ppFreckleOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceSkinMoles.AddPanel(_ppFreckleOpacity);
            _mlstAppearanceSkinDamage = new(GetLabelText("FACE_F_SUND"), CharacterCreatorData.SkinDamage, _characterSkin.Face.SkinDamage.Style);
            UIMenuPercentagePanel _ppSkinDamageOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceSkinDamage.AddPanel(_ppSkinDamageOpacity);
            _mlstAppearanceEyeColor = new(GetLabelText("FACE_APP_EYE"), CharacterCreatorData.EyeColours, _characterSkin.Face.Eye.Style);
            _mlstAppearanceEyeMakeup = new(GetLabelText("FACE_F_EYEM"), CharacterCreatorData.Makeup, _characterSkin.Face.Makeup.Style);
            UIMenuPercentagePanel _ppMakeupOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceEyeMakeup.AddPanel(_ppMakeupOpacity);
            _mlstAppearanceBlusher = new(GetLabelText("FACE_F_BLUSH"), CharacterCreatorData.BlusherFemale, _characterSkin.Face.Blusher.Style);
            UIMenuColorPanel _cpBlushColor1 = new("Colour", ColorPanelType.Makeup);
            //UIMenuColorPanel _cpBlushColor2 = new("Second Colour", ColorPanelType.Makeup);
            UIMenuPercentagePanel _ppBlushOpacity = new("Opacity", "0%", "100%");
            _mlstAppearanceBlusher.AddPanel(_ppBlushOpacity);
            _mlstAppearanceBlusher.AddPanel(_cpBlushColor1);
            //_mlstAppearanceBlusher.AddPanel(_cpBlushColor2);
            _mlstAppearanceLipStick = new(GetLabelText("FACE_F_LIPST"), CharacterCreatorData.Lipstick, _characterSkin.Face.Lipstick.Style);
            UIMenuColorPanel _cpLipColor1 = new UIMenuColorPanel("Colour", ColorPanelType.Makeup);
            //UIMenuColorPanel _cpLipColor2 = new UIMenuColorPanel("Second Colour", ColorPanelType.Makeup);
            UIMenuPercentagePanel _ppLipOpacity = new UIMenuPercentagePanel("Opacity", "0%", "100%");
            _mlstAppearanceLipStick.AddPanel(_ppLipOpacity);
            _mlstAppearanceLipStick.AddPanel(_cpLipColor1);
            //_mlstAppearanceLipStick.AddPanel(_cpLipColor2);

            _menuAppearance.OnColorPanelChange += (_menu, _panel, _index) =>
            {
                if (_menu == _mlstAppearanceHair)
                {
                    if (_panel == _menu.Panels[0])
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

            #region Advanced Apparel

            string[] clothingCategoryNames = new string[10] { "Masks", "Upper Body", "Lower Body", "Bags & Parachutes", "Shoes", "Scarfs & Chains", "Shirt & Accessory", "Body Armor", "Badges & Logos", "Jackets" };

            for (int i = 0; i < clothingCategoryNames.Length; i++)
            {
                int currentVariationIndex = 255;
                int currentVariationTextureIndex = 255;
                ePedComponents componentIndex = ePedComponents.Hair;

                if (i == 0)
                {
                    componentIndex = ePedComponents.Mask;
                    currentVariationIndex = _characterSkin.CharacterOutfit.ComponentDrawables.Mask;
                    currentVariationTextureIndex = _characterSkin.CharacterOutfit.ComponentTextures.Mask;
                }
                else if (i == 1)
                {
                    componentIndex = ePedComponents.Torso;
                    currentVariationIndex = _characterSkin.CharacterOutfit.ComponentDrawables.Torso;
                    currentVariationTextureIndex = _characterSkin.CharacterOutfit.ComponentTextures.Torso;
                }
                else if (i == 2)
                {
                    componentIndex = ePedComponents.Leg;
                    currentVariationIndex = _characterSkin.CharacterOutfit.ComponentDrawables.Leg;
                    currentVariationTextureIndex = _characterSkin.CharacterOutfit.ComponentTextures.Leg;
                }
                else if (i == 3)
                {
                    componentIndex = ePedComponents.BagOrParachute;
                    currentVariationIndex = _characterSkin.CharacterOutfit.ComponentDrawables.BagOrParachute;
                    currentVariationTextureIndex = _characterSkin.CharacterOutfit.ComponentTextures.BagOrParachute;
                }
                else if (i == 4)
                {
                    componentIndex = ePedComponents.Shoes;
                    currentVariationIndex = _characterSkin.CharacterOutfit.ComponentDrawables.Shoes;
                    currentVariationTextureIndex = _characterSkin.CharacterOutfit.ComponentTextures.Shoes;
                }
                else if (i == 5)
                {
                    componentIndex = ePedComponents.Accessory;
                    currentVariationIndex = _characterSkin.CharacterOutfit.ComponentDrawables.Accessory;
                    currentVariationTextureIndex = _characterSkin.CharacterOutfit.ComponentTextures.Accessory;
                }
                else if (i == 6)
                {
                    componentIndex = ePedComponents.Undershirt;
                    currentVariationIndex = _characterSkin.CharacterOutfit.ComponentDrawables.Undershirt;
                    currentVariationTextureIndex = _characterSkin.CharacterOutfit.ComponentTextures.Undershirt;
                }
                else if (i == 7)
                {
                    componentIndex = ePedComponents.Kevlar;
                    currentVariationIndex = _characterSkin.CharacterOutfit.ComponentDrawables.Kevlar;
                    currentVariationTextureIndex = _characterSkin.CharacterOutfit.ComponentTextures.Kevlar;
                }
                else if (i == 8)
                {
                    componentIndex = ePedComponents.Badge;
                    currentVariationIndex = _characterSkin.CharacterOutfit.ComponentDrawables.Badge;
                    currentVariationTextureIndex = _characterSkin.CharacterOutfit.ComponentTextures.Badge;
                }
                else if (i == 9)
                {
                    componentIndex = ePedComponents.Torso_2;
                    currentVariationIndex = _characterSkin.CharacterOutfit.ComponentDrawables.Torso_2;
                    currentVariationTextureIndex = _characterSkin.CharacterOutfit.ComponentTextures.Torso_2;
                }

                int maxDrawables = GetNumberOfPedDrawableVariations(_playerPed.Handle, (int)componentIndex);
                int maxTextures = GetNumberOfPedTextureVariations(_playerPed.Handle, i, currentVariationIndex);

                List<dynamic> menuItemList = new List<dynamic>();

                for (int x = 0; x < maxDrawables; x++)
                {
                    menuItemList.Add($"#{x}/{maxDrawables}");
                }

                UIMenuListItem mListItem = new UIMenuListItem(clothingCategoryNames[i], menuItemList, currentVariationIndex,
                    $"Select a drawable using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{currentVariationTextureIndex + 1} (of {maxTextures}).");

                _menuAdvancedApparel.AddItem(mListItem);
            }

            int _advMenuApparelIndex = 1;

            _menuAdvancedApparel.OnIndexChange += (_sender, _newIndex) =>
            {
                if (_newIndex == 0) // Masks
                    _advMenuApparelIndex = 1;
                if (_newIndex == 1) // Upper Body
                    _advMenuApparelIndex = 3;
                if (_newIndex == 2) // Lower Body
                    _advMenuApparelIndex = 4;
                if (_newIndex == 3) // Bags & Parachutes
                    _advMenuApparelIndex = 5;
                if (_newIndex == 4) // Shoes
                    _advMenuApparelIndex = 6;
                if (_newIndex == 5) // Scarfs & Chains
                    _advMenuApparelIndex = 7;
                if (_newIndex == 6) // Shirt & Accessory
                    _advMenuApparelIndex = 8;
                if (_newIndex == 7) // Body Armor & Accessory
                    _advMenuApparelIndex = 9;
                if (_newIndex == 8) // Badges & Logos
                    _advMenuApparelIndex = 10;
                if (_newIndex == 9) // Shirt Overlay & Jackets
                    _advMenuApparelIndex = 11;
            };

            _menuAdvancedApparel.OnListChange += (_sender, _listItem, _listIndex) => {
                int textureIndex = GetPedTextureVariation(_playerPed.Handle, _advMenuApparelIndex);
                int newTextureIndex = 0;
                int maxTextures = GetNumberOfPedTextureVariations(_playerPed.Handle, _advMenuApparelIndex, _listItem.Index);

                if (_advMenuApparelIndex == 1)
                    _characterSkin.CharacterOutfit.ComponentDrawables.Mask = _listItem.Index;
                else if (_advMenuApparelIndex == 3)
                    _characterSkin.CharacterOutfit.ComponentDrawables.Torso = _listItem.Index;
                else if (_advMenuApparelIndex == 4)
                    _characterSkin.CharacterOutfit.ComponentDrawables.Leg = _listItem.Index;
                else if (_advMenuApparelIndex == 5)
                    _characterSkin.CharacterOutfit.ComponentDrawables.BagOrParachute = _listItem.Index;
                else if (_advMenuApparelIndex == 6)
                    _characterSkin.CharacterOutfit.ComponentDrawables.Shoes = _listItem.Index;
                else if (_advMenuApparelIndex == 7)
                    _characterSkin.CharacterOutfit.ComponentDrawables.Accessory = _listItem.Index;
                else if (_advMenuApparelIndex == 8)
                    _characterSkin.CharacterOutfit.ComponentDrawables.Undershirt = _listItem.Index;
                else if (_advMenuApparelIndex == 9)
                    _characterSkin.CharacterOutfit.ComponentDrawables.Kevlar = _listItem.Index;
                else if (_advMenuApparelIndex == 10)
                    _characterSkin.CharacterOutfit.ComponentDrawables.Badge = _listItem.Index;
                else if (_advMenuApparelIndex == 11)
                    _characterSkin.CharacterOutfit.ComponentDrawables.Torso_2 = _listItem.Index;

                UpdateDress(_playerPed.Handle, _characterSkin.CharacterOutfit);

                _listItem.Description = $"Select an item using the arrow keys and press ~o~enter~s~ to cycle through all available styles. Currently selected style: #{newTextureIndex + 1} (of {maxTextures}).";
                _sender.UpdateDescription();
            };

            _menuAdvancedApparel.OnListSelect += (_sender, _listItem, _listIndex) =>
            {
                int textureIndex = GetPedTextureVariation(_playerPed.Handle, _advMenuApparelIndex);
                int newTextureIndex = (GetNumberOfPedTextureVariations(_playerPed.Handle, _advMenuApparelIndex, _listItem.Index) - 1) < (textureIndex + 1) ? 0 : textureIndex + 1;
                int maxTextures = GetNumberOfPedTextureVariations(_playerPed.Handle, _advMenuApparelIndex, _listItem.Index);

                if (_advMenuApparelIndex == 1)
                    _characterSkin.CharacterOutfit.ComponentTextures.Mask = newTextureIndex;
                else if (_advMenuApparelIndex == 3)
                    _characterSkin.CharacterOutfit.ComponentTextures.Torso = newTextureIndex;
                else if (_advMenuApparelIndex == 4)
                    _characterSkin.CharacterOutfit.ComponentTextures.Leg = newTextureIndex;
                else if (_advMenuApparelIndex == 5)
                    _characterSkin.CharacterOutfit.ComponentTextures.BagOrParachute = newTextureIndex;
                else if (_advMenuApparelIndex == 6)
                    _characterSkin.CharacterOutfit.ComponentTextures.Shoes = newTextureIndex;
                else if (_advMenuApparelIndex == 7)
                    _characterSkin.CharacterOutfit.ComponentTextures.Accessory = newTextureIndex;
                else if (_advMenuApparelIndex == 8)
                    _characterSkin.CharacterOutfit.ComponentTextures.Undershirt = newTextureIndex;
                else if (_advMenuApparelIndex == 9)
                    _characterSkin.CharacterOutfit.ComponentTextures.Kevlar = newTextureIndex;
                else if (_advMenuApparelIndex == 10)
                    _characterSkin.CharacterOutfit.ComponentTextures.Badge = newTextureIndex;
                else if (_advMenuApparelIndex == 11)
                    _characterSkin.CharacterOutfit.ComponentTextures.Torso_2 = newTextureIndex;

                UpdateDress(_playerPed.Handle, _characterSkin.CharacterOutfit);

                _listItem.Description = $"Select an item using the arrow keys and press ~o~enter~s~ to cycle through all available styles. Currently selected style: #{newTextureIndex + 1} (of {maxTextures}).";
                _sender.UpdateDescription();
            };

            #endregion

            #region Apparel Basic

            var styleList = new List<dynamic>();
            for (int i = 0; i < 8; i++)
                styleList.Add(GetLabelText("FACE_A_STY_" + i));

            var outfitList = new List<dynamic>();
            for (int i = 0; i < 8; i++)
                outfitList.Add(GetLabelText(CharacterCreatorData.GetOutfit(i, _characterSkin.IsMale)));

            var hatList = new List<dynamic>() { GetLabelText("FACE_OFF") };
            var glassesList = new List<dynamic>() { GetLabelText("FACE_OFF") };

            if (_characterSkin.IsMale)
            {
                foreach (var _hat in CharacterCreatorData.HatsMale)
                    hatList.Add(GetLabelText(_hat.label));
                foreach (var _glas in CharacterCreatorData.GlassesMale)
                    glassesList.Add(GetLabelText(_glas.label));
            }
            else
            {
                foreach (var _hat in CharacterCreatorData.HatsFemale)
                    hatList.Add(GetLabelText(_hat.label));
                foreach (var _glas in CharacterCreatorData.GlassesFemale)
                    glassesList.Add(GetLabelText(_glas.label));
            }

            UIMenuListItem _mlstApparelStyle = new(GetLabelText("FACE_APP_STY"), styleList, 0, GetLabelText("FACE_APPA_H"));
            UIMenuListItem _mlstApparelOutfit = new(GetLabelText("FACE_APP_OUT"), outfitList, 0, GetLabelText("FACE_APPA_H"));
            UIMenuListItem _mlstApparelHat = new(GetLabelText("FACE_HAT"), hatList, 0, GetLabelText("FACE_APPA_H"));
            UIMenuListItem _mlstApparelGlasses = new(GetLabelText("FACE_GLS"), glassesList, 0, GetLabelText("FACE_APPA_H"));

            //UIMenuListItem outfit = new UIMenuListItem(GetLabelText("FACE_APP_OUT"));
            _menuApparel.AddItem(_mlstApparelStyle);
            _menuApparel.AddItem(_mlstApparelOutfit);
            _menuApparel.AddItem(_mlstApparelHat);
            _menuApparel.AddItem(_mlstApparelGlasses);

            int first = 0;
            _menuApparel.OnListChange += (sender, item, index) =>
            {
                var id = _playerPed.Handle;
                if (item == _mlstApparelStyle)
                {
                    List<dynamic> list = new();

                    first = index * 8;
                    int maxNum = first + 8;

                    for (int i = first; i < maxNum; i++)
                    {
                        list.Add(GetLabelText(CharacterCreatorData.GetOutfit(i, _characterSkin.IsMale)));
                    }
                    
                    _mlstApparelOutfit.ChangeList(list, 0);

                    int[][] aa = GetCharacterOutfitSettings(_characterSkin.IsMale, first);
                    var comp = new ComponentDrawables(
                        aa[0][0],
                        aa[0][1],
                        aa[0][2],
                        aa[0][3],
                        aa[0][4],
                        aa[0][5],
                        aa[0][6],
                        aa[0][7],
                        aa[0][8],
                        aa[0][9],
                        aa[0][10],
                        aa[0][11]
                    );
                    var text = new ComponentDrawables(
                        aa[1][0],
                        aa[1][1],
                        aa[1][2],
                        aa[1][3],
                        aa[1][4],
                        aa[1][5],
                        aa[1][6],
                        aa[1][7],
                        aa[1][8],
                        aa[1][9],
                        aa[1][10],
                        aa[1][11]
                    );
                    var _prop = new PropDrawables(
                        GetPedPropIndex(id, 0),
                        GetPedPropIndex(id, 1),
                        GetPedPropIndex(id, 2),
                        GetPedPropIndex(id, 3),
                        GetPedPropIndex(id, 4),
                        GetPedPropIndex(id, 5),
                        GetPedPropIndex(id, 6),
                        GetPedPropIndex(id, 7),
                        GetPedPropIndex(id, 8)
                    );
                    var _proptxt = new PropDrawables(
                        GetPedPropTextureIndex(id, 0),
                        GetPedPropTextureIndex(id, 1),
                        GetPedPropTextureIndex(id, 2),
                        GetPedPropTextureIndex(id, 3),
                        GetPedPropTextureIndex(id, 4),
                        GetPedPropTextureIndex(id, 5),
                        GetPedPropTextureIndex(id, 6),
                        GetPedPropTextureIndex(id, 7),
                        GetPedPropTextureIndex(id, 8)
                    );
                    _characterSkin.CharacterOutfit = new("", "", comp, text, _prop, _proptxt);
                }
                else if (item == _mlstApparelOutfit)
                {
                    int[][] aa = GetCharacterOutfitSettings(_characterSkin.IsMale, (index + first));
                    var comp = new ComponentDrawables(
                        aa[0][0],
                        aa[0][1],
                        aa[0][2],
                        aa[0][3],
                        aa[0][4],
                        aa[0][5],
                        aa[0][6],
                        aa[0][7],
                        aa[0][8],
                        aa[0][9],
                        aa[0][10],
                        aa[0][11]
                    );
                    var text = new ComponentDrawables(
                        aa[1][0],
                        aa[1][1],
                        aa[1][2],
                        aa[1][3],
                        aa[1][4],
                        aa[1][5],
                        aa[1][6],
                        aa[1][7],
                        aa[1][8],
                        aa[1][9],
                        aa[1][10],
                        aa[1][11]
                    );
                    var _prop = new PropDrawables(
                        GetPedPropIndex(id, 0),
                        GetPedPropIndex(id, 1),
                        GetPedPropIndex(id, 2),
                        GetPedPropIndex(id, 3),
                        GetPedPropIndex(id, 4),
                        GetPedPropIndex(id, 5),
                        GetPedPropIndex(id, 6),
                        GetPedPropIndex(id, 7),
                        GetPedPropIndex(id, 8)
                    );
                    var _proptxt = new PropDrawables(
                        GetPedPropTextureIndex(id, 0),
                        GetPedPropTextureIndex(id, 1),
                        GetPedPropTextureIndex(id, 2),
                        GetPedPropTextureIndex(id, 3),
                        GetPedPropTextureIndex(id, 4),
                        GetPedPropTextureIndex(id, 5),
                        GetPedPropTextureIndex(id, 6),
                        GetPedPropTextureIndex(id, 7),
                        GetPedPropTextureIndex(id, 8)
                    );
                    _characterSkin.CharacterOutfit = new("", "", comp, text, _prop, _proptxt);
                }
                else if (item == _mlstApparelHat)
                {
                    if (index == 0)
                    {
                        SetPedPropIndex(id, (int)ePedProps.HatOrMask, -1, -1, false);
                        ClearPedProp(id, (int)ePedProps.HatOrMask);
                        Logger.Debug($"Removing hat");
                    }
                    else
                    {
                        ShopPed.PedComponentData prop = new();
                        if (_characterSkin.IsMale)
                            prop = CharacterCreatorData.HatsMale[index - 1];
                        else
                            prop = CharacterCreatorData.HatsFemale[index - 1];
                        var comp = new ComponentDrawables(
                            GetPedDrawableVariation(id, 0),
                            GetPedDrawableVariation(id, 1),
                            GetPedDrawableVariation(id, 2),
                            GetPedDrawableVariation(id, 3),
                            GetPedDrawableVariation(id, 4),
                            GetPedDrawableVariation(id, 5),
                            GetPedDrawableVariation(id, 6),
                            GetPedDrawableVariation(id, 7),
                            GetPedDrawableVariation(id, 8),
                            GetPedDrawableVariation(id, 9),
                            GetPedDrawableVariation(id, 10),
                            GetPedDrawableVariation(id, 11)
                        );
                        var text = new ComponentDrawables(
                            GetPedTextureVariation(id, 0),
                            GetPedTextureVariation(id, 1),
                            GetPedTextureVariation(id, 2),
                            GetPedTextureVariation(id, 3),
                            GetPedTextureVariation(id, 4),
                            GetPedTextureVariation(id, 5),
                            GetPedTextureVariation(id, 6),
                            GetPedTextureVariation(id, 7),
                            GetPedTextureVariation(id, 8),
                            GetPedTextureVariation(id, 9),
                            GetPedTextureVariation(id, 10),
                            GetPedTextureVariation(id, 11)
                        );
                        var _prop = new PropDrawables(
                            prop.drawable,
                            GetPedPropIndex(id, 1),
                            GetPedPropIndex(id, 2),
                            GetPedPropIndex(id, 3),
                            GetPedPropIndex(id, 4),
                            GetPedPropIndex(id, 5),
                            GetPedPropIndex(id, 6),
                            GetPedPropIndex(id, 7),
                            GetPedPropIndex(id, 8)
                        );
                        var _proptxt = new PropDrawables(
                            prop.texture,
                            GetPedPropTextureIndex(id, 1),
                            GetPedPropTextureIndex(id, 2),
                            GetPedPropTextureIndex(id, 3),
                            GetPedPropTextureIndex(id, 4),
                            GetPedPropTextureIndex(id, 5),
                            GetPedPropTextureIndex(id, 6),
                            GetPedPropTextureIndex(id, 7),
                            GetPedPropTextureIndex(id, 8)
                        );
                        _characterSkin.CharacterOutfit = new("", "", comp, text, _prop, _proptxt);
                    }
                }
                else if (item == _mlstApparelGlasses)
                {
                    if (index == 0)
                    {
                        SetPedPropIndex(id, (int)ePedProps.Glasses, -1, -1, false);
                        ClearPedProp(id, (int)ePedProps.Glasses);
                    }
                    else
                    {
                        ShopPed.PedComponentData prop = new();
                        if (_characterSkin.IsMale)
                            prop = CharacterCreatorData.GlassesMale[index - 1];
                        else
                            prop = CharacterCreatorData.GlassesFemale[index - 1];

                        var comp = new ComponentDrawables(
                            GetPedDrawableVariation(id, 0),
                            GetPedDrawableVariation(id, 1),
                            GetPedDrawableVariation(id, 2),
                            GetPedDrawableVariation(id, 3),
                            GetPedDrawableVariation(id, 4),
                            GetPedDrawableVariation(id, 5),
                            GetPedDrawableVariation(id, 6),
                            GetPedDrawableVariation(id, 7),
                            GetPedDrawableVariation(id, 8),
                            GetPedDrawableVariation(id, 9),
                            GetPedDrawableVariation(id, 10),
                            GetPedDrawableVariation(id, 11)
                        );
                        var text = new ComponentDrawables(
                            GetPedTextureVariation(id, 0),
                            GetPedTextureVariation(id, 1),
                            GetPedTextureVariation(id, 2),
                            GetPedTextureVariation(id, 3),
                            GetPedTextureVariation(id, 4),
                            GetPedTextureVariation(id, 5),
                            GetPedTextureVariation(id, 6),
                            GetPedTextureVariation(id, 7),
                            GetPedTextureVariation(id, 8),
                            GetPedTextureVariation(id, 9),
                            GetPedTextureVariation(id, 10),
                            GetPedTextureVariation(id, 11)
                        );
                        var _prop = new PropDrawables(
                            GetPedPropIndex(id, 0),
                            prop.drawable,
                            GetPedPropIndex(id, 2),
                            GetPedPropIndex(id, 3),
                            GetPedPropIndex(id, 4),
                            GetPedPropIndex(id, 5),
                            GetPedPropIndex(id, 6),
                            GetPedPropIndex(id, 7),
                            GetPedPropIndex(id, 8)
                        );
                        var _proptxt = new PropDrawables(
                            GetPedPropTextureIndex(id, 0),
                            prop.texture,
                            GetPedPropTextureIndex(id, 2),
                            GetPedPropTextureIndex(id, 3),
                            GetPedPropTextureIndex(id, 4),
                            GetPedPropTextureIndex(id, 5),
                            GetPedPropTextureIndex(id, 6),
                            GetPedPropTextureIndex(id, 7),
                            GetPedPropTextureIndex(id, 8)
                        );
                        _characterSkin.CharacterOutfit = new("", "", comp, text, _prop, _proptxt);
                    }
                }
                UpdateDress(_playerPed.Handle, _characterSkin.CharacterOutfit);
                // _playerPed.TaskEvidenceClothes(GetLineupOrCreationAnimation(true, false));
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
                            _playerPed.TaskCreatorClothes(GetLineupOrCreationAnimation(true, false));

                        if (_newMenu == _menuApparel)
                        {
                            // DIVIDE BY ZERO ERROR

                            var outfitList = new List<dynamic>();
                            for (int i = 0; i < 8; i++)
                                outfitList.Add(GetLabelText(CharacterCreatorData.GetOutfit(i, _characterSkin.IsMale)));

                            _mlstApparelOutfit.ChangeList(outfitList, 0);

                            var hatList = new List<dynamic>() { GetLabelText("FACE_OFF") };
                            var glassesList = new List<dynamic>() { GetLabelText("FACE_OFF") };
                            
                            var hats = _characterSkin.IsMale ? CharacterCreatorData.HatsMale : CharacterCreatorData.HatsFemale;
                            var glasses = _characterSkin.IsMale ? CharacterCreatorData.GlassesMale : CharacterCreatorData.GlassesFemale;

                            foreach (var _hat in hats)
                                hatList.Add(GetLabelText(_hat.label));
                                
                            foreach (var _glas in glasses)
                                glassesList.Add(GetLabelText(_glas.label));

                            _mlstApparelHat.ChangeList(hatList, 0);
                            _mlstApparelGlasses.ChangeList(glassesList, 0);
                        }

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
                        {
                            _playerPed.Heading = _finalHeading;
                            _playerPed.TaskEvidenceClothes(GetLineupOrCreationAnimation(true, false));
                            await BaseScript.Delay(5000);
                            _playerPed.TaskClothesALoop(GetLineupOrCreationAnimation(true, false));
                        }
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

            #region Menu Stats

            int StatMax = 101;
            UIMenuStatsItem stamina = new UIMenuStatsItem(GetLabelText("FACE_STAM"), GetLabelText("FACE_H_STA"), 0, HudColor.HUD_COLOUR_FREEMODE);
            UIMenuPercentagePanel maxstat = new("Remaining Points", "0", "100", StatMax);
            stamina.AddPanel(maxstat);

            UIMenuStatsItem shooting = new UIMenuStatsItem(GetLabelText("FACE_SHOOT"), GetLabelText("FACE_H_SHO"), 0, HudColor.HUD_COLOUR_FREEMODE);
            UIMenuPercentagePanel maxstat1 = new("Remaining Points", "0", "100", StatMax);
            shooting.AddPanel(maxstat1);

            UIMenuStatsItem strength = new UIMenuStatsItem(GetLabelText("FACE_STR"), GetLabelText("FACE_H_STR"), 0, HudColor.HUD_COLOUR_FREEMODE);
            UIMenuPercentagePanel maxstat2 = new("Remaining Points", "0", "100", StatMax);
            strength.AddPanel(maxstat2);

            UIMenuStatsItem stealth = new UIMenuStatsItem(GetLabelText("FACE_STEALTH"), GetLabelText("FACE_H_STE"), 0, HudColor.HUD_COLOUR_FREEMODE);
            UIMenuPercentagePanel maxstat3 = new("Remaining Points", "0", "100", StatMax);
            stealth.AddPanel(maxstat3);

            UIMenuStatsItem flying = new UIMenuStatsItem(GetLabelText("FACE_FLY"), GetLabelText("FACE_H_FLY"), 0, HudColor.HUD_COLOUR_FREEMODE);
            UIMenuPercentagePanel maxstat4 = new("Remaining Points", "0", "100", StatMax);
            flying.AddPanel(maxstat4);

            UIMenuStatsItem driving = new UIMenuStatsItem(GetLabelText("FACE_DRIV"), GetLabelText("FACE_H_DRI"), 0, HudColor.HUD_COLOUR_FREEMODE);
            UIMenuPercentagePanel maxstat5 = new("Remaining Points", "0", "100", StatMax);
            driving.AddPanel(maxstat5);

            UIMenuStatsItem lungs = new UIMenuStatsItem(GetLabelText("FACE_LUNG"), GetLabelText("FACE_H_LCP"), 0, HudColor.HUD_COLOUR_FREEMODE);
            UIMenuPercentagePanel maxstat6 = new("Remaining Points", "0", "100", StatMax);
            lungs.AddPanel(maxstat6);

            _menuStats.AddItem(stamina);
            _menuStats.AddItem(shooting);
            _menuStats.AddItem(strength);
            _menuStats.AddItem(stealth);
            _menuStats.AddItem(flying);
            _menuStats.AddItem(driving);
            _menuStats.AddItem(lungs);

            int _stamina = 0,
                _shooting = 0,
                _strength = 0,
                _stealth = 0,
                _flying = 0,
                _driving = 0,
                _lungs = 0;

            _menuStats.OnStatsItemChanged += (a, b, c) =>
            {
                var max = 0;
                foreach (var item in a.MenuItems)
                {
                    max += (item as UIMenuStatsItem).Value;
                }
                StatMax = 100 - max;
                foreach (var item in a.MenuItems)
                {
                    (item.Panels[0] as UIMenuPercentagePanel).Percentage = StatMax;
                }
                if (b == stamina)
                {
                    if (StatMax > 0)
                        _stamina = c;
                    else
                        b.Value = _stamina;
                }
                else if (b == shooting)
                {
                    if (StatMax > 0)
                        _shooting = c;
                    else
                        b.Value = _shooting;
                }
                else if (b == strength)
                {
                    if (StatMax > 0)
                        _strength = c;
                    else
                        b.Value = _strength;
                }
                else if (b == stealth)
                {
                    if (StatMax > 0)
                        _stealth = c;
                    else
                        b.Value = _stealth;
                }
                else if (b == flying)
                {
                    if (StatMax > 0)
                        _flying = c;
                    else
                        b.Value = _flying;
                }
                else if (b == driving)
                {
                    if (StatMax > 0)
                        _driving = c;
                    else
                        b.Value = _driving;
                }
                else if (b == lungs)
                {
                    if (StatMax > 0)
                        _lungs = c;
                    else
                        b.Value = _lungs;
                }
                _user.ActiveCharacter.Stats = new(_stamina, _strength, _lungs, _stealth, _shooting, _driving, _flying);
            };

            #endregion

            #region Save and Exit

            UIMenuItem miSaveAndExit = new UIMenuItem(
            "Save Character",
            "This will save the character and throw you into the deepend.",
            HudColor.HUD_COLOUR_FREEMODE_DARK, HudColor.HUD_COLOUR_FREEMODE);
            
            miSaveAndExit.SetRightBadge(BadgeIcon.TICK);
            _menuBase.AddItem(miSaveAndExit);

            bool isProcessing = false;
            
            miSaveAndExit.Activated += async (selectedItem, index) =>
            {
                if (isProcessing) return;
                isProcessing = true;

                DisplayRadar(false);
                DisplayHud(true);

                _user.ActiveCharacter.Skin = _characterSkin;
                _user.ActiveCharacter.IsRegistered = true;
                bool isSaved = await _user.ActiveCharacter.OnSaveCharacterAsync();

                if (!isSaved)
                {
                    isProcessing = false;
                    GameInterface.Hud.ShowNotificationError("Character Failed to save. If this continues, please open a support ticket.");
                    DisplayRadar(false);
                    DisplayHud(false);
                    return;
                }

                Instance.DetachTickHandler(OnCharacterCreationMenuControlsAsync);
                Instance.DetachTickHandler(OnCharacterCreationWarningAsync);

                GameInterface.Hud.ShowNotificationSuccess("Character Saved");
                await BaseScript.Delay(1000);
                await _playerPed.TaskPlayOutroOfCharacterCreationRoom(GetLineupOrCreationAnimation(true, false));
                await GameInterface.Hud.FadeOut(800);
                _menuBase.Visible = false;
                mugshotBoardAttachment.IsAttached = false;
                GameInterface.Hud.MenuPool.CloseAllMenus();
                Game.PlayerPed.Detach();

                RenderScriptCams(false, true, 0, false, false);
                World.DestroyAllCameras();

                RemoveAnimDict("mp_character_creation@lineup@male_a");
                RemoveAnimDict("mp_character_creation@lineup@male_b");
                RemoveAnimDict("mp_character_creation@lineup@female_a");
                RemoveAnimDict("mp_character_creation@lineup@female_b");
                RemoveAnimDict("mp_character_creation@customise@male_a");
                RemoveAnimDict("mp_character_creation@customise@female_a");

                ReleaseNamedScriptAudioBank("Mugshot_Character_Creator");
                ReleaseNamedScriptAudioBank("DLC_GTAO/MUGSHOT_ROOM");

                Vector3 position = _cityHall.AsVector();

                Game.PlayerPed.Position = position.GetGroundWithWaterTest();
                Game.PlayerPed.IsPositionFrozen = true;

                await BaseScript.Delay(1000);
                await GameInterface.Hud.FadeIn(1000);
                
                DisplayRadar(true);
                DisplayHud(true);

                Game.PlayerPed.IsPositionFrozen = false;
            };

            #endregion

            Instance.AttachTickHandler(OnCharacterCreationMenuControlsAsync);

            if (!_menuBase.Visible)
                _menuBase.Visible = true;

            if (Screen.LoadingPrompt.IsActive)
                Screen.LoadingPrompt.Hide();
        }

        #region Control Ticks
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
                        _playerPed.TaskLookLeft(GetLineupOrCreationAnimation(true, false));
                    }
                }
                else if (IsControlRightBumperPressed)
                {
                    if (!_isPedLookingRight)
                    {
                        _isPedLookingRight = true;
                        _playerPed.TaskLookRight(GetLineupOrCreationAnimation(true, false));
                    }
                }
                else
                {
                    if (_isPedLookingRight)
                        _playerPed.TaskStopLookingRight(GetLineupOrCreationAnimation(true, false));
                    else if (_isPedLookingLeft)
                        _playerPed.TaskStopLookingLeft(GetLineupOrCreationAnimation(true, false));
                    
                    _isPedLookingLeft = _isPedLookingRight = false;
                }
            }

            if (_menuAppearance.Visible)
            {
                int _frontendLeftTrigger = (int)Control.FrontendLt;
                int _frontendRightTrigger = (int)Control.FrontendRt;

                if (_mlstAppearanceHair.Selected)
                {
                    UIMenuColorPanel uIMenuColorPanel = (_mlstAppearanceHair.Panels[0] as UIMenuColorPanel);
                    int color = uIMenuColorPanel.CurrentSelection;

                    if (IsControlPressed(2, _frontendLeftTrigger) || IsDisabledControlPressed(2, _frontendLeftTrigger)
                        || IsControlPressed(2, _frontendRightTrigger) || IsDisabledControlPressed(2, _frontendRightTrigger))
                    {
                        if (IsControlPressed(2, _frontendRightTrigger) || IsDisabledControlPressed(2, _frontendRightTrigger) && !IsInputDisabled(2))
                        {
                            ++color;
                            if (color > MAX_CREATOR_COLOR)
                                color = 0;
                        }

                        if (IsControlPressed(2, _frontendLeftTrigger) || IsDisabledControlPressed(2, _frontendLeftTrigger) && !IsInputDisabled(2))
                        {
                            --color;
                            if (color < 0)
                                color = MAX_CREATOR_COLOR;
                        }

                        _characterSkin.Hair.Color[0] = color;
                        uIMenuColorPanel.CurrentSelection = color;

                        await BaseScript.Delay(100);
                    }
                }

                if (_mlstAppearanceEyeBrows.Selected)
                {
                    UIMenuPercentagePanel uIMenuPercentagePanel = (_mlstAppearanceEyeBrows.Panels[0] as UIMenuPercentagePanel);
                    UIMenuColorPanel uIMenuColorPanel = (_mlstAppearanceEyeBrows.Panels[1] as UIMenuColorPanel);
                    int color = uIMenuColorPanel.CurrentSelection;
                    float currentOpacity = uIMenuPercentagePanel.Percentage / 100;

                    if (IsControlPressed(2, _frontendLeftTrigger) || IsDisabledControlPressed(2, _frontendLeftTrigger)
                        || IsControlPressed(2, _frontendRightTrigger) || IsDisabledControlPressed(2, _frontendRightTrigger)
                        || IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6)
                        || IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5))
                    {
                        if (IsControlPressed(2, _frontendRightTrigger) || IsDisabledControlPressed(2, _frontendRightTrigger) && !IsInputDisabled(2))
                        {
                            ++color;
                            if (color > MAX_CREATOR_COLOR)
                                color = 0;

                            await BaseScript.Delay(100);
                        }

                        if (IsControlPressed(2, _frontendLeftTrigger) || IsDisabledControlPressed(2, _frontendLeftTrigger) && !IsInputDisabled(2))
                        {
                            --color;
                            if (color < 0)
                                color = MAX_CREATOR_COLOR;

                            await BaseScript.Delay(100);
                        }

                        if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2))
                        {
                            currentOpacity += 0.01f;
                            if (currentOpacity > 1f)
                                currentOpacity = 1f;
                            
                            await BaseScript.Delay(0);
                        }

                        if (IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2))
                        {
                            currentOpacity -= 0.01f;
                            if (currentOpacity < 0)
                                currentOpacity = 0;

                            await BaseScript.Delay(0);
                        }

                        _characterSkin.Face.Eyebrow.Color[0] = color;
                        uIMenuColorPanel.CurrentSelection = color;
                        _characterSkin.Face.Eyebrow.Opacity = currentOpacity;
                        uIMenuPercentagePanel.Percentage = currentOpacity * 100;
                    }
                }

                if (_mlstAppearanceBeard.Selected)
                {
                    UIMenuPercentagePanel uIMenuPercentagePanel = (_mlstAppearanceBeard.Panels[0] as UIMenuPercentagePanel);
                    UIMenuColorPanel uIMenuColorPanel = (_mlstAppearanceBeard.Panels[1] as UIMenuColorPanel);
                    int color = uIMenuColorPanel.CurrentSelection;
                    float currentOpacity = uIMenuPercentagePanel.Percentage / 100;

                    if (IsControlPressed(2, _frontendLeftTrigger) || IsDisabledControlPressed(2, _frontendLeftTrigger)
                        || IsControlPressed(2, _frontendRightTrigger) || IsDisabledControlPressed(2, _frontendRightTrigger)
                        || IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6)
                        || IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5))
                    {
                        if (IsControlPressed(2, _frontendRightTrigger) || IsDisabledControlPressed(2, _frontendRightTrigger) && !IsInputDisabled(2))
                        {
                            ++color;
                            if (color > MAX_CREATOR_COLOR)
                                color = 0;

                            await BaseScript.Delay(100);
                        }

                        if (IsControlPressed(2, _frontendLeftTrigger) || IsDisabledControlPressed(2, _frontendLeftTrigger) && !IsInputDisabled(2))
                        {
                            --color;
                            if (color < 0)
                                color = MAX_CREATOR_COLOR;

                            await BaseScript.Delay(100);
                        }

                        if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2))
                        {
                            currentOpacity += 0.01f;
                            if (currentOpacity > 1f)
                                currentOpacity = 1f;

                            await BaseScript.Delay(0);
                        }

                        if (IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2))
                        {
                            currentOpacity -= 0.01f;
                            if (currentOpacity < 0)
                                currentOpacity = 0;

                            await BaseScript.Delay(0);
                        }

                        _characterSkin.Face.Beard.Color[0] = color;
                        uIMenuColorPanel.CurrentSelection = color;
                        _characterSkin.Face.Beard.Opacity = currentOpacity;
                        uIMenuPercentagePanel.Percentage = currentOpacity * 100;
                    }
                }

                if (_mlstAppearanceBlusher.Selected)
                {
                    UIMenuPercentagePanel uIMenuPercentagePanel = (_mlstAppearanceBlusher.Panels[0] as UIMenuPercentagePanel);
                    UIMenuColorPanel uIMenuColorPanel = (_mlstAppearanceBlusher.Panels[1] as UIMenuColorPanel);
                    int color = uIMenuColorPanel.CurrentSelection;
                    float currentOpacity = uIMenuPercentagePanel.Percentage / 100;

                    if (IsControlPressed(2, _frontendLeftTrigger) || IsDisabledControlPressed(2, _frontendLeftTrigger)
                        || IsControlPressed(2, _frontendRightTrigger) || IsDisabledControlPressed(2, _frontendRightTrigger)
                        || IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6)
                        || IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5))
                    {
                        if (IsControlPressed(2, _frontendRightTrigger) || IsDisabledControlPressed(2, _frontendRightTrigger) && !IsInputDisabled(2))
                        {
                            ++color;
                            if (color > MAX_CREATOR_COLOR)
                                color = 0;

                            await BaseScript.Delay(100);
                        }

                        if (IsControlPressed(2, _frontendLeftTrigger) || IsDisabledControlPressed(2, _frontendLeftTrigger) && !IsInputDisabled(2))
                        {
                            --color;
                            if (color < 0)
                                color = MAX_CREATOR_COLOR;

                            await BaseScript.Delay(100);
                        }

                        if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2))
                        {
                            currentOpacity += 0.01f;
                            if (currentOpacity > 1f)
                                currentOpacity = 1f;

                            await BaseScript.Delay(0);
                        }

                        if (IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2))
                        {
                            currentOpacity -= 0.01f;
                            if (currentOpacity < 0)
                                currentOpacity = 0;

                            await BaseScript.Delay(0);
                        }

                        _characterSkin.Face.Blusher.Color[0] = color;
                        uIMenuColorPanel.CurrentSelection = color;
                        _characterSkin.Face.Blusher.Opacity = currentOpacity;
                        uIMenuPercentagePanel.Percentage = currentOpacity * 100;
                    }
                }

                if (_mlstAppearanceLipStick.Selected)
                {
                    UIMenuPercentagePanel uIMenuPercentagePanel = (_mlstAppearanceLipStick.Panels[0] as UIMenuPercentagePanel);
                    UIMenuColorPanel uIMenuColorPanel = (_mlstAppearanceLipStick.Panels[1] as UIMenuColorPanel);
                    int color = uIMenuColorPanel.CurrentSelection;
                    float currentOpacity = uIMenuPercentagePanel.Percentage / 100;

                    if (IsControlPressed(2, _frontendLeftTrigger) || IsDisabledControlPressed(2, _frontendLeftTrigger)
                        || IsControlPressed(2, _frontendRightTrigger) || IsDisabledControlPressed(2, _frontendRightTrigger)
                        || IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6)
                        || IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5))
                    {
                        if (IsControlPressed(2, _frontendRightTrigger) || IsDisabledControlPressed(2, _frontendRightTrigger) && !IsInputDisabled(2))
                        {
                            ++color;
                            if (color > MAX_CREATOR_COLOR)
                                color = 0;

                            await BaseScript.Delay(100);
                        }

                        if (IsControlPressed(2, _frontendLeftTrigger) || IsDisabledControlPressed(2, _frontendLeftTrigger) && !IsInputDisabled(2))
                        {
                            --color;
                            if (color < 0)
                                color = MAX_CREATOR_COLOR;

                            await BaseScript.Delay(100);
                        }

                        if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2))
                        {
                            currentOpacity += 0.01f;
                            if (currentOpacity > 1f)
                                currentOpacity = 1f;

                            await BaseScript.Delay(0);
                        }

                        if (IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2))
                        {
                            currentOpacity -= 0.01f;
                            if (currentOpacity < 0)
                                currentOpacity = 0;

                            await BaseScript.Delay(0);
                        }

                        _characterSkin.Face.Lipstick.Color[0] = color;
                        uIMenuColorPanel.CurrentSelection = color;
                        _characterSkin.Face.Lipstick.Opacity = currentOpacity;
                        uIMenuPercentagePanel.Percentage = currentOpacity * 100;
                    }
                }

                if (_mlstAppearanceSkinBlemishes.Selected)
                {
                    UIMenuPercentagePanel uIMenuPercentagePanel = (_mlstAppearanceSkinBlemishes.Panels[0] as UIMenuPercentagePanel);
                    float currentOpacity = uIMenuPercentagePanel.Percentage / 100;

                    if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6)
                        || IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5))
                    {
                        if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2))
                        {
                            currentOpacity += 0.01f;
                            if (currentOpacity > 1f)
                                currentOpacity = 1f;

                            await BaseScript.Delay(0);
                        }

                        if (IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2))
                        {
                            currentOpacity -= 0.01f;
                            if (currentOpacity < 0)
                                currentOpacity = 0;

                            await BaseScript.Delay(0);
                        }

                        _characterSkin.Face.Blemishes.Opacity = currentOpacity;
                        uIMenuPercentagePanel.Percentage = currentOpacity * 100;
                    }
                }

                if (_mlstAppearanceSkinAgeing.Selected)
                {
                    UIMenuPercentagePanel uIMenuPercentagePanel = (_mlstAppearanceSkinAgeing.Panels[0] as UIMenuPercentagePanel);
                    float currentOpacity = uIMenuPercentagePanel.Percentage / 100;

                    if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6)
                        || IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5))
                    {
                        if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2))
                        {
                            currentOpacity += 0.01f;
                            if (currentOpacity > 1f)
                                currentOpacity = 1f;

                            await BaseScript.Delay(0);
                        }

                        if (IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2))
                        {
                            currentOpacity -= 0.01f;
                            if (currentOpacity < 0)
                                currentOpacity = 0;

                            await BaseScript.Delay(0);
                        }

                        _characterSkin.Age.Opacity = currentOpacity;
                        uIMenuPercentagePanel.Percentage = currentOpacity * 100;
                    }
                }
                
                if (_mlstAppearanceSkinComplexion.Selected)
                {
                    UIMenuPercentagePanel uIMenuPercentagePanel = (_mlstAppearanceSkinComplexion.Panels[0] as UIMenuPercentagePanel);
                    float currentOpacity = uIMenuPercentagePanel.Percentage / 100;

                    if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6)
                        || IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5))
                    {
                        if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2))
                        {
                            currentOpacity += 0.01f;
                            if (currentOpacity > 1f)
                                currentOpacity = 1f;

                            await BaseScript.Delay(0);
                        }

                        if (IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2))
                        {
                            currentOpacity -= 0.01f;
                            if (currentOpacity < 0)
                                currentOpacity = 0;

                            await BaseScript.Delay(0);
                        }

                        _characterSkin.Face.Complexion.Opacity = currentOpacity;
                        uIMenuPercentagePanel.Percentage = currentOpacity * 100;
                    }
                }

                if (_mlstAppearanceSkinMoles.Selected)
                {
                    UIMenuPercentagePanel uIMenuPercentagePanel = (_mlstAppearanceSkinMoles.Panels[0] as UIMenuPercentagePanel);
                    float currentOpacity = uIMenuPercentagePanel.Percentage / 100;

                    if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6)
                        || IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5))
                    {
                        if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2))
                        {
                            currentOpacity += 0.01f;
                            if (currentOpacity > 1f)
                                currentOpacity = 1f;

                            await BaseScript.Delay(0);
                        }

                        if (IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2))
                        {
                            currentOpacity -= 0.01f;
                            if (currentOpacity < 0)
                                currentOpacity = 0;

                            await BaseScript.Delay(0);
                        }

                        _characterSkin.Face.Freckles.Opacity = currentOpacity;
                        uIMenuPercentagePanel.Percentage = currentOpacity * 100;
                    }
                }

                if (_mlstAppearanceSkinDamage.Selected)
                {
                    UIMenuPercentagePanel uIMenuPercentagePanel = (_mlstAppearanceSkinDamage.Panels[0] as UIMenuPercentagePanel);
                    float currentOpacity = uIMenuPercentagePanel.Percentage / 100;

                    if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6)
                        || IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5))
                    {
                        if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2))
                        {
                            currentOpacity += 0.01f;
                            if (currentOpacity > 1f)
                                currentOpacity = 1f;

                            await BaseScript.Delay(0);
                        }

                        if (IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2))
                        {
                            currentOpacity -= 0.01f;
                            if (currentOpacity < 0)
                                currentOpacity = 0;

                            await BaseScript.Delay(0);
                        }

                        _characterSkin.Face.SkinDamage.Opacity = currentOpacity;
                        uIMenuPercentagePanel.Percentage = currentOpacity * 100;
                    }
                }

                if (_mlstAppearanceEyeMakeup.Selected)
                {
                    UIMenuPercentagePanel uIMenuPercentagePanel = (_mlstAppearanceEyeMakeup.Panels[0] as UIMenuPercentagePanel);
                    float currentOpacity = uIMenuPercentagePanel.Percentage / 100;

                    if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6)
                        || IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5))
                    {
                        if (IsControlPressed(2, 6) || IsDisabledControlPressed(2, 6) && !IsInputDisabled(2))
                        {
                            currentOpacity += 0.01f;
                            if (currentOpacity > 1f)
                                currentOpacity = 1f;

                            await BaseScript.Delay(0);
                        }

                        if (IsControlPressed(2, 5) || IsDisabledControlPressed(2, 5) && !IsInputDisabled(2))
                        {
                            currentOpacity -= 0.01f;
                            if (currentOpacity < 0)
                                currentOpacity = 0;

                            await BaseScript.Delay(0);
                        }

                        _characterSkin.Face.Makeup.Opacity = currentOpacity;
                        uIMenuPercentagePanel.Percentage = currentOpacity * 100;
                    }
                }

                UpdateFace(_playerPed.Handle, _characterSkin);
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

        #endregion
        
        #region Animations

        string GetLineupOrCreationAnimation(bool isCreator, bool alternateAnimation)
        {
            if (isCreator)
                return _characterSkin.IsMale ? "mp_character_creation@customise@male_a" : "mp_character_creation@customise@female_a";

            if (!isCreator && alternateAnimation)
                return _characterSkin.IsMale ? "mp_character_creation@lineup@male_b" : "mp_character_creation@lineup@female_b";
            
            if (!isCreator)
                return _characterSkin.IsMale ? "mp_character_creation@lineup@male_a" : "mp_character_creation@lineup@female_a";

            return "mp_character_creation@lineup@male_a";
        }

        #endregion

        #region Character Setup and Randomisation
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

                _characterSkin.Model = (uint)(_characterSkin.Gender == 0 ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);
                Model model = (int)_characterSkin.Model;
                await model.Request(1000);
                Game.Player.ChangeModel(model);
                model.MarkAsNoLongerNeeded();
                await Common.MoveToMainThread();
            }

            _playerPed = Game.PlayerPed;
            SetupCharacterAsync();
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

        private async void RandomiseCharacterAppearance(bool update = false)
        {
            bool isMale = _characterSkin.IsMale;

            _characterSkin.Age = new(Common.RANDOM.Next(0, CharacterCreatorData.Ageing.Count), (float)Common.RANDOM.NextDouble());

            await Common.MoveToMainThread();
            
            if (_mlstAppearanceSkinAgeing is not null) _mlstAppearanceSkinAgeing.Index = _characterSkin.Age.Style;

            if (isMale)
            {
                _characterSkin.Face.Beard = new(Common.RANDOM.Next(0, CharacterCreatorData.Beards.Count), (float)Common.RANDOM.NextDouble(),
                    new int[2] { Common.RANDOM.Next(0, 63), 0 });
            }

            await Common.MoveToMainThread();

            if (!isMale)
            {
                _characterSkin.Face.Blusher = new(Common.RANDOM.Next(0, CharacterCreatorData.BlusherFemale.Count), (float)Common.RANDOM.NextDouble(),
                    new int[2] { Common.RANDOM.Next(0, 63), Common.RANDOM.Next(0, 63) });
                _characterSkin.Face.Lipstick = new(Common.RANDOM.Next(0, CharacterCreatorData.Lipstick.Count), (float)Common.RANDOM.NextDouble(),
                    new int[2] { Common.RANDOM.Next(0, 63), Common.RANDOM.Next(0, 63) });
            }

            await Common.MoveToMainThread();

            _characterSkin.Face.Blemishes = new(Common.RANDOM.Next(0, CharacterCreatorData.Blemishes.Count), (float)Common.RANDOM.NextDouble());
            _characterSkin.Face.Makeup = new(0, 1f);

            await Common.MoveToMainThread();

            _characterSkin.Face.Eyebrow = new(Common.RANDOM.Next(0, CharacterCreatorData.Eyebrows.Count), (float)Common.RANDOM.NextDouble(),
                new int[2] { Common.RANDOM.Next(0, 63), 0 });

            await Common.MoveToMainThread();

            _characterSkin.Face.Complexion = new(Common.RANDOM.Next(0, CharacterCreatorData.Complexions.Count), (float)Common.RANDOM.NextDouble());
            await Common.MoveToMainThread();
            _characterSkin.Face.SkinDamage = new(Common.RANDOM.Next(0, CharacterCreatorData.SkinDamage.Count), (float)Common.RANDOM.NextDouble());
            await Common.MoveToMainThread();
            _characterSkin.Face.Freckles = new(Common.RANDOM.Next(0, CharacterCreatorData.MolesAndFreckles.Count), (float)Common.RANDOM.NextDouble());
            await Common.MoveToMainThread();
            int hairCount = isMale ? CharacterCreatorData.HairMale.Count : CharacterCreatorData.HairFemale.Count;
            _characterSkin.Hair = new(Common.RANDOM.Next(1, hairCount), new int[2] { Common.RANDOM.Next(0, 63), 0 });
            await Common.MoveToMainThread();
            _characterSkin.Face.Eye = new(Common.RANDOM.Next(0, CharacterCreatorData.EyeColours.Count));
            await Common.MoveToMainThread();
            _characterSkin.Ears = new(255, 0);

            if (update)
                UpdateFace(_playerPed.Handle, _characterSkin);
        }

        private async Task SetupCharacterAsync()
        {
            RandomiseCharacterParents();
            RandomiseCharacterAppearance();
            UpdateFace(_playerPed.Handle, _characterSkin);
            await Common.MoveToMainThread();
            _playerPed.SetDefaultVariation();
            _playerPed.SetRandomFacialMood();
            await BaseScript.Delay(1000);
            RandomDress();
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
            
            SetPedComponentVariation(Handle, (int)ePedComponents.Hair, skin.Hair.Style, 0, 0);
            SetPedHairColor(Handle, skin.Hair.Color[0], skin.Hair.Color[1]);
            
            SetPedPropIndex(Handle, 2, skin.Ears.Style, skin.Ears.Color, false);

            for (int i = 0; i < skin.Face.Features.Length; i++)
                SetPedFaceFeature(Handle, i, skin.Face.Features[i]);
        }
        
        public async void RandomDress()
        {
            int id = _playerPed.Handle;
            int[][] aa = GetCharacterOutfitSettings(_characterSkin.IsMale, Common.RANDOM.Next(0, 64));
            var comp = new ComponentDrawables(
                aa[0][0],
                aa[0][1],
                aa[0][2],
                aa[0][3],
                aa[0][4],
                aa[0][5],
                aa[0][6],
                aa[0][7],
                aa[0][8],
                aa[0][9],
                aa[0][10],
                aa[0][11]
            );
            var text = new ComponentDrawables(
                aa[1][0],
                aa[1][1],
                aa[1][2],
                aa[1][3],
                aa[1][4],
                aa[1][5],
                aa[1][6],
                aa[1][7],
                aa[1][8],
                aa[1][9],
                aa[1][10],
                aa[1][11]
            );
            var _prop = new PropDrawables(
                GetPedPropIndex(id, 0),
                GetPedPropIndex(id, 1),
                GetPedPropIndex(id, 2),
                GetPedPropIndex(id, 3),
                GetPedPropIndex(id, 4),
                GetPedPropIndex(id, 5),
                GetPedPropIndex(id, 6),
                GetPedPropIndex(id, 7),
                GetPedPropIndex(id, 8)
            );
            var _proptxt = new PropDrawables(
                GetPedPropTextureIndex(id, 0),
                GetPedPropTextureIndex(id, 1),
                GetPedPropTextureIndex(id, 2),
                GetPedPropTextureIndex(id, 3),
                GetPedPropTextureIndex(id, 4),
                GetPedPropTextureIndex(id, 5),
                GetPedPropTextureIndex(id, 6),
                GetPedPropTextureIndex(id, 7),
                GetPedPropTextureIndex(id, 8)
            );
            _characterSkin.CharacterOutfit = new("", "", comp, text, _prop, _proptxt);
            await Common.MoveToMainThread();
            UpdateDress(id, _characterSkin.CharacterOutfit);
        }

        public static void UpdateDress(int Handle, CharacterOutfit dress)
        {
            SetPedComponentVariation(
                Handle,
                (int)ePedComponents.Face,
                dress.ComponentDrawables.Face,
                dress.ComponentTextures.Face,
                2
            );
            SetPedComponentVariation(
                Handle,
                (int)ePedComponents.Mask,
                dress.ComponentDrawables.Mask,
                dress.ComponentTextures.Mask,
                2
            );
            SetPedComponentVariation(
                Handle,
                (int)ePedComponents.Torso,
                dress.ComponentDrawables.Torso,
                dress.ComponentTextures.Torso,
                2
            );
            SetPedComponentVariation(
                Handle,
                (int)ePedComponents.Leg,
                dress.ComponentDrawables.Leg,
                dress.ComponentTextures.Leg,
                2
            );
            SetPedComponentVariation(
                Handle,
                (int)ePedComponents.BagOrParachute,
                dress.ComponentDrawables.BagOrParachute,
                dress.ComponentTextures.BagOrParachute,
                2
            );
            SetPedComponentVariation(
                Handle,
                (int)ePedComponents.Shoes,
                dress.ComponentDrawables.Shoes,
                dress.ComponentTextures.Shoes,
                2
            );
            SetPedComponentVariation(
                Handle,
                (int)ePedComponents.Accessory,
                dress.ComponentDrawables.Accessory,
                dress.ComponentTextures.Accessory,
                2
            );
            SetPedComponentVariation(
                Handle,
                (int)ePedComponents.Undershirt,
                dress.ComponentDrawables.Undershirt,
                dress.ComponentTextures.Undershirt,
                2
            );
            SetPedComponentVariation(
                Handle,
                (int)ePedComponents.Kevlar,
                dress.ComponentDrawables.Kevlar,
                dress.ComponentTextures.Kevlar,
                2
            );
            SetPedComponentVariation(
                Handle,
                (int)ePedComponents.Badge,
                dress.ComponentDrawables.Badge,
                dress.ComponentTextures.Badge,
                2
            );
            SetPedComponentVariation(
                Handle,
                (int)ePedComponents.Torso_2,
                dress.ComponentDrawables.Torso_2,
                dress.ComponentTextures.Torso_2,
                2
            );
            if (dress.PropDrawables.HatOrMask <= 0)
            {
                SetPedPropIndex(Handle, (int)ePedProps.HatOrMask, -1, -1, false);
                ClearPedProp(Handle, (int)ePedProps.HatOrMask);
            }
            else
                SetPedPropIndex(
                    Handle,
                    (int)ePedProps.HatOrMask,
                    dress.PropDrawables.HatOrMask,
                    dress.PropTextures.HatOrMask,
                    false
                );
            if (dress.PropDrawables.Ears <= 0)
            {
                SetPedPropIndex(Handle, (int)ePedProps.Ears, -1, -1, false);
                ClearPedProp(Handle, (int)ePedProps.Ears);
            }
            else
                SetPedPropIndex(
                    Handle,
                    (int)ePedProps.Ears,
                    dress.PropDrawables.Ears,
                    dress.PropTextures.Ears,
                    false
                );
            if (dress.PropDrawables.Glasses <= 0)
            {
                SetPedPropIndex(Handle, (int)ePedProps.Glasses, -1, -1, false);
                ClearPedProp(Handle, (int)ePedProps.Glasses);
            }
            else
                SetPedPropIndex(
                    Handle,
                    (int)ePedProps.Glasses,
                    dress.PropDrawables.Glasses,
                    dress.PropTextures.Glasses,
                    true
                );
            if (dress.PropDrawables.Unk_3 <= 0)
            {
                SetPedPropIndex(Handle, (int)ePedProps.Unk_3, -1, -1, false);
                ClearPedProp(Handle, (int)ePedProps.Unk_3);
            }
            else
                SetPedPropIndex(
                    Handle,
                    (int)ePedProps.Unk_3,
                    dress.PropDrawables.Unk_3,
                    dress.PropTextures.Unk_3,
                    true
                );
            if (dress.PropDrawables.Unk_4 <= 0)
            {
                SetPedPropIndex(Handle, (int)ePedProps.Unk_4, -1, -1, false);
                ClearPedProp(Handle, (int)ePedProps.Unk_4);
            }
            else
                SetPedPropIndex(
                    Handle,
                    (int)ePedProps.Unk_4,
                    dress.PropDrawables.Unk_4,
                    dress.PropTextures.Unk_4,
                    true
                );
            if (dress.PropDrawables.Unk_5 <= 0)
            {
                SetPedPropIndex(Handle, (int)ePedProps.Unk_5, -1, -1, false);
                ClearPedProp(Handle, (int)ePedProps.Unk_5);
            }
            else
                SetPedPropIndex(
                    Handle,
                    (int)ePedProps.Unk_5,
                    dress.PropDrawables.Unk_5,
                    dress.PropTextures.Unk_5,
                    true
                );
            if (dress.PropDrawables.Watches <= 0)
            {
                SetPedPropIndex(Handle, (int)ePedProps.Watches, -1, -1, false);
                ClearPedProp(Handle, (int)ePedProps.Watches);
            }
            else
                SetPedPropIndex(
                    Handle,
                    (int)ePedProps.Watches,
                    dress.PropDrawables.Watches,
                    dress.PropTextures.Watches,
                    true
                );
            if (dress.PropDrawables.Bracelets <= 0)
            {
                SetPedPropIndex(Handle, (int)ePedProps.Bracelets, -1, -1, false);
                ClearPedProp(Handle, (int)ePedProps.Bracelets);
            }
            else
                SetPedPropIndex(
                    Handle,
                    (int)ePedProps.Bracelets,
                    dress.PropDrawables.Bracelets,
                    dress.PropTextures.Bracelets,
                    true
                );
            if (dress.PropDrawables.Unk_8 <= 0)
            {
                SetPedPropIndex(Handle, (int)ePedProps.Unk_8, -1, -1, false);
                ClearPedProp(Handle, (int)ePedProps.Unk_8);
            }
            else
                SetPedPropIndex(
                    Handle,
                    (int)ePedProps.Unk_8,
                    dress.PropDrawables.Unk_8,
                    dress.PropTextures.Unk_8,
                    true
                );
        }

        #endregion

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

        #region character outfits
        static int[][] GetCharacterOutfitSettings(bool isMale, int iParam1)
        {
            var components = new int[12];
            var textures = new int[12];

            for (int i = 0; i < 12; i++)
            {
                components[i] = -1;
                textures[i] = -1;
            }

            switch (iParam1)
            {
                case 56:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 0;
                        textures[4] = 6;
                        components[6] = 0;
                        textures[6] = 10;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 1;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 9;
                        textures[3] = 0;
                        components[4] = 4;
                        textures[4] = 9;
                        components[6] = 13;
                        textures[6] = 12;
                        components[7] = 1;
                        textures[7] = 2;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 9;
                        textures[11] = 9;
                    }
                    break;

                case 57:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 1;
                        textures[4] = 15;
                        components[6] = 1;
                        textures[6] = 9;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 14;
                        textures[11] = 11;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 2;
                        textures[3] = 0;
                        components[4] = 2;
                        textures[4] = 2;
                        components[6] = 2;
                        textures[6] = 14;
                        components[7] = 5;
                        textures[7] = 4;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 2;
                        textures[11] = 6;
                    }
                    break;

                case 58:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 1;
                        textures[3] = 0;
                        components[4] = 4;
                        textures[4] = 2;
                        components[6] = 0;
                        textures[6] = 10;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 1;
                        textures[8] = 5;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 7;
                        textures[11] = 15;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 3;
                        textures[3] = 0;
                        components[4] = 3;
                        textures[4] = 11;
                        components[6] = 16;
                        textures[6] = 1;
                        components[7] = 1;
                        textures[7] = 3;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 3;
                        textures[11] = 10;
                    }
                    break;

                case 59:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 1;
                        textures[4] = 0;
                        components[6] = 1;
                        textures[6] = 1;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 0;
                        textures[11] = 2;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 8;
                        textures[4] = 8;
                        components[6] = 15;
                        textures[6] = 1;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 4;
                        textures[8] = 13;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 1;
                        textures[11] = 5;
                    }
                    break;

                case 60:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 12;
                        textures[3] = 0;
                        components[4] = 1;
                        textures[4] = 14;
                        components[6] = 1;
                        textures[6] = 4;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 41;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 2;
                        textures[3] = 0;
                        components[4] = 3;
                        textures[4] = 7;
                        components[6] = 16;
                        textures[6] = 11;
                        components[7] = 1;
                        textures[7] = 0;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 2;
                        textures[11] = 15;
                    }
                    break;

                case 61:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 8;
                        textures[3] = 0;
                        components[4] = 4;
                        textures[4] = 4;
                        components[6] = 4;
                        textures[6] = 1;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 38;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 3;
                        textures[3] = 0;
                        components[4] = 2;
                        textures[4] = 0;
                        components[6] = 16;
                        textures[6] = 6;
                        components[7] = 2;
                        textures[7] = 1;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 3;
                        textures[11] = 11;
                    }
                    break;

                case 62:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 8;
                        textures[4] = 3;
                        components[6] = 1;
                        textures[6] = 8;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 14;
                        textures[11] = 3;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 16;
                        textures[4] = 4;
                        components[6] = 2;
                        textures[6] = 5;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 0;
                        textures[11] = 11;
                    }
                    break;

                case 63:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 0;
                        textures[4] = 5;
                        components[6] = 1;
                        textures[6] = 0;
                        components[7] = 17;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 33;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 2;
                        textures[3] = 0;
                        components[4] = 0;
                        textures[4] = 8;
                        components[6] = 6;
                        textures[6] = 2;
                        components[7] = 6;
                        textures[7] = 2;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 2;
                        textures[11] = 7;
                    }
                    break;

                case 0:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 15;
                        textures[4] = 9;
                        components[6] = 12;
                        textures[6] = 12;
                        components[7] = 17;
                        textures[7] = 1;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 0;
                        textures[11] = 2;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 3;
                        textures[3] = 0;
                        components[4] = 11;
                        textures[4] = 10;
                        components[6] = 7;
                        textures[6] = 6;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 43;
                        textures[11] = 4;
                    }
                    break;

                case 1:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 1;
                        textures[3] = 0;
                        components[4] = 7;
                        textures[4] = 15;
                        components[6] = 1;
                        textures[6] = 1;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 1;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 7;
                        textures[11] = 1;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 3;
                        textures[4] = 8;
                        components[6] = 4;
                        textures[6] = 2;
                        components[7] = 1;
                        textures[7] = 0;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 32;
                        textures[11] = 0;
                    }
                    break;

                case 2:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 5;
                        textures[4] = 1;
                        components[6] = 6;
                        textures[6] = 0;
                        components[7] = 17;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 1;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 3;
                        textures[3] = 0;
                        components[4] = 3;
                        textures[4] = 15;
                        components[6] = 3;
                        textures[6] = 15;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 3;
                        textures[11] = 1;
                    }
                    break;

                case 3:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 7;
                        textures[4] = 4;
                        components[6] = 12;
                        textures[6] = 4;
                        components[7] = 17;
                        textures[7] = 1;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 5;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 27;
                        textures[4] = 0;
                        components[6] = 11;
                        textures[6] = 1;
                        components[7] = 2;
                        textures[7] = 4;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 5;
                        textures[11] = 9;
                    }
                    break;

                case 4:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 6;
                        textures[3] = 0;
                        components[4] = 5;
                        textures[4] = 12;
                        components[6] = 12;
                        textures[6] = 12;
                        components[7] = 17;
                        textures[7] = 2;
                        components[8] = 5;
                        textures[8] = 2;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 3;
                        textures[11] = 12;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 3;
                        textures[3] = 0;
                        components[4] = 11;
                        textures[4] = 0;
                        components[6] = 3;
                        textures[6] = 1;
                        components[7] = 1;
                        textures[7] = 0;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 3;
                        textures[11] = 12;
                    }
                    break;

                case 5:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 1;
                        textures[3] = 0;
                        components[4] = 15;
                        textures[4] = 12;
                        components[6] = 6;
                        textures[6] = 0;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 0;
                        textures[8] = 2;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 7;
                        textures[11] = 1;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 3;
                        textures[4] = 1;
                        components[6] = 4;
                        textures[6] = 1;
                        components[7] = 2;
                        textures[7] = 1;
                        components[8] = 2;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 5;
                        textures[11] = 0;
                    }
                    break;

                case 6:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 1;
                        textures[4] = 15;
                        components[6] = 7;
                        textures[6] = 0;
                        components[7] = 17;
                        textures[7] = 1;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 0;
                        textures[11] = 2;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 11;
                        textures[4] = 14;
                        components[6] = 3;
                        textures[6] = 8;
                        components[7] = 2;
                        textures[7] = 2;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 32;
                        textures[11] = 1;
                    }
                    break;

                case 7:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 6;
                        textures[3] = 0;
                        components[4] = 12;
                        textures[4] = 7;
                        components[6] = 7;
                        textures[6] = 0;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 5;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 3;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 30;
                        textures[4] = 1;
                        components[6] = 24;
                        textures[6] = 0;
                        components[7] = 5;
                        textures[7] = 4;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 4;
                        textures[11] = 14;
                    }
                    break;

                case 8:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 24;
                        textures[4] = 2;
                        components[6] = 10;
                        textures[6] = 0;
                        components[7] = 12;
                        textures[7] = 2;
                        components[8] = 31;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 31;
                        textures[11] = 2;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 8;
                        textures[4] = 4;
                        components[6] = 19;
                        textures[6] = 3;
                        components[7] = 10;
                        textures[7] = 3;
                        components[8] = 13;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 35;
                        textures[11] = 8;
                    }
                    break;

                case 9:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 4;
                        textures[4] = 2;
                        components[6] = 20;
                        textures[6] = 5;
                        components[7] = 25;
                        textures[7] = 2;
                        components[8] = 4;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 24;
                        textures[11] = 4;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 9;
                        textures[4] = 15;
                        components[6] = 6;
                        textures[6] = 0;
                        components[7] = 4;
                        textures[7] = 3;
                        components[8] = 2;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 13;
                        textures[11] = 6;
                    }
                    break;

                case 10:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 4;
                        textures[4] = 0;
                        components[6] = 20;
                        textures[6] = 7;
                        components[7] = 25;
                        textures[7] = 2;
                        components[8] = 4;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 24;
                        textures[11] = 3;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 23;
                        textures[4] = 0;
                        components[6] = 20;
                        textures[6] = 2;
                        components[7] = 7;
                        textures[7] = 1;
                        components[8] = 13;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 24;
                        textures[11] = 9;
                    }
                    break;

                case 11:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 12;
                        textures[3] = 0;
                        components[4] = 24;
                        textures[4] = 0;
                        components[6] = 10;
                        textures[6] = 0;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 32;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 29;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 8;
                        textures[4] = 0;
                        components[6] = 19;
                        textures[6] = 4;
                        components[7] = 6;
                        textures[7] = 1;
                        components[8] = 23;
                        textures[8] = 1;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 24;
                        textures[11] = 6;
                    }
                    break;

                case 12:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 6;
                        textures[3] = 0;
                        components[4] = 26;
                        textures[4] = 0;
                        components[6] = 20;
                        textures[6] = 3;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 5;
                        textures[8] = 2;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 4;
                        textures[11] = 14;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 9;
                        textures[4] = 6;
                        components[6] = 14;
                        textures[6] = 11;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 13;
                        textures[8] = 3;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 8;
                        textures[11] = 0;
                    }
                    break;

                case 13:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 12;
                        textures[3] = 0;
                        components[4] = 24;
                        textures[4] = 5;
                        components[6] = 10;
                        textures[6] = 0;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 32;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 29;
                        textures[11] = 5;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 6;
                        textures[3] = 0;
                        components[4] = 24;
                        textures[4] = 8;
                        components[6] = 0;
                        textures[6] = 0;
                        components[7] = 1;
                        textures[7] = 0;
                        components[8] = 13;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 25;
                        textures[11] = 9;
                    }
                    break;

                case 14:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 6;
                        textures[3] = 0;
                        components[4] = 4;
                        textures[4] = 0;
                        components[6] = 10;
                        textures[6] = 0;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 5;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 4;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 8;
                        textures[4] = 12;
                        components[6] = 8;
                        textures[6] = 0;
                        components[7] = 11;
                        textures[7] = 0;
                        components[8] = 5;
                        textures[8] = 7;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 8;
                        textures[11] = 1;
                    }
                    break;

                case 15:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 12;
                        textures[3] = 0;
                        components[4] = 24;
                        textures[4] = 0;
                        components[6] = 10;
                        textures[6] = 0;
                        components[7] = 27;
                        textures[7] = 2;
                        components[8] = 35;
                        textures[8] = 1;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 30;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 9;
                        textures[4] = 8;
                        components[6] = 6;
                        textures[6] = 0;
                        components[7] = 6;
                        textures[7] = 0;
                        components[8] = 0;
                        textures[8] = 10;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 6;
                        textures[11] = 0;
                    }
                    break;

                case 16:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 11;
                        textures[3] = 0;
                        components[4] = 26;
                        textures[4] = 3;
                        components[6] = 22;
                        textures[6] = 7;
                        components[7] = 11;
                        textures[7] = 2;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 42;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 27;
                        textures[4] = 6;
                        components[6] = 22;
                        textures[6] = 5;
                        components[7] = 14;
                        textures[7] = 0;
                        components[8] = 28;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 31;
                        textures[11] = 6;
                    }
                    break;

                case 17:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 1;
                        textures[3] = 0;
                        components[4] = 26;
                        textures[4] = 11;
                        components[6] = 22;
                        textures[6] = 8;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 38;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 35;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 8;
                        textures[4] = 11;
                        components[6] = 7;
                        textures[6] = 0;
                        components[7] = 9;
                        textures[7] = 0;
                        components[8] = 2;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 4;
                        textures[11] = 14;
                    }
                    break;

                case 18:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 27;
                        textures[4] = 4;
                        components[6] = 1;
                        textures[6] = 9;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 22;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 27;
                        textures[4] = 10;
                        components[6] = 22;
                        textures[6] = 6;
                        components[7] = 14;
                        textures[7] = 2;
                        components[8] = 26;
                        textures[8] = 1;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 31;
                        textures[11] = 3;
                    }
                    break;

                case 19:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 1;
                        textures[3] = 0;
                        components[4] = 26;
                        textures[4] = 7;
                        components[6] = 23;
                        textures[6] = 6;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 43;
                        textures[8] = 3;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 35;
                        textures[11] = 3;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 0;
                        textures[4] = 1;
                        components[6] = 6;
                        textures[6] = 0;
                        components[7] = 2;
                        textures[7] = 0;
                        components[8] = 2;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 13;
                        textures[11] = 7;
                    }
                    break;

                case 20:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 8;
                        textures[3] = 0;
                        components[4] = 26;
                        textures[4] = 0;
                        components[6] = 22;
                        textures[6] = 3;
                        components[7] = 30;
                        textures[7] = 3;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 38;
                        textures[11] = 4;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 6;
                        textures[3] = 0;
                        components[4] = 27;
                        textures[4] = 2;
                        components[6] = 3;
                        textures[6] = 10;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 28;
                        textures[8] = 3;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 34;
                        textures[11] = 0;
                    }
                    break;

                case 21:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 1;
                        textures[3] = 0;
                        components[4] = 26;
                        textures[4] = 6;
                        components[6] = 22;
                        textures[6] = 0;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 38;
                        textures[8] = 1;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 35;
                        textures[11] = 2;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 27;
                        textures[4] = 11;
                        components[6] = 22;
                        textures[6] = 4;
                        components[7] = 14;
                        textures[7] = 3;
                        components[8] = 28;
                        textures[8] = 6;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 35;
                        textures[11] = 3;
                    }
                    break;

                case 22:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 26;
                        textures[4] = 1;
                        components[6] = 7;
                        textures[6] = 0;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 44;
                        textures[11] = 3;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 27;
                        textures[4] = 7;
                        components[6] = 19;
                        textures[6] = 1;
                        components[7] = 4;
                        textures[7] = 2;
                        components[8] = 28;
                        textures[8] = 1;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 31;
                        textures[11] = 6;
                    }
                    break;

                case 23:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 14;
                        textures[3] = 0;
                        components[4] = 26;
                        textures[4] = 8;
                        components[6] = 22;
                        textures[6] = 11;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 23;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 35;
                        textures[11] = 6;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 0;
                        textures[4] = 7;
                        components[6] = 20;
                        textures[6] = 5;
                        components[7] = 11;
                        textures[7] = 0;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 33;
                        textures[11] = 4;
                    }
                    break;

                case 24:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 15;
                        textures[4] = 0;
                        components[6] = 16;
                        textures[6] = 5;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 17;
                        textures[11] = 4;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 15;
                        textures[3] = 0;
                        components[4] = 25;
                        textures[4] = 2;
                        components[6] = 16;
                        textures[6] = 9;
                        components[7] = 1;
                        textures[7] = 2;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 18;
                        textures[11] = 10;
                    }
                    break;

                case 25:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 18;
                        textures[4] = 2;
                        components[6] = 1;
                        textures[6] = 3;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 1;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 16;
                        textures[4] = 0;
                        components[6] = 15;
                        textures[6] = 1;
                        components[7] = 14;
                        textures[7] = 1;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 17;
                        textures[11] = 0;
                    }
                    break;

                case 26:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 16;
                        textures[4] = 6;
                        components[6] = 16;
                        textures[6] = 6;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 17;
                        textures[11] = 5;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 11;
                        textures[3] = 0;
                        components[4] = 17;
                        textures[4] = 7;
                        components[6] = 16;
                        textures[6] = 9;
                        components[7] = 11;
                        textures[7] = 2;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 36;
                        textures[11] = 3;
                    }
                    break;

                case 27:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 15;
                        textures[3] = 0;
                        components[4] = 18;
                        textures[4] = 11;
                        components[6] = 5;
                        textures[6] = 3;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 15;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 15;
                        textures[3] = 0;
                        components[4] = 12;
                        textures[4] = 14;
                        components[6] = 3;
                        textures[6] = 13;
                        components[7] = 11;
                        textures[7] = 1;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 18;
                        textures[11] = 9;
                    }
                    break;

                case 28:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 16;
                        textures[4] = 2;
                        components[6] = 16;
                        textures[6] = 8;
                        components[7] = 17;
                        textures[7] = 1;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 17;
                        textures[11] = 3;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 9;
                        textures[3] = 0;
                        components[4] = 14;
                        textures[4] = 8;
                        components[6] = 13;
                        textures[6] = 5;
                        components[7] = 4;
                        textures[7] = 3;
                        components[8] = 2;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 9;
                        textures[11] = 3;
                    }
                    break;

                case 29:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 18;
                        textures[4] = 3;
                        components[6] = 16;
                        textures[6] = 4;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 5;
                        textures[11] = 7;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 15;
                        textures[3] = 0;
                        components[4] = 16;
                        textures[4] = 8;
                        components[6] = 16;
                        textures[6] = 1;
                        components[7] = 9;
                        textures[7] = 0;
                        components[8] = 2;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 18;
                        textures[11] = 6;
                    }
                    break;

                case 30:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 15;
                        textures[3] = 0;
                        components[4] = 16;
                        textures[4] = 1;
                        components[6] = 1;
                        textures[6] = 7;
                        components[7] = 16;
                        textures[7] = 1;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 15;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 11;
                        textures[3] = 0;
                        components[4] = 17;
                        textures[4] = 4;
                        components[6] = 16;
                        textures[6] = 7;
                        components[7] = 3;
                        textures[7] = 1;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 11;
                        textures[11] = 10;
                    }
                    break;

                case 31:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 6;
                        textures[4] = 10;
                        components[6] = 16;
                        textures[6] = 6;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 44;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 16;
                        textures[4] = 10;
                        components[6] = 16;
                        textures[6] = 3;
                        components[7] = 10;
                        textures[7] = 0;
                        components[8] = 16;
                        textures[8] = 4;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 31;
                        textures[11] = 5;
                    }
                    break;

                case 32:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 12;
                        textures[3] = 0;
                        components[4] = 22;
                        textures[4] = 5;
                        components[6] = 21;
                        textures[6] = 10;
                        components[7] = 21;
                        textures[7] = 12;
                        components[8] = 28;
                        textures[8] = 13;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 24;
                        textures[11] = 5;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 23;
                        textures[4] = 4;
                        components[6] = 0;
                        textures[6] = 0;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 13;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 24;
                        textures[11] = 2;
                    }
                    break;

                case 33:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 12;
                        textures[3] = 0;
                        components[4] = 25;
                        textures[4] = 1;
                        components[6] = 10;
                        textures[6] = 0;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 32;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 31;
                        textures[11] = 1;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 24;
                        textures[4] = 4;
                        components[6] = 19;
                        textures[6] = 0;
                        components[7] = 13;
                        textures[7] = 0;
                        components[8] = 2;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 27;
                        textures[11] = 0;
                    }
                    break;

                case 34:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 11;
                        textures[3] = 0;
                        components[4] = 22;
                        textures[4] = 7;
                        components[6] = 20;
                        textures[6] = 2;
                        components[7] = 25;
                        textures[7] = 10;
                        components[8] = 6;
                        textures[8] = 11;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 25;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 23;
                        textures[4] = 0;
                        components[6] = 19;
                        textures[6] = 3;
                        components[7] = 1;
                        textures[7] = 1;
                        components[8] = 25;
                        textures[8] = 5;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 24;
                        textures[11] = 0;
                    }
                    break;

                case 35:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 10;
                        textures[4] = 0;
                        components[6] = 10;
                        textures[6] = 0;
                        components[7] = 25;
                        textures[7] = 2;
                        components[8] = 4;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 28;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 6;
                        textures[3] = 0;
                        components[4] = 6;
                        textures[4] = 0;
                        components[6] = 13;
                        textures[6] = 0;
                        components[7] = 6;
                        textures[7] = 0;
                        components[8] = 23;
                        textures[8] = 3;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 7;
                        textures[11] = 0;
                    }
                    break;

                case 36:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 11;
                        textures[3] = 0;
                        components[4] = 10;
                        textures[4] = 0;
                        components[6] = 11;
                        textures[6] = 12;
                        components[7] = 21;
                        textures[7] = 11;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 26;
                        textures[11] = 2;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 7;
                        textures[4] = 2;
                        components[6] = 19;
                        textures[6] = 9;
                        components[7] = 1;
                        textures[7] = 1;
                        components[8] = 24;
                        textures[8] = 3;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 28;
                        textures[11] = 10;
                    }
                    break;

                case 37:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 23;
                        textures[4] = 8;
                        components[6] = 20;
                        textures[6] = 8;
                        components[7] = 12;
                        textures[7] = 2;
                        components[8] = 10;
                        textures[8] = 2;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 23;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 6;
                        textures[4] = 2;
                        components[6] = 20;
                        textures[6] = 7;
                        components[7] = 6;
                        textures[7] = 4;
                        components[8] = 25;
                        textures[8] = 9;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 6;
                        textures[11] = 2;
                    }
                    break;

                case 38:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 10;
                        textures[4] = 2;
                        components[6] = 10;
                        textures[6] = 0;
                        components[7] = 25;
                        textures[7] = 14;
                        components[8] = 4;
                        textures[8] = 2;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 28;
                        textures[11] = 2;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 6;
                        textures[3] = 0;
                        components[4] = 7;
                        textures[4] = 0;
                        components[6] = 20;
                        textures[6] = 0;
                        components[7] = 6;
                        textures[7] = 0;
                        components[8] = 13;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 25;
                        textures[11] = 2;
                    }
                    break;

                case 39:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 11;
                        textures[3] = 0;
                        components[4] = 23;
                        textures[4] = 8;
                        components[6] = 21;
                        textures[6] = 6;
                        components[7] = 25;
                        textures[7] = 13;
                        components[8] = 6;
                        textures[8] = 12;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 25;
                        textures[11] = 9;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 23;
                        textures[4] = 4;
                        components[6] = 19;
                        textures[6] = 8;
                        components[7] = 13;
                        textures[7] = 2;
                        components[8] = 2;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 27;
                        textures[11] = 4;
                    }
                    break;

                case 40:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 1;
                        textures[3] = 0;
                        components[4] = 3;
                        textures[4] = 15;
                        components[6] = 2;
                        textures[6] = 6;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 0;
                        textures[8] = 3;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 3;
                        textures[11] = 15;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 14;
                        textures[3] = 0;
                        components[4] = 2;
                        textures[4] = 0;
                        components[6] = 10;
                        textures[6] = 2;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 14;
                        textures[11] = 7;
                    }
                    break;

                case 41:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 5;
                        textures[4] = 6;
                        components[6] = 2;
                        textures[6] = 6;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 1;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 11;
                        textures[3] = 0;
                        components[4] = 2;
                        textures[4] = 2;
                        components[6] = 10;
                        textures[6] = 2;
                        components[7] = 3;
                        textures[7] = 3;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 11;
                        textures[11] = 0;
                    }
                    break;

                case 42:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 15;
                        textures[4] = 8;
                        components[6] = 2;
                        textures[6] = 13;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 9;
                        textures[11] = 4;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 14;
                        textures[3] = 0;
                        components[4] = 12;
                        textures[4] = 8;
                        components[6] = 10;
                        textures[6] = 3;
                        components[7] = 3;
                        textures[7] = 4;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 14;
                        textures[11] = 10;
                    }
                    break;

                case 43:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 8;
                        textures[3] = 0;
                        components[4] = 14;
                        textures[4] = 1;
                        components[6] = 2;
                        textures[6] = 13;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 38;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 7;
                        textures[3] = 0;
                        components[4] = 14;
                        textures[4] = 9;
                        components[6] = 11;
                        textures[6] = 0;
                        components[7] = 2;
                        textures[7] = 4;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 10;
                        textures[11] = 0;
                    }
                    break;

                case 44:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 1;
                        textures[3] = 0;
                        components[4] = 7;
                        textures[4] = 4;
                        components[6] = 7;
                        textures[6] = 1;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 0;
                        textures[8] = 7;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 7;
                        textures[11] = 5;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 14;
                        textures[3] = 0;
                        components[4] = 14;
                        textures[4] = 8;
                        components[6] = 3;
                        textures[6] = 1;
                        components[7] = 3;
                        textures[7] = 5;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 14;
                        textures[11] = 0;
                    }
                    break;

                case 45:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 1;
                        textures[3] = 0;
                        components[4] = 3;
                        textures[4] = 4;
                        components[6] = 7;
                        textures[6] = 15;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 41;
                        textures[8] = 3;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 7;
                        textures[11] = 4;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 7;
                        textures[3] = 0;
                        components[4] = 10;
                        textures[4] = 2;
                        components[6] = 1;
                        textures[6] = 13;
                        components[7] = 1;
                        textures[7] = 1;
                        components[8] = 5;
                        textures[8] = 9;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 10;
                        textures[11] = 10;
                    }
                    break;

                case 46:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 18;
                        textures[4] = 1;
                        components[6] = 9;
                        textures[6] = 7;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 39;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 14;
                        textures[3] = 0;
                        components[4] = 12;
                        textures[4] = 0;
                        components[6] = 4;
                        textures[6] = 1;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 14;
                        textures[11] = 4;
                    }
                    break;

                case 47:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 6;
                        textures[4] = 0;
                        components[6] = 9;
                        textures[6] = 0;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 9;
                        textures[11] = 10;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 7;
                        textures[3] = 0;
                        components[4] = 2;
                        textures[4] = 2;
                        components[6] = 11;
                        textures[6] = 1;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 1;
                        textures[8] = 8;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 10;
                        textures[11] = 7;
                    }
                    break;

                case 48:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 11;
                        textures[3] = 0;
                        components[4] = 26;
                        textures[4] = 9;
                        components[6] = 23;
                        textures[6] = 3;
                        components[7] = 11;
                        textures[7] = 2;
                        components[8] = 6;
                        textures[8] = 12;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 40;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 24;
                        textures[4] = 0;
                        components[6] = 20;
                        textures[6] = 9;
                        components[7] = 6;
                        textures[7] = 1;
                        components[8] = 23;
                        textures[8] = 3;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 24;
                        textures[11] = 10;
                    }
                    break;

                case 49:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 28;
                        textures[4] = 2;
                        components[6] = 23;
                        textures[6] = 14;
                        components[7] = 30;
                        textures[7] = 4;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 33;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 27;
                        textures[4] = 5;
                        components[6] = 8;
                        textures[6] = 3;
                        components[7] = 2;
                        textures[7] = 5;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 13;
                        textures[11] = 8;
                    }
                    break;

                case 50:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 22;
                        textures[4] = 11;
                        components[6] = 20;
                        textures[6] = 6;
                        components[7] = 25;
                        textures[7] = 4;
                        components[8] = 4;
                        textures[8] = 1;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 24;
                        textures[11] = 0;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 27;
                        textures[4] = 13;
                        components[6] = 22;
                        textures[6] = 3;
                        components[7] = 14;
                        textures[7] = 2;
                        components[8] = 28;
                        textures[8] = 8;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 35;
                        textures[11] = 1;
                    }
                    break;

                case 51:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 0;
                        textures[3] = 0;
                        components[4] = 27;
                        textures[4] = 6;
                        components[6] = 12;
                        textures[6] = 15;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 15;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 22;
                        textures[11] = 1;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 27;
                        textures[4] = 13;
                        components[6] = 22;
                        textures[6] = 7;
                        components[7] = 11;
                        textures[7] = 0;
                        components[8] = 2;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 33;
                        textures[11] = 2;
                    }
                    break;

                case 52:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 22;
                        textures[4] = 12;
                        components[6] = 10;
                        textures[6] = 0;
                        components[7] = 25;
                        textures[7] = 0;
                        components[8] = 26;
                        textures[8] = 12;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 23;
                        textures[11] = 3;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 7;
                        textures[3] = 0;
                        components[4] = 0;
                        textures[4] = 14;
                        components[6] = 19;
                        textures[6] = 4;
                        components[7] = 1;
                        textures[7] = 0;
                        components[8] = 1;
                        textures[8] = 8;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 10;
                        textures[11] = 15;
                    }
                    break;

                case 53:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 28;
                        textures[4] = 12;
                        components[6] = 20;
                        textures[6] = 2;
                        components[7] = 12;
                        textures[7] = 2;
                        components[8] = 10;
                        textures[8] = 14;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 35;
                        textures[11] = 4;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 5;
                        textures[3] = 0;
                        components[4] = 24;
                        textures[4] = 5;
                        components[6] = 20;
                        textures[6] = 7;
                        components[7] = 11;
                        textures[7] = 3;
                        components[8] = 0;
                        textures[8] = 15;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 24;
                        textures[11] = 8;
                    }
                    break;

                case 54:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 12;
                        textures[3] = 0;
                        components[4] = 27;
                        textures[4] = 1;
                        components[6] = 23;
                        textures[6] = 10;
                        components[7] = 22;
                        textures[7] = 6;
                        components[8] = 26;
                        textures[8] = 6;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 35;
                        textures[11] = 2;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 12;
                        textures[3] = 0;
                        components[4] = 27;
                        textures[4] = 9;
                        components[6] = 20;
                        textures[6] = 8;
                        components[7] = 10;
                        textures[7] = 1;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 26;
                        textures[11] = 11;
                    }
                    break;

                case 55:
                    if (isMale)
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 14;
                        textures[3] = 0;
                        components[4] = 26;
                        textures[4] = 10;
                        components[6] = 23;
                        textures[6] = 8;
                        components[7] = 0;
                        textures[7] = 0;
                        components[8] = 23;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 24;
                        textures[11] = 2;
                    }
                    else
                    {
                        components[1] = 0;
                        textures[1] = 0;
                        components[3] = 4;
                        textures[3] = 0;
                        components[4] = 24;
                        textures[4] = 9;
                        components[6] = 8;
                        textures[6] = 8;
                        components[7] = 11;
                        textures[7] = 0;
                        components[8] = 3;
                        textures[8] = 0;
                        components[9] = 0;
                        textures[9] = 0;
                        components[10] = 0;
                        textures[10] = 0;
                        components[11] = 33;
                        textures[11] = 8;
                    }
                    break;
            }
            return new int[][]
            {
                new int[12]
                {
                    components[0],
                    components[1],
                    components[2],
                    components[3],
                    components[4],
                    components[5],
                    components[6],
                    components[7],
                    components[8],
                    components[9],
                    components[10],
                    components[11]
                },
                new int[12]
                {
                    textures[0],
                    textures[1],
                    textures[2],
                    textures[3],
                    textures[4],
                    textures[5],
                    textures[6],
                    textures[7],
                    textures[8],
                    textures[9],
                    textures[10],
                    textures[11]
                }
            };
        }
        #endregion
    }
}
