using CitizenFX.Core;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Shared.Client.net;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Vehicle.Client.net.Classes.Menus
{
    class VehicleSpawnedMenu
    {
        static Menu menu;
        static Client client = Client.GetInstance();
        static Random random = new Random();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Vehicle:SpawnedMenu", new Action<string>(OnUpdateMenu));
        }

        public static void OpenMenu()
        {
            if (menu == null)
            {
                menu = new Menu("Vehicle Options", "Edit your vehicle");
                menu.OnMenuOpen += Menu_OnMenuOpen;
                menu.OnMenuClose += Menu_OnMenuClose;
                menu.OnItemSelect += Menu_OnItemSelect;

                MenuController.AddMenu(menu);
                MenuController.EnableMenuToggleKeyOnController = false;
                MenuController.EnableManualGCs = false;
            }

            menu.ClearMenuItems();
            menu.OpenMenu();
        }

        public static void CloseMenu()
        {
            if (menu != null)
                menu.CloseMenu();
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            menu.ClearMenuItems();
            MenuBaseFunctions.MenuClose();
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            MenuBaseFunctions.MenuOpen();
        }

        private static void OnUpdateMenu(string encodedJson)
        {

        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {

        }
    }
}
