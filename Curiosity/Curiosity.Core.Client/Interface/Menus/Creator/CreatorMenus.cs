using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Extensions;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus.Creator
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
        private UIMenuItem itemSave;
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
        private CharacterApparel _CharacterApparel = new CharacterApparel();
        private CharacterAppearance _CharacterAppearance = new CharacterAppearance();

        internal void CreateMenu()
        {
            // TICKS & SETUP
            _MenuPool = new MenuPool();
            _MenuPool.MouseEdgeEnabled = false;

            //UIMenu
            menuMain = new UIMenu(Game.Player.Name, "Player Creator");
            _MenuPool.Add(menuMain);

            menuMain.OnMenuStateChanged += MenuMain_OnMenuStateChanged;
            menuMain.OnListChange += MenuMain_OnListChange;
            menuMain.OnItemSelect += MenuMain_OnItemSelect;

            // items
            mLstGender = new UIMenuListItem("Gender", Genders, 0);
            menuMain.AddItem(mLstGender);

            menuCharacterHeritage = _MenuPool.AddSubMenu(menuMain, "Heritage");
            _CharacterHeritage.CreateMenu(menuCharacterHeritage);

            menuCharacterFeatures = _MenuPool.AddSubMenu(menuMain, "Features");
            _CharacterFeatures.CreateMenu(menuCharacterFeatures);
            menuCharacterAppearance = _MenuPool.AddSubMenu(menuMain, "Appearance");
            _CharacterAppearance.CreateMenu(menuCharacterAppearance);
            menuCharacterApparel = _MenuPool.AddSubMenu(menuMain, "Apparel");
            _CharacterApparel.CreateMenu(menuCharacterApparel);

            itemSave = new UIMenuItem("Save", "Save your character and enter the world.");
            menuMain.AddItem(itemSave);

            // buttons
            menuMain.InstructionalButtons.Add(btnRotateLeft);
            menuMain.InstructionalButtons.Add(btnRotateRight);

            _MenuPool.RefreshIndex();
        }

        private void MenuMain_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.Closed)
                OnMenuClose();

            if (state == MenuState.Opened)
                OnMenuOpen();
        }

        private async void MenuMain_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == itemSave)
            {
                Cache.Character.MarkedAsRegistered = true;
                await Cache.Character.Save();

                DestroyMenus();
                menuMain.Clear();
                _MenuPool.CloseAllMenus();
                menuMain = null;
            }
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
                Cache.UpdatePedId();

                if (Cache.Character.Heritage == null)
                {
                    Cache.Character.Heritage = new Systems.Library.Models.CharacterHeritage();
                }

                Cache.Character.Gender = newIndex;

                _CharacterHeritage.UpdatePedBlendData();

                await BaseScript.Delay(500);

                playerModel.MarkAsNoLongerNeeded();

                Screen.Fading.FadeIn(1000);
                while (Screen.Fading.IsFadingIn) await BaseScript.Delay(10);
                return;
            }
        }

        private void DestroyMenus()
        {
            PluginManager.Instance.DetachTickHandler(OnMenuCreate);
            PluginManager.Instance.DetachTickHandler(OnPlayerControls);
        }

        private void OnMenuClose()
        {
            PluginManager.Instance.DetachTickHandler(OnPlayerControls);
        }

        private void OnMenuOpen()
        {
            PluginManager.Instance.DiscordRichPresence.Status = "Character Creator";
            PluginManager.Instance.DiscordRichPresence.Commit();

            PluginManager.Instance.AttachTickHandler(OnPlayerControls);
        }

        private async Task OnPlayerControls()
        {
            if (Game.IsControlPressed(0, Control.Pickup))
            {
                Cache.PlayerPed.Heading += 10f;
            }

            if (Game.IsControlPressed(0, Control.Cover))
            {
                Cache.PlayerPed.Heading -= 10f;
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
            PluginManager.Instance.DetachTickHandler(OnPlayerControls);

            menuCharacterHeritage = null;
        }

        internal void AddSubMenu(UIMenu menu, string title)
        {
            _MenuPool.AddSubMenu(menu, title);
        }

        internal void OpenMenu()
        {
            PluginManager.Instance.AttachTickHandler(OnMenuCreate);
            PluginManager.Instance.AttachTickHandler(OnPlayerControls);
        }
    }
}
