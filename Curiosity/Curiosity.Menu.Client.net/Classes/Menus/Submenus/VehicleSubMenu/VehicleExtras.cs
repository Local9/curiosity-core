using CitizenFX.Core;
using MenuAPI;
using System.Collections.Generic;

namespace Curiosity.Menus.Client.net.Classes.Menus.Submenus.VehicleSubMenu
{
    class VehicleExtras
    {
        static Client client = Client.GetInstance();

        static Dictionary<MenuItem, int> VehicleExtraItems = new Dictionary<MenuItem, int>();
        static Menu VehicleExtrasMenu;

        static MenuItem mItemGoBack = new MenuItem("Go Back", "Go back to the Vehicle Options menu.");
        static MenuItem mItemGoBackNoItems = new MenuItem("No Extras Available :(", "Go back to the Vehicle Options menu.");

        static public void SetupMenu()
        {
            if (VehicleExtrasMenu == null)
            {
                VehicleExtrasMenu = new Menu("Extras");

                VehicleExtrasMenu.OnMenuOpen += VehicleExtrasMenu_OnMenuOpen;
                VehicleExtrasMenu.OnMenuClose += VehicleExtrasMenu_OnMenuClose;
                VehicleExtrasMenu.OnCheckboxChange += VehicleExtrasMenu_OnCheckboxChange;
                VehicleExtrasMenu.OnItemSelect += VehicleExtrasMenu_OnItemSelect;
            }
            VehicleMenu.AddSubMenu(VehicleExtrasMenu, Client.CurrentVehicle != null);
        }

        private static void VehicleExtrasMenu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemGoBackNoItems || menuItem == mItemGoBack)
            {
                VehicleExtrasMenu.GoBack();
            }
        }

        private static void VehicleExtrasMenu_OnCheckboxChange(Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState)
        {
            // When a checkbox is checked/unchecked, get the selected checkbox item index and use that to get the component ID from the list.
            // Then toggle that extra.
            if (VehicleExtraItems.TryGetValue(menuItem, out int extra))
            {
                Vehicle veh = Client.CurrentVehicle;

                int health = veh.Health;
                float bodyHealth = veh.BodyHealth;
                float engineHealth = veh.EngineHealth;

                veh.ToggleExtra(extra, newCheckedState);

                veh.Health = health;
                veh.EngineHealth = engineHealth;
                veh.BodyHealth = bodyHealth;
            }
        }

        private static void VehicleExtrasMenu_OnMenuClose(Menu menu)
        {
            MenuBase.MenuOpen(false);
        }

        private static void VehicleExtrasMenu_OnMenuOpen(Menu menu)
        {
            MenuBase.MenuOpen(true);

            VehicleExtrasMenu.ClearMenuItems();

            Vehicle clientVeh = Client.CurrentVehicle;

            if (clientVeh != null && clientVeh.Exists() && clientVeh.IsAlive && clientVeh.Driver == Game.PlayerPed)
            {
                for (var extra = 0; extra < 64; extra++)
                {
                    if (clientVeh.ExtraExists(extra))
                    {
                        MenuCheckboxItem menuCheckboxItem = new MenuCheckboxItem($"Extra #{extra.ToString()}", extra.ToString(), clientVeh.IsExtraOn(extra));
                        VehicleExtrasMenu.AddMenuItem(menuCheckboxItem);
                        VehicleExtraItems[menuCheckboxItem] = extra;
                    }
                }
            }

            if (VehicleExtraItems.Count == 0)
            {
                VehicleExtrasMenu.AddMenuItem(mItemGoBackNoItems);
            }
            else
            {
                VehicleExtrasMenu.AddMenuItem(mItemGoBack);
            }

            VehicleExtrasMenu.RefreshIndex();
        }
    }
}
