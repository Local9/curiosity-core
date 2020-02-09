using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.System.Client.Environment.Entities.Models;
using Curiosity.Systems.Client.Diagnostics;
using Curiosity.Systems.Client.Extensions;
using Curiosity.Systems.Library.Models;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Interface.Menus
{
    class PlayerAppearance
    {
        private static Menu menu;

        public static RotatablePosition[] CameraViews = CharacterExtensions.CameraViews;

        private bool IsFacecamActive = false;

        private int FatherApperance = 0;
        private int MotherApperance = 0;
        private float ApperanceBlend = .5f;
        private int ApperanceBlendNumber = 25;
        private int FatherSkin = 0;
        private int MotherSkin = 0;
        private float SkinBlend = .5f;
        private int SkinBlendNumber = 25;

        private int EyeColor = 0;
        private int HairPrimaryColor = 0;
        private int HairSecondaryColor = 0;

        private List<string> Genders = new List<string> { $"{Gender.Male}", $"{Gender.Female}" };
        private List<string> MaleFaces;
        private List<string> FemaleFaces;
        private List<string> MaleSkins;
        private List<string> FemaleSkins;
        private List<string> EyeColors;
        private List<string> HairColors;

        private MenuListItem mLstGender;

        private MenuListItem mLstFatherApperance;
        private MenuListItem mLstMotherApperance;
        private MenuSliderItem mSldFatherMotherApperanceBlend;

        private MenuListItem mLstFatherSkin;
        private MenuListItem mLstMotherSkin;
        private MenuSliderItem mSldFatherMotherSkinBlend;

        private MenuListItem mLstEyeColor;
        private MenuListItem mLstHairColor;
        private MenuListItem mLstHairSecondaryColor;

        // Male Faces 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 42, 43, 44
        // Female Faces 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 45

        private void SetupLists()
        {
            MaleFaces = new List<string>() { };
            FemaleFaces = new List<string>() { };
        }

        public void CreateMenu()
        {
            menu = new Menu(Game.Player.Name, "Customize Heritage");
            // Menu Changes
            menu.OnListIndexChange += Menu_OnListIndexChange;
            menu.OnSliderPositionChange += Menu_OnSliderPositionChange;
            menu.OnIndexChange += Menu_OnIndexChange;
            // random settings

            FatherApperance = CuriosityPlugin.Rand.Next(45);
            MotherApperance = CuriosityPlugin.Rand.Next(45);
            ApperanceBlendNumber = CuriosityPlugin.Rand.Next(50);
            ApperanceBlend = ApperanceBlendNumber / 50f;
            FatherSkin = CuriosityPlugin.Rand.Next(45);
            MotherSkin = CuriosityPlugin.Rand.Next(45);
            SkinBlendNumber = CuriosityPlugin.Rand.Next(50);
            SkinBlend = SkinBlendNumber / 50f;
            
            // Menu List Content
            MaleFaces = GenerateNumberList("Face", 45);
            FemaleFaces = GenerateNumberList("Face", 45);

            MaleSkins = GenerateNumberList("Skin", 45);
            FemaleSkins = GenerateNumberList("Skin", 45);

            EyeColors = GenerateNumberList("Color", 31);
            HairColors = GenerateNumberList("Color", 64);
            // Menu Items
            mLstGender = new MenuListItem("Gender", Genders, 0);
            
            mLstFatherApperance = new MenuListItem("Face: Father", MaleFaces, FatherApperance);
            mLstMotherApperance = new MenuListItem("Face: Mother", FemaleFaces, MotherApperance);

            mSldFatherMotherApperanceBlend = new MenuSliderItem("Face: Blend", 0, 49, ApperanceBlendNumber) { SliderLeftIcon = MenuItem.Icon.FEMALE, SliderRightIcon = MenuItem.Icon.MALE };
            
            mLstFatherSkin = new MenuListItem("Skin: Father", MaleSkins, FatherSkin);
            mLstMotherSkin = new MenuListItem("Skin: Mother", FemaleSkins, MotherSkin);
            
            mSldFatherMotherSkinBlend = new MenuSliderItem("Skin: Blend", 0, 49, SkinBlendNumber) { SliderLeftIcon = MenuItem.Icon.FEMALE, SliderRightIcon = MenuItem.Icon.MALE };

            // Move to Appearance
            mLstEyeColor = new MenuListItem("Eye Color", EyeColors, 0);
            mLstHairColor = new MenuListItem("Primary Hair Color", HairColors, 0);
            mLstHairSecondaryColor = new MenuListItem("Secondary Hair Color", HairColors, 0);
            // Add menu Items
            menu.AddMenuItem(mLstGender);
            menu.AddMenuItem(mLstFatherApperance);
            menu.AddMenuItem(mLstMotherApperance);
            menu.AddMenuItem(mSldFatherMotherApperanceBlend);
            menu.AddMenuItem(mLstFatherSkin);
            menu.AddMenuItem(mLstMotherSkin);
            menu.AddMenuItem(mSldFatherMotherSkinBlend);

            //menu.AddMenuItem(mLstEyeColor);
            //menu.AddMenuItem(mLstHairColor);
            //menu.AddMenuItem(mLstHairSecondaryColor);

            UpdatePedBlendData();

            MenuController.AddMenu(menu);
            MenuController.DisableBackButton = true;

            CuriosityPlugin.Instance.AttachTickHandler(OnPlayerControls);

            Logger.Info("[PlayerApperance] Created");
        }

        private async Task OnPlayerControls()
        {
            if (Game.IsControlPressed(0, Control.Pickup))
            {
                Game.PlayerPed.Heading += 10f;
            }

            if (Game.IsControlPressed(0, Control.Cover))
            {
                Game.PlayerPed.Heading -= 10f;
            }
        }

        private async void Menu_OnIndexChange(Menu menu, MenuItem oldItem, MenuItem newItem, int oldIndex, int newIndex)
        {
            if (newItem == mLstFatherApperance || newItem == mLstMotherApperance || newItem == mLstEyeColor || newItem == mSldFatherMotherApperanceBlend)
            {
                if (IsFacecamActive) return;

                IsFacecamActive = true;

                Cache.Player.CameraQueue.Reset();
                await Cache.Player.CameraQueue.View(new CameraBuilder()
                            .SkipTask()
                            .WithMotionBlur(0.5f)
                            .WithInterpolation(CameraViews[1], CameraViews[2], 500)
                        );
            }
            else
            {
                if (!IsFacecamActive) return;

                IsFacecamActive = false;

                Cache.Player.CameraQueue.Reset();
                await Cache.Player.CameraQueue.View(new CameraBuilder()
                    .SkipTask()
                    .WithMotionBlur(0.5f)
                    .WithInterpolation(CameraViews[2], CameraViews[1], 500)
                );
            }
        }

        private void Menu_OnSliderPositionChange(Menu menu, MenuSliderItem sliderItem, int oldPosition, int newPosition, int itemIndex)
        {
            if (sliderItem == mSldFatherMotherApperanceBlend)
            {
                ApperanceBlend = newPosition / 50f;
            }

            if (sliderItem == mSldFatherMotherSkinBlend)
            {
                SkinBlend = newPosition / 50f;
            }

            UpdatePedBlendData();
        }

        private async void Menu_OnListIndexChange(Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex)
        {
            if (listItem == mLstGender) // Player Gender
            {
                Model playerModel = PedHash.FreemodeMale01;
                if (newSelectionIndex == 1)
                {
                    playerModel = PedHash.FreemodeFemale01;
                }
                await playerModel.Request(10000);
                Screen.Fading.FadeOut(500);
                while (Screen.Fading.IsFadingOut) await BaseScript.Delay(10);

                await Game.Player.ChangeModel(playerModel);

                if (Cache.Character.Style == null)
                {
                    Cache.Character.Style = new Library.Models.Style();
                }

                Cache.Character.Style.Gender = newSelectionIndex;
                UpdatePedBlendData();

                await BaseScript.Delay(500);

                Screen.Fading.FadeIn(1000);
                while (Screen.Fading.IsFadingIn) await BaseScript.Delay(10);
                return;
            }

            if (listItem == mLstEyeColor)
            {
                EyeColor = newSelectionIndex;
                API.SetPedEyeColor(Cache.Entity.Id, newSelectionIndex);
                Cache.Character.Style.EyeColor = newSelectionIndex;
                return;
            }

            if (listItem == mLstHairColor || listItem == mLstHairSecondaryColor)
            {
                if (listItem == mLstHairColor)
                    HairPrimaryColor = newSelectionIndex;

                if (listItem == mLstHairSecondaryColor)
                    HairSecondaryColor = newSelectionIndex;

                API.SetPedHairColor(Cache.Entity.Id, HairPrimaryColor, HairSecondaryColor);
                Cache.Character.Style.UpdateHairData(HairPrimaryColor, HairSecondaryColor);
                return;
            }

            if (listItem == mLstFatherApperance)
            {
                FatherApperance = newSelectionIndex;
            }

            if (listItem == mLstMotherApperance)
            {
                MotherApperance = newSelectionIndex;
            }

            if (listItem == mLstFatherSkin)
            {
                FatherSkin = newSelectionIndex;
            }

            if (listItem == mLstMotherSkin)
            {
                MotherSkin = newSelectionIndex;
            }

            UpdatePedBlendData();
        }

        public void OpenMenu()
        {
            menu.OpenMenu();
            Logger.Info("[PlayerApperance] Open");
        }

        public void DestroyMenu()
        {
            // Remove the menu as its no longer required
            if (MenuController.Menus.Contains(menu))
                MenuController.Menus.Remove(menu);

            CuriosityPlugin.Instance.DetachTickHandler(OnPlayerControls);

            menu = null;
        }

        private void UpdatePedBlendData()
        {
            Logger.Debug($"[UpdatePedBlendData] fa/ma {FatherApperance}/{MotherApperance} | fs/ms {FatherSkin}/{MotherSkin} | ab/sb {ApperanceBlend}/{SkinBlend}");

            Cache.Character.Style.UpdateBlendData(FatherApperance, MotherApperance, FatherSkin, MotherSkin, ApperanceBlend, SkinBlend);
            API.SetPedHeadBlendData(Cache.Entity.Id, FatherApperance, MotherApperance, 0, FatherSkin, MotherSkin, 0, ApperanceBlend, SkinBlend, 0f, false);
        }

        public static List<string> GenerateNumberList(string txt, int max, int min = 0)
        {
            List<string> lst = new List<string>();
            for (int i = min; i < max + 1; i++)
                lst.Add($"{txt} #{i.ToString()}");
            return lst;
        }
    }
}
