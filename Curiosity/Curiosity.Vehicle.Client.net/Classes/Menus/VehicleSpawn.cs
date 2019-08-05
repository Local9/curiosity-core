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
        static Client client = Client.GetInstance();
        static VehicleSpawnTypes VehicleSpawnType;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Server:Vehicle:VehicleList", new Action<string>(OnUpdateMenu));
        }

        public static void OpenMenu(VehicleSpawnTypes vehicleSpawnType)
        {
            VehicleSpawnType = vehicleSpawnType;

            if (menu == null)
            {
                menu = new Menu("Vehicle Spawn", "Select a vehicle");
                menu.OnMenuOpen += Menu_OnMenuOpen;
                menu.OnMenuClose += Menu_OnMenuClose;

                MenuController.AddMenu(menu);
                MenuController.EnableMenuToggleKeyOnController = false;
                MenuController.EnableManualGCs = false;
            }

            menu.ClearMenuItems();
            menu.OpenMenu();
        }

        public static void CloseMenu()
        {
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
            menu.AddMenuItem(new MenuItem("Loading..."));
            Client.TriggerServerEvent("curiosity:Server:Vehicle:GetVehicleList", VehicleSpawnType);
        }

        private static void OnUpdateMenu(string json)
        {
            menu.ClearMenuItems();
            // add new items
            // VehicleHash, VehicleName, Enabled, Description
        }
    }
}
