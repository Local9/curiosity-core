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

namespace Curiosity.Client.net.Classes.Menus.PlayerCreator
{
    class PlayerCreatorMenu
    {
        // It's fine if this is kept between switches
        static int blendHeadA = 0;
        static int blendHeadB = 0;
        static float blendHeadAmount = 0.5f;
        static int blendSkinA = 0;
        static int blendSkinB = 0;
        static float blendSkinAmount = 0.5f;
        static Dictionary<string, Tuple<int, int>> componentSettings = new Dictionary<string, Tuple<int, int>>();
        static Dictionary<string, Tuple<int, int>> propSettings = new Dictionary<string, Tuple<int, int>>();
        static Dictionary<string, string> componentAndPropRenamings = new Dictionary<string, string>()
        {
            ["Torso"] = "Arms",
            ["Legs"] = "Pants",
            ["Hands"] = "Parachutes, Vests and Bags",
            ["Special1"] = "Neck",
            ["Special2"] = "Overshirt",
            ["Special3"] = "Tactical Vests",
            ["Textures"] = "Logos",
            ["Torso"] = "Arms",
            ["Torso2"] = "Jacket",
            ["EarPieces"] = "Ear Pieces"
        };

        static List<PedProps> PedPropValues = Enum.GetValues(typeof(PedProps)).OfType<PedProps>().ToList();
        static List<string> PedPropNames = Enum.GetNames(typeof(PedProps)).Select(c => c.AddSpacesToCamelCase()).ToList();

        public static Menu menu = new Menu("Player Creator", "Customise your character");
        static float defaultFov = 0.0f;

        static List<string> GenerateNumberList(string txt, int max)
        {
            List<string> lst = new List<string>();
            for (int i = 0; i < max + 1; i++)
                lst.Add($"{txt} #{i.ToString()}");
            return lst;
        }

        public static void Init()
        {
            MenuListItem gender = new MenuListItem($@"Gender", new List<string> { "Male", "Female" }, 0) { ItemData = "GENDER" };
            menu.AddMenuItem(gender);

            MenuListItem fatherAppearance = new MenuListItem($@"Face: Father's appearance", GenerateNumberList("Face", 45), 0) { ItemData = "FATHER_FACE" };
            MenuListItem motherAppearance = new MenuListItem($@"Face: Mother's appearance", GenerateNumberList("Face", 45), 0) { ItemData = "MOTHER_FACE" };
            menu.AddMenuItem(fatherAppearance);
            menu.AddMenuItem(motherAppearance);
            MenuSliderItem fatherMotherGene = new MenuSliderItem($@"Face: Blend", 0, 49, 24) { SliderLeftIcon = MenuItem.Icon.MALE, SliderRightIcon = MenuItem.Icon.FEMALE, ItemData = "FACE_GENEWEIGHT" };
            menu.AddMenuItem(fatherMotherGene);

            MenuListItem fatherSkin = new MenuListItem($@"Skin color: Father's appearance", GenerateNumberList("Color", 45), 0) { ItemData = "FATHER_SKIN" };
            MenuListItem motherSkin = new MenuListItem($@"Skin color: Mother's appearance", GenerateNumberList("Color", 45), 0) { ItemData = "MOTHER_SKIN" };
            menu.AddMenuItem(fatherSkin);
            menu.AddMenuItem(motherSkin);
            MenuSliderItem fatherMotherSkin = new MenuSliderItem($@"Skin color: Blend", 0, 49, 24) { SliderLeftIcon = MenuItem.Icon.MALE, SliderRightIcon = MenuItem.Icon.FEMALE, ItemData = "SKIN_GENEWEIGHT" };
            menu.AddMenuItem(fatherMotherSkin);

            int hairColorPrimary = 0;
            int hairColorSecondary = 0;

            List<string> colorList = new List<string>();
            for (var i = 0; i < 64; i++)
            {
                colorList.Add($"Color #{i}");
            }
            MenuListItem eyeColors = new MenuListItem("Eye Color", GenerateNumberList("Color", 31), 0) { ItemData = "EYE_COLOR" };
            MenuListItem hairColors = new MenuListItem("Primary Hair Color", colorList, 0, "Hair color pallete.") { ShowColorPanel = true, ItemData = "HAIR_PRIM" };
            MenuListItem hairSecondayColors = new MenuListItem("Secondary Hair Color", colorList, 0, "Secondary Hair color pallete.") { ShowColorPanel = true, ItemData = "HAIR_SEC" };

            menu.AddMenuItem(eyeColors);
            menu.AddMenuItem(hairColors);
            menu.AddMenuItem(hairSecondayColors);

            menu.OnMenuOpen += (_menu) =>
            {
                Environment.UI.Location.HideLocation = true;

                if (defaultFov == 0.0f)
                    defaultFov = World.RenderingCamera.FieldOfView;

                World.RenderingCamera.FieldOfView = 30;
            };

            menu.OnMenuClose += (_menu) =>
            {
                Environment.UI.Location.HideLocation = false;
            };

            menu.OnIndexChange += (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                
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

                    await new Model(model).Request(10000);

                    Screen.Fading.FadeOut(500);
                    while (Screen.Fading.IsFadingOut) await BaseScript.Delay(0);

                    await Game.Player.ChangeModel(new Model(model));
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);

                    Player.Creation.ClearTasks();
                    await BaseScript.Delay(333);
                    Player.Creation.IsPedHoldingSign = false;
                    await BaseScript.Delay(333);

                    Screen.Fading.FadeIn(500);
                    while (Screen.Fading.IsFadingIn) await BaseScript.Delay(0);
                }

                if ($"{_listItem.ItemData}" == "FATHER_FACE")
                {
                    blendHeadA = _newSelectionIndex;
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);
                }

                if ($"{_listItem.ItemData}" == "MOTHER_FACE")
                {
                    blendHeadB = _newSelectionIndex;
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);
                }

                if ($"{_listItem.ItemData}" == "FACE_GENEWEIGHT")
                {
                    blendHeadAmount = _newSelectionIndex;
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);
                }

                if ($"{_listItem.ItemData}" == "FATHER_SKIN")
                {
                    blendSkinA = _newSelectionIndex;
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);
                }

                if ($"{_listItem.ItemData}" == "MOTHER_SKIN")
                {
                    blendSkinB = _newSelectionIndex;
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);
                }

                if ($"{_listItem.ItemData}" == "SKIN_GENEWEIGHT")
                {
                    blendSkinAmount = _newSelectionIndex;
                    API.SetPedHeadBlendData(Client.PedHandle, blendHeadA, blendHeadB, 0, blendSkinA, blendSkinB, 0, blendHeadAmount, blendSkinAmount, 0f, false);
                }

                if ($"{_listItem.ItemData}" == "HAIR_PRIM" || $"{_listItem.ItemData}" == "HAIR_SEC")
                {
                    if ($"{_listItem.ItemData}" == "HAIR_PRIM")
                        hairColorPrimary = _newSelectionIndex;

                    if ($"{_listItem.ItemData}" == "HAIR_SEC")
                        hairColorSecondary = _newSelectionIndex;

                    API.SetPedHairColor(Client.PedHandle, hairColorPrimary, hairColorSecondary);
                }

                if ($"{_listItem.ItemData}" == "EYE_COLOR")
                {
                    API.SetPedEyeColor(Client.PedHandle, _newSelectionIndex);
                }
            };

            MenuBase.AddSubMenu(menu);
        }

        public static void RemoveMenu()
        {
            MenuBase.RemoveMenu(menu);
        }

        public static void ManualOpenMenu()
        {
            menu.ParentMenu.OpenMenu();
        }
    }
}
