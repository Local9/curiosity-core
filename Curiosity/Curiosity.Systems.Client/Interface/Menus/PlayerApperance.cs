﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Interface.Menus
{
    class PlayerApperance
    {
        private Menu menu;

        private int FatherApperance = 0;
        private int MotherApperance = 0;
        private int ApperanceBlend = 25;
        private int FatherSkin = 0;
        private int MotherSkin = 0;
        private int SkinBlend = 25;

        private int EyeColor = 0;
        private int HairPrimaryColor = 0;
        private int HairSecondaryColor = 0;

        private List<string> Genders = new List<string> { $"{Gender.Male}", $"{Gender.Female}" };
        private List<string> Faces;
        private List<string> Skins;
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

        public void CreateMenu()
        {
            menu = new Menu(Game.Player.Name, "Customize Appereance");
            // Menu Changes
            menu.OnListIndexChange += Menu_OnListIndexChange;
            menu.OnSliderPositionChange += Menu_OnSliderPositionChange;
            // Menu List Content
            Faces = GenerateNumberList("Face", 45);
            Skins = GenerateNumberList("Skin", 45);
            EyeColors = GenerateNumberList("Color", 31);
            HairColors = GenerateNumberList("Color", 64);
            // Menu Items
            mLstGender = new MenuListItem("Gender", Genders, 0);
            mLstFatherApperance = new MenuListItem("Face: Father", Faces, 0);
            mLstMotherApperance = new MenuListItem("Face: Mother", Faces, 0);
            mSldFatherMotherApperanceBlend = new MenuSliderItem("Face: Blend", 0, 49, ApperanceBlend);
            mLstFatherSkin = new MenuListItem("Skin: Father", Skins, 0);
            mLstMotherSkin = new MenuListItem("Skin: Mother", Skins, 0);
            mSldFatherMotherSkinBlend = new MenuSliderItem("Skin: Blend", 0, 49, SkinBlend);
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
            menu.AddMenuItem(mLstEyeColor);
            menu.AddMenuItem(mLstHairColor);
            menu.AddMenuItem(mLstHairSecondaryColor);

            MenuController.AddMenu(menu);
        }

        private void Menu_OnSliderPositionChange(Menu menu, MenuSliderItem sliderItem, int oldPosition, int newPosition, int itemIndex)
        {
            if (sliderItem == mSldFatherMotherApperanceBlend)
            {
                ApperanceBlend = newPosition;
            }

            if (sliderItem == mSldFatherMotherSkinBlend)
            {
                SkinBlend = newPosition;
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
                Cache.Character.Style.Gender = newSelectionIndex;
                UpdatePedBlendData();

                Screen.Fading.FadeIn(500);
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

            if (listItem == mLstFatherApperance)
            {
                MotherSkin = newSelectionIndex;
            }

            UpdatePedBlendData();
        }

        public void OpenMenu()
        {
            menu.OpenMenu();
        }

        public void DestroyMenu()
        {
            // Remove the menu as its no longer required
            if (MenuController.Menus.Contains(menu))
                MenuController.Menus.Remove(menu);

            menu = null;
        }

        private void UpdatePedBlendData()
        {
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
