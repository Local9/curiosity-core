using MenuAPI;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Client.net.Classes.Menus
{
    class VehicleMenu
    {
        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Vehicles", "Vehicle Settings");
        static CitizenFX.Core.Vehicle ownedVehicle;

        public static void Init()
        {
            PlayerMenu.AddSubMenu(menu);

            menu.OnMenuOpen += (_menu) => {

                List<string> vehicleLocking = new List<string>() { "Everyone", "Party", "Clan", "No One" };
                MenuListItem mliVehicleLocks = new MenuListItem("Access Rights", vehicleLocking, 0, "Select to set vehicle access rights");
                menu.AddMenuItem(mliVehicleLocks);
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
        }
    }
}
