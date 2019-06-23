using MenuAPI;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Client.net.Classes.Menus
{
    class VehicleMenu
    {
        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Vehicle", "Vehicle Settings and Options");
        static CitizenFX.Core.Vehicle ownedVehicle;

        static string CRUISE_CONTROL = "CruiseControl";
        static string THREE_D_SPEEDO = "ThreeDSpeedo";

        public static void Init()
        {
            MenuBase.AddSubMenu(menu);

            menu.OnMenuOpen += (_menu) => {

                //List<string> vehicleLocking = new List<string>() { "Everyone", "Party", "Clan", "No One" };
                //MenuListItem mliVehicleLocks = new MenuListItem("Access Rights", vehicleLocking, 0, "Select to set vehicle access rights");
                //menu.AddMenuItem(mliVehicleLocks);

                MenuCheckboxItem cruiseControlMenuItem = new MenuCheckboxItem("Cruise Control")
                {
                    Checked = false,
                    Description = "Enables or disables the cruise control feature.",
                    ItemData = CRUISE_CONTROL
                };

                MenuCheckboxItem hideThreeDSpeedo = new MenuCheckboxItem("3D Speed-o-meter")
                {
                    Checked = true,
                    Description = "Hide or show the 3D Speed-o-meter",
                    ItemData = THREE_D_SPEEDO
                };

                menu.AddMenuItem(cruiseControlMenuItem);
                menu.AddMenuItem(hideThreeDSpeedo);

            };

            menu.OnMenuClose += (_menu) =>
            {
                _menu.ClearMenuItems();
            };

            menu.OnListIndexChange += (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                // Code in here would get executed whenever the selected value of a list item changes (when left/right key is pressed).
                Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");
            };

            menu.OnCheckboxChange += Menu_OnCheckboxChange;
        }

        private static void Menu_OnCheckboxChange(Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState)
        {
            if (menuItem.ItemData == CRUISE_CONTROL)
                Vehicle.CruiseControl.IsCruiseControlDisabled = !menuItem.Checked;
            if (menuItem.ItemData == THREE_D_SPEEDO)
                Environment.UI.Speedometer3D.Hide = !menuItem.Checked;
        }
    }
}
