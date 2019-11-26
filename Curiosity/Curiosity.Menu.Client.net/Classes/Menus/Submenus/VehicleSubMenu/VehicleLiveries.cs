using CitizenFX.Core.Native;
using MenuAPI;
using System.Collections.Generic;

namespace Curiosity.Menus.Client.net.Classes.Menus.Submenus.VehicleSubMenu
{
    class VehicleLiveries
    {
        static Client client = Client.GetInstance();

        static Menu VehicleLiveriesMenu;
        // ITEMS
        static List<string> LiveryList = new List<string>();
        static MenuListItem mListItemLiveryList = new MenuListItem("Set Livery", LiveryList, API.GetVehicleLivery(Client.CurrentVehicle.Handle), "Choose a livery for this vehicle.");
        static MenuItem mItemGoBack = new MenuItem("Go Back", "Go back to the Vehicle Options menu.");
        static MenuItem mItemGoBackNoItems = new MenuItem("No Liveries Available :(", "Go back to the Vehicle Options menu.");

        static public void SetupMenu()
        {
            if (VehicleLiveriesMenu == null)
            {
                VehicleLiveriesMenu = new Menu("Liveries");

                VehicleLiveriesMenu.OnMenuOpen += VehicleLiveriesMenu_OnMenuOpen;
                VehicleLiveriesMenu.OnMenuClose += VehicleLiveriesMenu_OnMenuClose;
                VehicleLiveriesMenu.OnListIndexChange += VehicleLiveriesMenu_OnListIndexChange;
                VehicleLiveriesMenu.OnItemSelect += VehicleLiveriesMenu_OnItemSelect;
            }
            VehicleMenu.AddSubMenu(VehicleLiveriesMenu, Client.CurrentVehicle != null);
        }

        private static void VehicleLiveriesMenu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemGoBackNoItems || menuItem == mItemGoBack)
            {
                VehicleLiveriesMenu.GoBack();
            }
        }

        private static void VehicleLiveriesMenu_OnListIndexChange(Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex)
        {
            if (listItem == mListItemLiveryList)
            {
                Client.CurrentVehicle.Mods.Livery = newSelectionIndex;
            }
        }

        private static void VehicleLiveriesMenu_OnMenuClose(Menu menu)
        {
            MenuBase.MenuOpen(false);
        }

        private static void VehicleLiveriesMenu_OnMenuOpen(Menu menu)
        {
            MenuBase.MenuOpen(true);

            VehicleLiveriesMenu.ClearMenuItems();

            Client.CurrentVehicle.Mods.InstallModKit();

            int liveryCount = Client.CurrentVehicle.Mods.LiveryCount - 1;

            if (liveryCount > 0)
            {
                for (var i = 0; i < liveryCount; i++)
                {
                    var livery = API.GetLiveryName(Client.CurrentVehicle.Handle, i);
                    livery = API.GetLabelText(livery) != "NULL" ? API.GetLabelText(livery) : $"Livery #{i}";
                    LiveryList.Add(livery);
                }
                mListItemLiveryList.ListItems = LiveryList;
                VehicleLiveriesMenu.AddMenuItem(mListItemLiveryList);
                VehicleLiveriesMenu.AddMenuItem(mItemGoBack);
            }
            else
            {
                VehicleLiveriesMenu.AddMenuItem(mItemGoBackNoItems);
            }

            VehicleLiveriesMenu.RefreshIndex();
        }
    }
}
