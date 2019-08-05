using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Shared.Client.net;

namespace Curiosity.Vehicle.Client.net.Classes.Menus
{
    class VehicleSpawn
    {
        static Menu menu;

        public static void OpenMenu(VehicleSpawnTypes vehicleSpawnType)
        {
            menu = new Menu("Vehicle Spawn", "Select a vehicle");
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            MenuController.AddMenu(menu);
            MenuController.EnableMenuToggleKeyOnController = false;
            MenuController.EnableManualGCs = false;

            menu.OpenMenu();
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuBaseFunctions.MenuClose();
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            MenuBaseFunctions.MenuOpen();
            Log.Verbose("OPEN MENU");
            menu.AddMenuItem(new MenuItem("Loading..."));
        }
    }
}
