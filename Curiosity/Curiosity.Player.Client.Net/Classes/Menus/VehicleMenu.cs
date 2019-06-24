using CitizenFX.Core;
using MenuAPI;

namespace Curiosity.Client.net.Classes.Menus
{
    class VehicleMenu
    {
        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Vehicle", "Vehicle Settings and Options");
        static CitizenFX.Core.Vehicle ownedVehicle;

        static string CRUISE_CONTROL = "CruiseControl";
        static string THREE_D_SPEEDO = "ThreeDSpeedo";
        static string ENGINE = "Engine";

        public static void Init()
        {
            MenuBase.AddSubMenu(menu);

            menu.OnMenuOpen += (_menu) => {

                //List<string> vehicleLocking = new List<string>() { "Everyone", "Party", "Clan", "No One" };
                //MenuListItem mliVehicleLocks = new MenuListItem("Access Rights", vehicleLocking, 0, "Select to set vehicle access rights");
                //menu.AddMenuItem(mliVehicleLocks);

                MenuCheckboxItem cruiseControlMenuItem = new MenuCheckboxItem("Cruise Control")
                {
                    Checked = !Vehicle.CruiseControl.IsCruiseControlDisabled,
                    Description = "Enables or disables the cruise control feature",
                    ItemData = CRUISE_CONTROL
                };

                MenuCheckboxItem hideThreeDSpeedoMenuItem = new MenuCheckboxItem("3D Speed-o-meter")
                {
                    Checked = !Environment.UI.Speedometer3D.Hide,
                    Description = "Hide or show the 3D Speed-o-meter",
                    ItemData = THREE_D_SPEEDO
                };

                MenuCheckboxItem engineMenuItem = new MenuCheckboxItem("Engine")
                {
                    Checked = Game.PlayerPed.CurrentVehicle.IsEngineRunning,
                    Description = "Turn the engine on/off",
                    ItemData = ENGINE
                };

                menu.AddMenuItem(cruiseControlMenuItem);
                menu.AddMenuItem(hideThreeDSpeedoMenuItem);
                menu.AddMenuItem(engineMenuItem);

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
            if (menuItem.ItemData == ENGINE)
                Game.PlayerPed.CurrentVehicle.IsEngineRunning = menuItem.Checked;
        }
    }
}
