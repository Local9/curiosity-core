using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;

namespace Curiosity.Missions.Client.net.Scripts.Police.MenuHandler.Submenu
{
    class TrafficStopInteractions
    {
        static Menu menu;

        static MenuItem mItemFollow = new MenuItem("Follow");
        static MenuItem mItemBreathalyzer = new MenuItem("Breathalyzer");
        static MenuItem mItemDrugTest = new MenuItem("Drug test");
        static MenuItem mItemSearch = new MenuItem("Search");
        static MenuItem mItemGoBack = new MenuItem("Go Back");

        static public void SetupMenu()
        {
            if (menu == null)
            {
                menu = new Menu("Order out of Vehicle", "Interact with the driver");

                menu.OnMenuOpen += Menu_OnMenuOpen;
                menu.OnMenuClose += Menu_OnMenuClose;
                menu.OnItemSelect += Menu_OnItemSelect;

                menu.AddMenuItem(mItemFollow);

                menu.AddMenuItem(mItemBreathalyzer);
                menu.AddMenuItem(mItemDrugTest);
                menu.AddMenuItem(mItemSearch);

                menu.AddMenuItem(mItemGoBack);
            }

            if (TrafficStop.StoppedDriver.IsInVehicle())
            {
                menu.MenuTitle = "Order out of Vehicle";
            }
            else
            {
                menu.MenuTitle = "Interact with the Driver";
            }

            SuspectMenu.AddSubMenu(SuspectMenu.TrafficStopMenu, menu);
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuController.DontOpenAnyMenu = true;
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemBreathalyzer)
            {
            }
            if (menuItem == mItemDrugTest)
            {
            }
            if (menuItem == mItemSearch)
            {
            }
            if (menuItem == mItemFollow)
            {
            }
            if (menuItem == mItemGoBack)
            {
                menu.CloseMenu();
                SuspectMenu.Open();
            }
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", true);
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", true);
            menu.MenuTitle = "";
            MenuController.DontOpenAnyMenu = false;
            SuspectMenu.IsMenuOpen = true;
        }
    }
}
