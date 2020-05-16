using MenuAPI;
using System;

using static CitizenFX.Core.Native.API;

namespace Curiosity.Menus.Client.net.Classes.Menus.MissionCreator
{
    class MissionMenu
    {
        static bool menuSetup = false;

        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Mission Maker", "WORK IN PROGRESS");

        // Buttons
        static MenuItem menuItemFreecam = new MenuItem("Enter Freecam");

        public static void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            SetupMenu();
        }

        static void OnPlayerSpawned(dynamic dynData)
        {
            SetupMenu();
        }

        static void SetupMenu()
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;

            if (menuSetup) return;

            menuSetup = true;

            menu.AddMenuItem(menuItemFreecam);

            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            MenuBase.AddSubMenu(menu, "WIP", true);
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuBase.MenuOpen(false);
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            MenuBase.MenuOpen(true);
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == menuItemFreecam)
            {
                if (!Classes.NoClipController.IsEnabled)
                {
                    Classes.NoClipController.EnterNoClip();
                    menuItem.Text = "Exit Freecam";
                }
                else
                {
                    Classes.NoClipController.ExitNoClip();
                    menuItem.Text = "Enter Freecam";
                }
            }
        }
    }
}
