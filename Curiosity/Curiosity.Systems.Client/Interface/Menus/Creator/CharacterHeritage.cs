using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Client.Environment.Entities.Models;
using Curiosity.Systems.Client.Diagnostics;
using Curiosity.Systems.Client.Extensions;
using Curiosity.Systems.Library;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Interface.Menus.Creator
{
    class CharacterHeritage
    {
        private UIMenuHeritageWindow HeritageWindow;
        private UIMenuListItem Mothers;
        private UIMenuListItem Fathers;
        private UIMenuSliderHeritageItem Resemblance;
        private UIMenuSliderHeritageItem SkinTone;

        private int MotherId = 0;
        private int FatherId = 0;

        private int MenuMotherIndex = 0;
        private int MenuFatherIndex = 0;

        private float ResembalanceBlend = .5f;
        private float SkinToneBlend = .5f;

        private Dictionary<string, int> Parents = new Dictionary<string, int>();

        // 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 45
        List<dynamic> MotherFaces = new List<dynamic>() { "Hannah", "Audrey", "Jasmine", "Giselle", "Amelia", "Isabella", "Zoe", "Ava", "Camilla", "Violet", "Sophia", "Eveline", "Nicole", "Ashley", "Grace", "Brianna", "Natalie", "Olivia", "Elizabeth", "Charlotte", "Emma", "Misty" };
        // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 42, 43, 44
        List<dynamic> FatherFaces = new List<dynamic>() { "Benjamin", "Daniel", "Joshua", "Noah", "Andrew", "Joan", "Alex", "Isaac", "Evan", "Ethan", "Vincent", "Angel", "Diego", "Adrian", "Gabriel", "Michael", "Santiago", "Kevin", "Louis", "Samuel", "Anthony", "Claude", "Niko", "John" };

        public UIMenu CreateMenu(UIMenu menu)
        {
            SetupParents();

            menu.OnListChange += Menu_OnListChange;
            menu.OnSliderChange += Menu_OnSliderChange;
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            menu.AddInstructionalButton(CreatorMenus.btnRandom);
            menu.AddInstructionalButton(CreatorMenus.btnRotateLeft);
            menu.AddInstructionalButton(CreatorMenus.btnRotateRight);

            MenuMotherIndex = CuriosityPlugin.Rand.Next(MotherFaces.Count);
            MenuFatherIndex = CuriosityPlugin.Rand.Next(FatherFaces.Count);

            HeritageWindow = new UIMenuHeritageWindow(MenuMotherIndex, MenuFatherIndex);
            menu.AddWindow(HeritageWindow);

            Mothers = new UIMenuListItem("Mother", MotherFaces, MenuMotherIndex);
            Fathers = new UIMenuListItem("Father", FatherFaces, MenuFatherIndex);

            MotherId = Parents[$"{MotherFaces[MenuMotherIndex]}"];
            FatherId = Parents[$"{FatherFaces[MenuFatherIndex]}"];

            Resemblance = new UIMenuSliderHeritageItem("Resemblance", "", true);
            Resemblance.Value = CuriosityPlugin.Rand.Next(100);
            SkinTone = new UIMenuSliderHeritageItem("Skin Tone", "", true);
            SkinTone.Value = CuriosityPlugin.Rand.Next(100);

            ResembalanceBlend = (100 - Resemblance.Value) / 100f;
            SkinToneBlend = (100 - SkinTone.Value) / 100f;

            UpdatePedBlendData();

            menu.AddItem(Mothers);
            menu.AddItem(Fathers);
            menu.AddItem(Resemblance);
            menu.AddItem(SkinTone);

            return menu;
        }

        private async void Menu_OnMenuClose(UIMenu sender)
        {
            Cache.Player.CameraQueue.Reset();
            await Cache.Player.CameraQueue.View(new CameraBuilder()
                .SkipTask()
                .WithMotionBlur(0.5f)
                .WithInterpolation(CreatorMenus.CameraViews[2], CreatorMenus.CameraViews[1], 500)
            );
            CuriosityPlugin.Instance.DetachTickHandler(OnPlayerControls);
        }

        private async void Menu_OnMenuOpen(UIMenu sender)
        {
            Cache.Player.CameraQueue.Reset();
            await Cache.Player.CameraQueue.View(new CameraBuilder()
                .SkipTask()
                .WithMotionBlur(0.5f)
                .WithInterpolation(CreatorMenus.CameraViews[1], CreatorMenus.CameraViews[2], 500)
            );
            CuriosityPlugin.Instance.AttachTickHandler(OnPlayerControls);
        }

        private async Task OnPlayerControls()
        {
            CreatorMenus._MenuPool.ProcessMouse();

            if (Game.IsControlPressed(0, Control.Pickup))
            {
                Game.PlayerPed.Heading += 10f;
            }

            if (Game.IsControlPressed(0, Control.Cover))
            {
                Game.PlayerPed.Heading -= 10f;
            }

            if (Game.IsControlJustPressed(0, Control.Jump))
            {
                Randomise();
            }
        }

        private void Randomise()
        {
            MenuMotherIndex = CuriosityPlugin.Rand.Next(MotherFaces.Count);
            MenuFatherIndex = CuriosityPlugin.Rand.Next(FatherFaces.Count);

            MotherId = MenuMotherIndex;
            FatherId = MenuFatherIndex;

            Mothers.Index = MenuMotherIndex;
            Fathers.Index = MenuFatherIndex;

            Resemblance.Value = CuriosityPlugin.Rand.Next(100);
            SkinTone.Value = CuriosityPlugin.Rand.Next(100);

            ResembalanceBlend = (100 - Resemblance.Value) / 100f;
            SkinToneBlend = (100 - SkinTone.Value) / 100f;

            HeritageWindow.Index(MenuMotherIndex, MenuFatherIndex);
            UpdatePedBlendData();
        }

        private void Menu_OnSliderChange(UIMenu sender, UIMenuSliderItem listItem, int newIndex)
        {
            if (listItem == Resemblance)
                ResembalanceBlend = (100 - newIndex) / 100f;

            if (listItem == SkinTone)
                SkinToneBlend = (100 - newIndex) / 100f;

            UpdatePedBlendData();
        }

        private void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            var value = listItem.Items[newIndex];

            int index = Parents[$"{value}"];

            if (listItem == Mothers)
            {
                MotherId = index;
                MenuMotherIndex = newIndex;
            }

            if (listItem == Fathers)
            {
                FatherId = index;
                MenuFatherIndex = newIndex;
            }

            HeritageWindow.Index(MenuMotherIndex, MenuFatherIndex);
            UpdatePedBlendData();
        }

        public void SetupParents()
        {
            Parents.Add("Benjamin", 0);
            Parents.Add("Daniel", 1);
            Parents.Add("Joshua", 2);
            Parents.Add("Noah", 3);
            Parents.Add("Andrew", 4);
            Parents.Add("Joan", 5);
            Parents.Add("Alex", 6);
            Parents.Add("Isaac", 7);
            Parents.Add("Evan", 8);
            Parents.Add("Ethan", 9);
            Parents.Add("Vincent", 10);
            Parents.Add("Angel", 11);
            Parents.Add("Diego", 12);
            Parents.Add("Adrian", 13);
            Parents.Add("Gabriel", 14);
            Parents.Add("Michael", 15);
            Parents.Add("Santiago", 16);
            Parents.Add("Kevin", 17);
            Parents.Add("Louis", 18);
            Parents.Add("Samuel", 19);
            Parents.Add("Anthony", 20);
            Parents.Add("Hannah", 21);
            Parents.Add("Audrey", 22);
            Parents.Add("Jasmine", 23);
            Parents.Add("Giselle", 24);
            Parents.Add("Amelia", 25);
            Parents.Add("Isabella", 26);
            Parents.Add("Zoe", 27);
            Parents.Add("Ava", 28);
            Parents.Add("Camilla", 29);
            Parents.Add("Violet", 30);
            Parents.Add("Sophia", 31);
            Parents.Add("Eveline", 32);
            Parents.Add("Nicole", 33);
            Parents.Add("Ashley", 34);
            Parents.Add("Grace", 35);
            Parents.Add("Brianna", 36);
            Parents.Add("Natalie", 37);
            Parents.Add("Olivia", 38);
            Parents.Add("Elizabeth", 39);
            Parents.Add("Charlotte", 40);
            Parents.Add("Emma", 41);
            Parents.Add("Claude", 42);
            Parents.Add("Niko", 43);
            Parents.Add("John", 44);
            Parents.Add("Misty", 45);
        }

        public void UpdatePedBlendData()
        {
            // Logger.Debug($"[UpdatePedBlendData] fa/ma {FatherApperance}/{MotherApperance} | fs/ms {FatherSkin}/{MotherSkin} | ab/sb {ApperanceBlend}/{SkinBlend}");

            Cache.Character.Heritage.UpdateBlendData(FatherId, MotherId, ResembalanceBlend, SkinToneBlend);
            API.SetPedHeadBlendData(Cache.Entity.Id, FatherId, MotherId, 0, FatherId, MotherId, 0, ResembalanceBlend, SkinToneBlend, 0f, false);
        }
    }
}
