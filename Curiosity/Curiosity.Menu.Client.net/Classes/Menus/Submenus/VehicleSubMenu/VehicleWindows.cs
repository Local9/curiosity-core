using CitizenFX.Core;
using Curiosity.Shared.Client.net.Extensions;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Menus.Client.net.Classes.Menus.Submenus.VehicleSubMenu
{
    class VehicleWindows
    {
        static Client client = Client.GetInstance();

        static Menu VehicleWindowsMenu;

        static List<VehicleWindowIndex> VehicleWindowValues = Enum.GetValues(typeof(VehicleWindowIndex)).OfType<VehicleWindowIndex>().Where(w => (int)w < 4).ToList();
        static List<string> VehicleWindowNames = VehicleWindowValues.Select(d => d.ToString().AddSpacesToCamelCase()).ToList();
        static Dictionary<VehicleWindowIndex, bool> VehicleWindowStates = new Dictionary<VehicleWindowIndex, bool>();

        static public void SetupMenu()
        {
            if (VehicleWindowsMenu == null)
            {
                VehicleWindowsMenu = new Menu("Windows");

                VehicleWindowsMenu.OnMenuClose += VehicleWindowsMenu_OnMenuClose;
                VehicleWindowsMenu.OnMenuOpen += VehicleWindowsMenu_OnMenuOpen;
                VehicleWindowsMenu.OnCheckboxChange += VehicleWindowsMenu_OnCheckboxChange;
            }
            VehicleMenu.AddSubMenu(VehicleWindowsMenu);
        }

        private static void VehicleWindowsMenu_OnCheckboxChange(Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState)
        {
            VehicleWindow window = Client.CurrentVehicle.Windows[menuItem.ItemData];
            if (menuItem.Checked) window.RollDown(); else window.RollUp();
            VehicleWindowStates[(VehicleWindowIndex)menuItem.Index] = menuItem.Checked;
        }

        private static void VehicleWindowsMenu_OnMenuOpen(Menu menu)
        {
            MenuBase.MenuOpen(true);
            VehicleWindowsMenu.ClearMenuItems();

            if (VehicleWindowStates != null)
                VehicleWindowStates.Clear();

            VehicleWindowStates = VehicleWindowValues.ToDictionary(v => v, v => false);

            VehicleWindowValues.Select((window, index) => new { window, index }).ToList().ForEach(o =>
            {
                var window = Client.CurrentVehicle.Windows[o.window];
                VehicleWindowsMenu.AddMenuItem(new MenuCheckboxItem($"Roll Down {window.Index.ToString().AddSpacesToCamelCase()}")
                {
                    Checked = VehicleWindowStates[window.Index],
                    ItemData = window.Index
                });
            });
        }

        private static void VehicleWindowsMenu_OnMenuClose(Menu menu)
        {
            MenuBase.MenuOpen(false);
        }
    }
}
