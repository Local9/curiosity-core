using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Environment.Entities.Models;
using NativeUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus.Creator
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
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;

            if (Cache.Character is not null)
            {
                if (Cache.Character.Heritage is not null)
                {
                    MenuMotherIndex = Cache.Character.Heritage.MotherId;
                    MenuFatherIndex = Cache.Character.Heritage.FatherId;
                }
                else
                {
                    MenuMotherIndex = PluginManager.Rand.Next(MotherFaces.Count);
                    MenuFatherIndex = PluginManager.Rand.Next(FatherFaces.Count);
                }
            }
            else
            {
                MenuMotherIndex = PluginManager.Rand.Next(MotherFaces.Count);
                MenuFatherIndex = PluginManager.Rand.Next(FatherFaces.Count);
            }

            if (MenuMotherIndex < 0)
                MenuMotherIndex = 0;

            if (MenuFatherIndex < 0)
                MenuFatherIndex = 0;

            HeritageWindow = new UIMenuHeritageWindow(MenuMotherIndex, MenuFatherIndex);
            menu.AddWindow(HeritageWindow);

            Mothers = new UIMenuListItem("Mother", MotherFaces, MenuMotherIndex);
            Fathers = new UIMenuListItem("Father", FatherFaces, MenuFatherIndex);

            MotherId = Parents[$"{MotherFaces[MenuMotherIndex]}"];
            FatherId = Parents[$"{FatherFaces[MenuFatherIndex]}"];

            Resemblance = new UIMenuSliderHeritageItem("Resemblance", "", true);
            Resemblance.Value = PluginManager.Rand.Next(100);
            SkinTone = new UIMenuSliderHeritageItem("Skin Tone", "", true);
            SkinTone.Value = PluginManager.Rand.Next(100);

            ResembalanceBlend = (100 - Resemblance.Value) / 100f;
            SkinToneBlend = (100 - SkinTone.Value) / 100f;

            UpdatePedBlendData(true);

            menu.AddItem(Mothers);
            menu.AddItem(Fathers);
            menu.AddItem(Resemblance);
            menu.AddItem(SkinTone);

            return menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeBackward)
                OnMenuClose(oldMenu);

            if (state == MenuState.ChangeForward)
                OnMenuOpen(newMenu);
        }

        private async void OnMenuClose(UIMenu menu)
        {
            Cache.Player.CameraQueue.Reset();
            await Cache.Player.CameraQueue.View(new CameraBuilder()
                .SkipTask()
                .WithMotionBlur(0.5f)
                .WithInterpolation(CreatorMenus.CameraViews[2], CreatorMenus.CameraViews[1], 500)
            );
            PluginManager.Instance.DetachTickHandler(OnPlayerControls);

            menu.InstructionalButtons.Clear();
        }

        private async void OnMenuOpen(UIMenu menu)
        {
            Cache.Player.CameraQueue.Reset();
            await Cache.Player.CameraQueue.View(new CameraBuilder()
                .SkipTask()
                .WithMotionBlur(0.5f)
                .WithInterpolation(CreatorMenus.CameraViews[1], CreatorMenus.CameraViews[2], 500)
            );
            PluginManager.Instance.AttachTickHandler(OnPlayerControls);

            menu.InstructionalButtons.Clear();

            menu.InstructionalButtons.Add(CreatorMenus.btnRandom);
            menu.InstructionalButtons.Add(CreatorMenus.btnRotateLeft);
            menu.InstructionalButtons.Add(CreatorMenus.btnRotateRight);
        }

        private async Task OnPlayerControls()
        {
            CreatorMenus._MenuPool.ProcessMouse();

            if (Game.IsControlPressed(0, Control.Pickup))
            {
                Cache.PlayerPed.Heading += 10f;
            }

            if (Game.IsControlPressed(0, Control.Cover))
            {
                Cache.PlayerPed.Heading -= 10f;
            }

            if (Game.IsControlJustPressed(0, Control.Jump))
            {
                Randomise();
            }
        }

        private void Randomise()
        {
            MenuMotherIndex = PluginManager.Rand.Next(MotherFaces.Count);
            MenuFatherIndex = PluginManager.Rand.Next(FatherFaces.Count);

            MotherId = MenuMotherIndex;
            FatherId = MenuFatherIndex;

            Mothers.Index = MenuMotherIndex;
            Fathers.Index = MenuFatherIndex;

            Resemblance.Value = PluginManager.Rand.Next(100);
            SkinTone.Value = PluginManager.Rand.Next(100);

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

        public void UpdatePedBlendData(bool checkExisting = false)
        {
            // Logger.Debug($"[UpdatePedBlendData] fa/ma {FatherApperance}/{MotherApperance} | fs/ms {FatherSkin}/{MotherSkin} | ab/sb {ApperanceBlend}/{SkinBlend}");
            if (checkExisting)
            {
                FatherId = Cache.Character.Heritage.FatherId;
                MotherId = Cache.Character.Heritage.MotherId;
                ResembalanceBlend = Cache.Character.Heritage.BlendApperance;
                SkinToneBlend = Cache.Character.Heritage.BlendSkin;
            }

            Cache.Character.Heritage.UpdateBlendData(FatherId, MotherId, ResembalanceBlend, SkinToneBlend);
            API.SetPedHeadBlendData(Cache.Entity.Id, FatherId, MotherId, 0, FatherId, MotherId, 0, ResembalanceBlend, SkinToneBlend, 0f, false);
        }
    }
}
