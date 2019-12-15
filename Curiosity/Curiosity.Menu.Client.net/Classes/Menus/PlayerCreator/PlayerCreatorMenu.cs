using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MenuAPI;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using CitizenFX.Core;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Global.Shared.net.Entity;

namespace Curiosity.Menus.Client.net.Classes.Menus.PlayerCreator
{
    class PlayerCreatorMenu
    {
        private static PlayerCharacter _playerCharacter;
        // It's fine if this is kept between switches
        static int blendHeadA = 0;
        static int blendHeadB = 0;
        static int blendHeadAmount = 25;
        static int blendSkinA = 0;
        static int blendSkinB = 0;
        static int blendSkinAmount = 25;

        public static Menu menu = new Menu("Player Creator", "Customise your character");
        public static bool MenuSetup = false;
        static float defaultFov = 0.0f;

        public static List<string> GenerateNumberList(string txt, int max, int min = 0)
        {
            List<string> lst = new List<string>();
            for (int i = min; i < max + 1; i++)
                lst.Add($"{txt} #{i.ToString()}");
            return lst;
        }

        public static PlayerCharacter PlayerCharacter()
        {
            return _playerCharacter;
        }

        public static async void Init()
        {
            while (!Client.hasPlayerSpawned)
                await BaseScript.Delay(0);

            _playerCharacter = Client.User.Skin;

            blendHeadA = _playerCharacter.FatherAppearance;
            blendHeadB = _playerCharacter.MotherAppearance;
            blendHeadAmount = _playerCharacter.FatherMotherAppearanceGene;

            blendSkinA = _playerCharacter.FatherSkin;
            blendSkinB = _playerCharacter.MotherSkin;
            blendSkinAmount = _playerCharacter.FatherMotherSkinGene;

            MenuListItem gender = new MenuListItem($@"Gender", new List<string> { "Male", "Female" }, (_playerCharacter.Model == "mp_m_freemode_01") ? 0 : 1) { ItemData = "GENDER" };
            menu.AddMenuItem(gender);

            MenuListItem fatherAppearance = new MenuListItem($@"Face: Father's appearance", GenerateNumberList("Face", 45), _playerCharacter.FatherAppearance) { ItemData = "FATHER_FACE" };
            MenuListItem motherAppearance = new MenuListItem($@"Face: Mother's appearance", GenerateNumberList("Face", 45), _playerCharacter.MotherAppearance) { ItemData = "MOTHER_FACE" };
            menu.AddMenuItem(fatherAppearance);
            menu.AddMenuItem(motherAppearance);
            MenuSliderItem fatherMotherGene = new MenuSliderItem($@"Face: Blend", 0, 49, _playerCharacter.FatherMotherAppearanceGene) { SliderLeftIcon = MenuItem.Icon.MALE, SliderRightIcon = MenuItem.Icon.FEMALE, ItemData = "FACE_GENEWEIGHT" };
            menu.AddMenuItem(fatherMotherGene);

            MenuListItem fatherSkin = new MenuListItem($@"Skin color: Father's appearance", GenerateNumberList("Color", 45), _playerCharacter.FatherSkin) { ItemData = "FATHER_SKIN" };
            MenuListItem motherSkin = new MenuListItem($@"Skin color: Mother's appearance", GenerateNumberList("Color", 45), _playerCharacter.MotherSkin) { ItemData = "MOTHER_SKIN" };
            menu.AddMenuItem(fatherSkin);
            menu.AddMenuItem(motherSkin);
            MenuSliderItem fatherMotherSkin = new MenuSliderItem($@"Skin color: Blend", 0, 49, _playerCharacter.FatherMotherSkinGene) { SliderLeftIcon = MenuItem.Icon.MALE, SliderRightIcon = MenuItem.Icon.FEMALE, ItemData = "SKIN_GENEWEIGHT" };
            menu.AddMenuItem(fatherMotherSkin);

            int hairColorPrimary = 0;
            int hairColorSecondary = 0;

            List<string> colorList = new List<string>();
            for (var i = 0; i < 64; i++)
            {
                colorList.Add($"Color #{i}");
            }
            MenuListItem eyeColors = new MenuListItem("Eye Color", GenerateNumberList("Color", 31), _playerCharacter.EyeColor) { ItemData = "EYE_COLOR" };
            MenuListItem hairColors = new MenuListItem("Primary Hair Color", colorList, _playerCharacter.HairColor, "Hair color pallete.") { ShowColorPanel = true, ItemData = "HAIR_PRIM" };
            MenuListItem hairSecondayColors = new MenuListItem("Secondary Hair Color", colorList, _playerCharacter.HairSecondaryColor, "Secondary Hair color pallete.") { ShowColorPanel = true, ItemData = "HAIR_SEC" };

            menu.AddMenuItem(eyeColors);
            menu.AddMenuItem(hairColors);
            menu.AddMenuItem(hairSecondayColors);

            menu.OnSliderPositionChange += (Menu menu, MenuSliderItem sliderItem, int oldPosition, int newPosition, int itemIndex) =>
            {
                if ($"{sliderItem.ItemData}" == "FACE_GENEWEIGHT")
                {
                    blendHeadAmount = newPosition;
                    _playerCharacter.FatherMotherAppearanceGene = blendHeadAmount;
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);
                }

                if ($"{sliderItem.ItemData}" == "SKIN_GENEWEIGHT")
                {
                    blendSkinAmount = newPosition;
                    _playerCharacter.FatherMotherSkinGene = blendSkinAmount;
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);
                }
            };

