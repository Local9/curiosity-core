using CitizenFX.Core;
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
        private Menu menuPlayerHeritage;
        private Menu menuPlayerLifeStyle;
        private Menu menuPlayerAppearance;

        internal void CreateMenu()
        {
            // TICKS & SETUP
            CuriosityPlugin.Instance.AttachTickHandler(OnPlayerControls);

            //MENU
            menuMain = new Menu(Game.Player.Name, "Player Creator");
            menuMain.OnMenuOpen += MainMenu_OnMenuOpen;
            menuMain.OnMenuClose += MainMenu_OnMenuClose;
            MenuController.AddMenu(menuMain);

            menuPlayerHeritage = new PlayerHeritage().CreateMenu();
            AddSubMenu(menuMain, menuPlayerHeritage, "Heritage");

            // Controls
            menuMain.InstructionalButtons.Add(Control.Cover, "Spin Left");
            menuMain.InstructionalButtons.Add(Control.Pickup, "Spin Right");
        }

        private void DestroyMenus()
        {
            MenuController.Menus.Remove(menuPlayerHeritage);
            MenuController.Menus.Remove(menuPlayerLifeStyle);
            MenuController.Menus.Remove(menuPlayerAppearance);
            MenuController.Menus.Remove(menuMain);
        }

        private void MainMenu_OnMenuClose(Menu menu)
        {
            CuriosityPlugin.Instance.DetachTickHandler(OnPlayerControls);
        }

        private void MainMenu_OnMenuOpen(Menu menu)
        {
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
            if (MenuController.Menus.Contains(menuPlayerHeritage))
                MenuController.Menus.Remove(menuPlayerHeritage);

            CuriosityPlugin.Instance.DetachTickHandler(OnPlayerControls);

            menuPlayerHeritage = null;
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
