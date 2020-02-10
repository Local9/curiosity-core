﻿using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Systems.Client.Extensions;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Interface.Menus.Creator
{
    class CreatorMenus
    {
        // menu pool
        public static MenuPool _MenuPool;
        // Menus
        private UIMenu menuMain;
        private UIMenu menuCharacterHeritage;
        // private UIMenu menuPlayerLifeStyle; // 
        private UIMenu menuCharacterFeatures; // eyes, hair?
        private UIMenu menuCharacterAppearance; // clothes
        private UIMenu menuCharacterApparel; // clothing
        private UIMenu menuCharacterStats; // stats
        // Cameras
        public static RotatablePosition[] CameraViews = CharacterExtensions.CameraViews;

        // items
        private List<dynamic> Genders = new List<dynamic> { $"{Gender.Male}", $"{Gender.Female}" };
        private UIMenuListItem mLstGender;

        // buttons
        public static InstructionalButton btnRotateLeft = new InstructionalButton(Control.Cover, "Spin Left");
        public static InstructionalButton btnRotateRight = new InstructionalButton(Control.Pickup, "Spin Right");
        public static InstructionalButton btnRandom = new InstructionalButton(Control.Jump, "Random");

        // Classes
        private CharacterHeritage _CharacterHeritage = new CharacterHeritage();
        private CharacterFeatures _CharacterFeatures = new CharacterFeatures();

        internal void CreateMenu()
        {
            // TICKS & SETUP
            _MenuPool = new MenuPool();
            _MenuPool.MouseEdgeEnabled = false;
            
            //UIMenu
            menuMain = new UIMenu(Game.Player.Name, "Player Creator");
            _MenuPool.Add(menuMain);

            menuMain.OnMenuOpen += MainMenu_OnMenuOpen;
            menuMain.OnMenuClose += MainMenu_OnMenuClose;
            menuMain.OnListChange += MenuMain_OnListChange;

            // items
            mLstGender = new UIMenuListItem("Gender", Genders, 0);
            menuMain.AddItem(mLstGender);

            menuCharacterHeritage = _MenuPool.AddSubMenu(menuMain, "Heritage");
            _CharacterHeritage.CreateMenu(menuCharacterHeritage);
            menuCharacterFeatures = _MenuPool.AddSubMenu(menuMain, "Features");
            _CharacterFeatures.CreateMenu(menuCharacterFeatures);

            // buttons
            menuMain.AddInstructionalButton(btnRotateLeft);
            menuMain.AddInstructionalButton(btnRotateRight);

            _MenuPool.RefreshIndex();
        }

        private async void MenuMain_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == mLstGender) // Player Gender
            {
                Model playerModel = PedHash.FreemodeMale01;
                if (newIndex == 1)
                {
                    playerModel = PedHash.FreemodeFemale01;
                }
                await playerModel.Request(10000);
                Screen.Fading.FadeOut(500);
                while (Screen.Fading.IsFadingOut) await BaseScript.Delay(10);

                await Game.Player.ChangeModel(playerModel);

                if (Cache.Character.Style == null)
                {
                    Cache.Character.Style = new Library.Models.CharacterHeritage();
                }

                Cache.Character.Gender = newIndex;

                _CharacterHeritage.UpdatePedBlendData();

                await BaseScript.Delay(500);

                Screen.Fading.FadeIn(1000);
                while (Screen.Fading.IsFadingIn) await BaseScript.Delay(10);
                return;
            }
        }

        private void DestroyMenus()
        {
            CuriosityPlugin.Instance.DetachTickHandler(OnMenuCreate);
            CuriosityPlugin.Instance.DetachTickHandler(OnPlayerControls);
        }

        private void MainMenu_OnMenuClose(UIMenu UIMenu)
        {
            CuriosityPlugin.Instance.DetachTickHandler(OnPlayerControls);
        }

        private void MainMenu_OnMenuOpen(UIMenu UIMenu)
        {
            CuriosityPlugin.Instance.DiscordRichPresence.Status = "Character Creator";
            CuriosityPlugin.Instance.DiscordRichPresence.Commit();

            CuriosityPlugin.Instance.AttachTickHandler(OnPlayerControls);
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

        private async Task OnMenuCreate()
        {
            _MenuPool.ProcessMenus();
            _MenuPool.ProcessMouse();

            if (!_MenuPool.IsAnyMenuOpen()) // KEEP IT FUCKING OPEN
                menuMain.Visible = true;
        }

        internal void DestroyMenu()
        {
            // Remove the UIMenu as its no longer required
            CuriosityPlugin.Instance.DetachTickHandler(OnPlayerControls);

            menuCharacterHeritage = null;
        }

        internal void AddSubMenu(UIMenu menu, string title)
        {
            _MenuPool.AddSubMenu(menu, title);
        }

        internal void OpenMenu()
        {
            CuriosityPlugin.Instance.AttachTickHandler(OnMenuCreate);
            CuriosityPlugin.Instance.AttachTickHandler(OnPlayerControls);
        }
    }
}
