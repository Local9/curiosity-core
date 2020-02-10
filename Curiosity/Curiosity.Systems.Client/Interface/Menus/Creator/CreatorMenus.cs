using CitizenFX.Core;
using CitizenFX.Core.UI;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Interface.Menus.Creator
{
    class CreatorMenus
    {
        // Menus
        private Menu menuMain;
        private Menu menuCharacterHeritage;
        // private Menu menuPlayerLifeStyle; // 
        private Menu menuCharacterFeatures; // eyes, hair?
        private Menu menuCharacterAppearance; // clothes
        private Menu menuCharacterApparel; // clothing
        private Menu menuCharacterStats; // stats
        // items
        private List<string> Genders = new List<string> { $"{Gender.Male}", $"{Gender.Female}" };
        private MenuListItem mLstGender;

        // Classes
        CharacterHeritage CharacterHeritage = new CharacterHeritage();

        internal void CreateMenu()
        {
            // TICKS & SETUP
            CuriosityPlugin.Instance.AttachTickHandler(OnPlayerControls);

            //MENU
            menuMain = new Menu(Game.Player.Name, "Player Creator");
            menuMain.OnMenuOpen += MainMenu_OnMenuOpen;
            menuMain.OnMenuClose += MainMenu_OnMenuClose;
            menuMain.OnListIndexChange += MenuMain_OnListIndexChange;

            MenuController.AddMenu(menuMain);
            // items
            mLstGender = new MenuListItem("Gender", Genders, 0);
            menuMain.AddMenuItem(mLstGender);

            // submenus
            menuCharacterHeritage = CharacterHeritage.CreateMenu();
            AddSubMenu(menuMain, menuCharacterHeritage, "Heritage");

            // Controls
            menuMain.InstructionalButtons.Add(Control.Cover, "Spin Left");
            menuMain.InstructionalButtons.Add(Control.Pickup, "Spin Right");
        }

        private async void MenuMain_OnListIndexChange(Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex)
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
                    Cache.Character.Style = new Library.Models.CharacterHeritage();
                }

                Cache.Character.Gender = newSelectionIndex;

                CharacterHeritage.UpdatePedBlendData();

                await BaseScript.Delay(500);

                Screen.Fading.FadeIn(1000);
                while (Screen.Fading.IsFadingIn) await BaseScript.Delay(10);
                return;
            }
        }

        private void DestroyMenus()
        {
            // MenuController.Menus.Remove(menuPlayerLifeStyle);
            // MenuController.Menus.Remove(menuPlayerAppearance);
            MenuController.Menus.Remove(menuCharacterHeritage);
            // remove main menu last
            MenuController.Menus.Remove(menuMain);
        }

        private void MainMenu_OnMenuClose(Menu menu)
        {
            CuriosityPlugin.Instance.DetachTickHandler(OnPlayerControls);
        }

        private void MainMenu_OnMenuOpen(Menu menu)
        {
            CuriosityPlugin.Instance.DiscordRichPresence.Status = "Character Creator";
            CuriosityPlugin.Instance.DiscordRichPresence.Commit();

            CuriosityPlugin.Instance.AttachTickHandler(OnPlayerControls);
            MenuController.DisableBackButton = true;
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

        internal void DestroyMenu()
        {
            // Remove the menu as its no longer required
            if (MenuController.Menus.Contains(menuCharacterHeritage))
                MenuController.Menus.Remove(menuCharacterHeritage);

            CuriosityPlugin.Instance.DetachTickHandler(OnPlayerControls);

            menuCharacterHeritage = null;
        }

        internal static void AddSubMenu(Menu menu, Menu submenu, string title, string label = "→→→", bool buttonEnabled = true, string description = "", MenuItem.Icon leftIcon = MenuItem.Icon.NONE, MenuItem.Icon rightIcon = MenuItem.Icon.NONE)
        {
            MenuController.AddSubmenu(menu, submenu);
            MenuItem submenuButton = new MenuItem(title, submenu.MenuSubtitle) { Label = label, LeftIcon = leftIcon, RightIcon = rightIcon };
            if (!buttonEnabled)
            {
                submenuButton = new MenuItem(title, description) { Enabled = buttonEnabled, RightIcon = MenuItem.Icon.LOCK };
            }
            menu.AddMenuItem(submenuButton);
            MenuController.BindMenuItem(menu, submenu, submenuButton);
        }

        internal void OpenMenu()
        {
            menuMain.OpenMenu();
        }
    }
}
