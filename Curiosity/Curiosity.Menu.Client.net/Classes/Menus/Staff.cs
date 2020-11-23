using Curiosity.Menus.Client.net.Classes.Player;
using MenuAPI;
using System;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class Staff
    {
        static Client client = Client.GetInstance();

        static Menu staffMenu;

        static MenuItem staffVehicles = new MenuItem("Vehicles");
        static MenuItem subMenuItem;

        public static void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
            SetupMenu();
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
            if (!PlayerInformation.IsStaff()) return;
            
            if (staffMenu == null)
            {
                staffMenu = new Menu("Staff Menu", "Some options for fun");

                staffMenu.OnMenuOpen += Menu_OnMenuOpen;
                staffMenu.OnMenuClose += Menu_OnMenuClose;

                staffMenu.OnItemSelect += Menu_OnItemSelect;

                staffMenu.AddMenuItem(staffVehicles);

                subMenuItem = MenuBase.AddSubMenu(staffMenu, "→→→", true, string.Empty, MenuItem.Icon.ROCKSTAR);
            }

            MenuController.MainMenu.GetMenuItems().ForEach(mitem =>
            {
                if (mitem.Text == staffMenu.MenuTitle)
                {
                    mitem.Enabled = true;
                    mitem.Description = "Staff Menu";
                    mitem.Label = "→→→";
                    mitem.RightIcon = MenuItem.Icon.NONE;
                    mitem.LeftIcon = MenuItem.Icon.ROCKSTAR;
                };
            });
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == staffVehicles)
            {
                Client.TriggerEvent("curiosity:Client:Vehicle:OpenStaffVehicles");
                menu.CloseMenu();
            }
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuBase.MenuOpen(false);
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            MenuBase.MenuOpen(true);
        }

        public static string GetVehDisplayNameFromModel(string name) => GetLabelText(GetDisplayNameFromVehicleModel((uint)GetHashKey(name)));
        public static bool DoesModelExist(uint modelHash) => IsModelInCdimage(modelHash);
    }
}
