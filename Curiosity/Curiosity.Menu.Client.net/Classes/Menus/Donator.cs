using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class Donator
    {
        static Client client = Client.GetInstance();

        static Menu donatorMenu;
        static MenuItem menuItemDonatorVehicles = new MenuItem("Vehicles") { LeftIcon = MenuItem.Icon.INV_CAR };

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
            bool isItemEnabled = (Player.PlayerInformation.IsStaff() || Player.PlayerInformation.IsDonator());
            string description = isItemEnabled ? "Donator Options" : "~b~Pateron: ~y~https://www.patreon.com/lifev\n\n~w~If you have recently donated, please reconnect or contact support.";
            if (donatorMenu == null)
            {
                donatorMenu = new Menu("Donator Menu");

                donatorMenu.OnMenuOpen += Menu_OnMenuOpen;
                donatorMenu.OnMenuClose += Menu_OnMenuClose;

                donatorMenu.OnItemSelect += Menu_OnItemSelect;
                MenuBase.AddSubMenu(donatorMenu, "→→→", isItemEnabled, description, MenuItem.Icon.STAR);
            }

            MenuController.MainMenu.GetMenuItems().ForEach(mitem =>
            {
                if (mitem.Text == donatorMenu.MenuTitle)
                {
                    mitem.Enabled = isItemEnabled;
                    mitem.Description = description;
                    mitem.Label = "→→→";
                    mitem.RightIcon = MenuItem.Icon.NONE;
                    mitem.LeftIcon = MenuItem.Icon.STAR;
                };
            });            
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == menuItemDonatorVehicles)
            {
                Client.TriggerEvent("curiosity:Client:Vehicle:OpenDonatorVehicles");
                menu.CloseMenu();
            }
        }

        private static void Menu_OnMenuClose(Menu menu)
        {

        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            menu.ClearMenuItems();
            menu.AddMenuItem(menuItemDonatorVehicles);
        }
    }
}
