using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.System.Client.Environment.Entities.Models;
using Curiosity.Systems.Client.Diagnostics;
using Curiosity.Systems.Client.Extensions;
using Curiosity.Systems.Library;
using Curiosity.Systems.Library.Models;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Interface.Menus.Creator
{
    class CharacterHeritage
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

        private List<string> Faces;
        private List<string> Skins;

        private MenuListItem mLstFatherApperance;
        private MenuListItem mLstMotherApperance;
        private MenuSliderItem mSldFatherMotherApperanceBlend;

        private MenuListItem mLstFatherSkin;
        private MenuListItem mLstMotherSkin;
        private MenuSliderItem mSldFatherMotherSkinBlend;

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
            Faces = Generators.GenerateNumberList("Face", 45);
            Skins = Generators.GenerateNumberList("Skin", 45);

            // Menu Items
            
            mLstFatherApperance = new MenuListItem("Face: Father", Faces, FatherApperance);
            mLstMotherApperance = new MenuListItem("Face: Mother", Faces, MotherApperance);

            mSldFatherMotherApperanceBlend = new MenuSliderItem("Resemblance", 0, 49, ApperanceBlendNumber) { SliderLeftIcon = MenuItem.Icon.FEMALE, SliderRightIcon = MenuItem.Icon.MALE, ShowDivider = true };
            
            mLstFatherSkin = new MenuListItem("Skin: Father", Skins, FatherSkin);
            mLstMotherSkin = new MenuListItem("Skin: Mother", Skins, MotherSkin);
            
            mSldFatherMotherSkinBlend = new MenuSliderItem("Skin Tone", 0, 49, SkinBlendNumber) { SliderLeftIcon = MenuItem.Icon.FEMALE, SliderRightIcon = MenuItem.Icon.MALE, ShowDivider = true };

            // Add menu Items
            playerHeritageMenu.AddMenuItem(mLstFatherApperance);
            playerHeritageMenu.AddMenuItem(mLstMotherApperance);
            playerHeritageMenu.AddMenuItem(mSldFatherMotherApperanceBlend);
            playerHeritageMenu.AddMenuItem(mLstFatherSkin);
            playerHeritageMenu.AddMenuItem(mLstMotherSkin);
            playerHeritageMenu.AddMenuItem(mSldFatherMotherSkinBlend);

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
            CuriosityPlugin.Instance.DiscordRichPresence.Status = "Player Heritage";
            CuriosityPlugin.Instance.DiscordRichPresence.Commit();

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

        public void UpdatePedBlendData()
        {
            // Logger.Debug($"[UpdatePedBlendData] fa/ma {FatherApperance}/{MotherApperance} | fs/ms {FatherSkin}/{MotherSkin} | ab/sb {ApperanceBlend}/{SkinBlend}");

            Cache.Character.Style.UpdateBlendData(FatherApperance, MotherApperance, FatherSkin, MotherSkin, ApperanceBlend, SkinBlend);
            API.SetPedHeadBlendData(Cache.Entity.Id, FatherApperance, MotherApperance, 0, FatherSkin, MotherSkin, 0, ApperanceBlend, SkinBlend, 0f, false);
        }
    }
}