            menu.OnMenuOpen += (_menu) =>
            {
                MenuBase.MenuOpen(true);

                if (defaultFov == 0.0f)
                    defaultFov = World.RenderingCamera.FieldOfView;

                World.RenderingCamera.FieldOfView = 30;
            };

            menu.OnMenuClose += (_menu) =>
            {
                MenuBase.MenuOpen(false);
            };

            menu.OnIndexChange += (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                
            };

            menu.OnItemSelect += (Menu menu, MenuItem menuItem, int itemIndex) =>
            {
                if (menuItem.ItemData == "SAVE")
                {
                    PlayerSave.SaveCharacter();
                }
                if (menuItem.ItemData == "RESET")
                {
                    PlayerReset.ResetCharacter();
                }
            };

            menu.OnListIndexChange += async (Menu _menu, MenuListItem _listItem, int _oldSelectionIndex, int _newSelectionIndex, int _itemIndex) =>
            {
                if ($"{_listItem.ItemData}" == "GENDER")
                {
                    string model = "mp_m_freemode_01";
                    if (_newSelectionIndex == 1)
                    {
                        model = "mp_f_freemode_01";
                    }

                    _playerCharacter.Model = model;

                    await new Model(model).Request(10000);

                    Screen.Fading.FadeOut(500);
                    while (Screen.Fading.IsFadingOut) await BaseScript.Delay(0);

                    await Game.Player.ChangeModel(new Model(model));
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);

                    Screen.Fading.FadeIn(500);
                    while (Screen.Fading.IsFadingIn) await BaseScript.Delay(0);
                }

                if ($"{_listItem.ItemData}" == "FATHER_FACE")
                {
                    blendHeadA = _newSelectionIndex;
                    _playerCharacter.FatherAppearance = blendHeadA;
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);
                }

                if ($"{_listItem.ItemData}" == "MOTHER_FACE")
                {
                    blendHeadB = _newSelectionIndex;
                    _playerCharacter.MotherAppearance = blendHeadB;
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);
                }

                if ($"{_listItem.ItemData}" == "FATHER_SKIN")
                {
                    blendSkinA = _newSelectionIndex;
                    _playerCharacter.FatherSkin = blendSkinA;
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);
                }

                if ($"{_listItem.ItemData}" == "MOTHER_SKIN")
                {
                    blendSkinB = _newSelectionIndex;
                    _playerCharacter.MotherSkin = blendSkinB;
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);
                }

                if ($"{_listItem.ItemData}" == "HAIR_PRIM" || $"{_listItem.ItemData}" == "HAIR_SEC")
                {
                    if ($"{_listItem.ItemData}" == "HAIR_PRIM")
                    {
                        hairColorPrimary = _newSelectionIndex;
                        _playerCharacter.HairColor = hairColorPrimary;
                    }

                    if ($"{_listItem.ItemData}" == "HAIR_SEC")
                    {
                        hairColorSecondary = _newSelectionIndex;
                        _playerCharacter.HairSecondaryColor = hairColorSecondary;
                    }

                    API.SetPedHairColor(Client.PedHandle, hairColorPrimary, hairColorSecondary);
                }

                if ($"{_listItem.ItemData}" == "EYE_COLOR")
                {
                    _playerCharacter.EyeColor = _newSelectionIndex;
                    API.SetPedEyeColor(Client.PedHandle, _newSelectionIndex);
                }
            };

            MenuBase.AddSubMenu(menu, leftIcon: MenuItem.Icon.INV_PERSON);

            MenuSetup = true;
        }

        public static void RemoveMenu()
        {
            MenuBase.RemoveMenu(menu);
        }

        public static void ManualOpenMenu()
        {
            menu.ParentMenu.OpenMenu();
        }

        public static void StoreOverlay(int overlayId, int option)
        {
            if (_playerCharacter.PedHeadOverlay.ContainsKey(overlayId))
            {
                _playerCharacter.PedHeadOverlay[overlayId] = option;
            }
            else
            {
                _playerCharacter.PedHeadOverlay.Add(overlayId, option);
            }
        }

        public static void StoreOverlayColor(int overlayId, int colorType, int option)
        {
            if (_playerCharacter.PedHeadOverlayColor.ContainsKey(overlayId))
            {
                _playerCharacter.PedHeadOverlayColor[overlayId] = new Tuple<int, int>(colorType, option);
            }
            else
            {
                _playerCharacter.PedHeadOverlayColor.Add(overlayId, new Tuple<int, int>(colorType, option));
            }
        }

        public static void StoreComponents(int componentId, int model, int texture)
        {
            if (_playerCharacter.Components.ContainsKey(componentId))
            {
                _playerCharacter.Components[componentId] = new Tuple<int, int>(model, texture);
            }
            else
            {
                _playerCharacter.Components.Add(componentId, new Tuple<int, int>(model, texture));
            }
        }

        public static void StoreProps(int propId, int model, int texture)
        {
            if (_playerCharacter.Props.ContainsKey(propId))
            {
                _playerCharacter.Props[propId] = new Tuple<int, int>(model, texture);
            }
            else
            {
                _playerCharacter.Props.Add(propId, new Tuple<int, int>(model, texture));
            }
        }
    }
}
