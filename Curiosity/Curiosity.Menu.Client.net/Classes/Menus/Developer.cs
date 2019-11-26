using MenuAPI;
using System;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class Developer
    {
        static bool menuSetup = false;

        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Developer Menu", "Dev tools, what else did you expect?~n~~n~Why does this exist now, BECAUSE FUCK WEATHER!!");

        // Vehicle Options
        static MenuItem mItemRepair = new MenuItem("Repair Vehicle");
        static MenuItem mItemRefuel = new MenuItem("Refuel Vehicle");

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

            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            menu.OnItemSelect += Menu_OnItemSelect;

            menuSetup = true;

            MenuBase.AddSubMenu(menu);
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuBase.MenuOpen(false);
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            MenuBase.MenuOpen(true);
            menu.ClearMenuItems();

            bool enableVehicleOptions = Client.CurrentVehicle != null;

            mItemRefuel.Enabled = enableVehicleOptions;
            menu.AddMenuItem(mItemRefuel);
            mItemRepair.Enabled = enableVehicleOptions;
            menu.AddMenuItem(mItemRepair);
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemRefuel)
                Client.TriggerEvent("curiosity:Client:Vehicle:DevRefuel");

            if (menuItem == mItemRepair)
                Client.TriggerEvent("curiosity:Client:Vehicle:DevRepair");
        }
    }
}
