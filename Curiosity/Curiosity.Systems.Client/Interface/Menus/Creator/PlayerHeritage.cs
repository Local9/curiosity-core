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

namespace Curiosity.Systems.Client.Interface.Menus.Creator
{
    class PlayerHeritage
    {
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

        public Menu CreateMenu()
        {
            Menu playerHeritageMenu = new Menu(Game.Player.Name, "Customize Heritage");
            // Menu Changes
            playerHeritageMenu.OnListIndexChange += Menu_OnListIndexChange;
            playerHeritageMenu.OnSliderPositionChange += Menu_OnSliderPositionChange;

            playerHeritageMenu.OnMenuOpen += PlayerHeritage_OnMenuOpen;
            playerHeritageMenu.OnMenuClose += PlayerHeritage_OnMenuClose;
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
            playerHeritageMenu.AddMenuItem(mLstGender);
            playerHeritageMenu.AddMenuItem(mLstFatherApperance);
            playerHeritageMenu.AddMenuItem(mLstMotherApperance);
            playerHeritageMenu.AddMenuItem(mSldFatherMotherApperanceBlend);
            playerHeritageMenu.AddMenuItem(mLstFatherSkin);
            playerHeritageMenu.AddMenuItem(mLstMotherSkin);
            playerHeritageMenu.AddMenuItem(mSldFatherMotherSkinBlend);

            //menu.AddMenuItem(mLstEyeColor);
            //menu.AddMenuItem(mLstHairColor);
            //menu.AddMenuItem(mLstHairSecondaryColor);

            playerHeritageMenu.InstructionalButtons.Add(Control.Cover, "Spin Left");
            playerHeritageMenu.InstructionalButtons.Add(Control.Pickup, "Spin Right");

            playerHeritageMenu.InstructionalButtons.Add(Control.Jump, "Random");

            UpdatePedBlendData();
            Logger.Info("[PlayerApperance] Created");

            return playerHeritageMenu;
        }

        private async Task OnPlayerControls()
        {
            if (Game.IsControlJustPressed(0, Control.Jump))
            {
                FatherApperance = CuriosityPlugin.Rand.Next(45);
                MotherApperance = CuriosityPlugin.Rand.Next(45);
                ApperanceBlendNumber = CuriosityPlugin.Rand.Next(50);
                ApperanceBlend = ApperanceBlendNumber / 50f;
                FatherSkin = CuriosityPlugin.Rand.Next(45);
                MotherSkin = CuriosityPlugin.Rand.Next(45);
                SkinBlendNumber = CuriosityPlugin.Rand.Next(50);
                SkinBlend = SkinBlendNumber / 50f;

                mLstFatherApperance.ListIndex = FatherApperance;
                mLstMotherApperance.ListIndex = MotherApperance;
                mSldFatherMotherApperanceBlend.Position = ApperanceBlendNumber;
                mLstFatherSkin.ListIndex = FatherSkin;
                mLstMotherSkin.ListIndex = MotherSkin;
                mSldFatherMotherSkinBlend.Position = SkinBlendNumber;

                UpdatePedBlendData();
            }

            if (Game.IsControlPressed(0, Control.Pickup))
            {
                Game.PlayerPed.Heading += 10f;
            }

            if (Game.IsControlPressed(0, Control.Cover))
            {
                Game.PlayerPed.Heading -= 10f;
            }
        }

        private async void PlayerHeritage_OnMenuClose(Menu menu)
        {
            CuriosityPlugin.Instance.DetachTickHandler(OnPlayerControls);

            Cache.Player.CameraQueue.Reset();
            await Cache.Player.CameraQueue.View(new CameraBuilder()
                .SkipTask()
                .WithMotionBlur(0.5f)
                .WithInterpolation(CameraViews[2], CameraViews[1], 500)
            );
        }

        private async void PlayerHeritage_OnMenuOpen(Menu menu)
        {
            CuriosityPlugin.Instance.AttachTickHandler(OnPlayerControls);
            MenuController.DisableBackButton = false;
            
            Cache.Player.CameraQueue.Reset();
            await Cache.Player.CameraQueue.View(new CameraBuilder()
                        .SkipTask()
                        .WithMotionBlur(0.5f)
                        .WithInterpolation(CameraViews[1], CameraViews[2], 500)
                    );
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
