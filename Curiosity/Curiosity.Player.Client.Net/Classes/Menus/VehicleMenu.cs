using CitizenFX.Core;
using Curiosity.Shared.Client.net.Extensions;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Client.net.Classes.Menus
{
    class VehicleMenu
    {
        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Vehicle", "Vehicle Settings and Options");

        static string CRUISE_CONTROL = "CruiseControl";
        static string THREE_D_SPEEDO = "ThreeDSpeedo";
        static string ENGINE = "Engine";

        static List<VehicleWindowIndex> VehicleWindowValues = Enum.GetValues(typeof(VehicleWindowIndex)).OfType<VehicleWindowIndex>().Where(w => (int)w < 4).ToList();
        static List<string> VehicleWindowNames = VehicleWindowValues.Select(d => d.ToString().AddSpacesToCamelCase()).ToList();
        static Dictionary<VehicleWindowIndex, bool> windowStates;

        static List<VehicleDoorIndex> VehicleDoorValues = Enum.GetValues(typeof(VehicleDoorIndex)).OfType<VehicleDoorIndex>().ToList();
        static List<string> VehicleDoorNames = Enum.GetNames(typeof(VehicleDoorIndex)).Select(d => d.AddSpacesToCamelCase()).ToList();

        static CitizenFX.Core.Vehicle ownedVehicle = null;
        static CitizenFX.Core.Vehicle currentVehicle = null;

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

                menu.AddMenuItem(cruiseControlMenuItem);
                menu.AddMenuItem(hideThreeDSpeedoMenuItem);

                if (Game.PlayerPed.IsInVehicle())
                {
                    currentVehicle = Game.PlayerPed.CurrentVehicle;

                    MenuCheckboxItem engineMenuItem = new MenuCheckboxItem("Engine")
                    {
                        Checked = Game.PlayerPed.CurrentVehicle.IsEngineRunning,
                        Description = "Turn the engine on/off",
                        ItemData = ENGINE
                    };

                    menu.AddMenuItem(engineMenuItem);

                    SetupWindowsMenu();
                    SetupDoorsMenu();
                }

            };

            menu.OnMenuOpen += (_menu) =>
            {
                Environment.UI.Location.HideLocation = true;
            };

            menu.OnMenuClose += (_menu) =>
            {
                Environment.UI.Location.HideLocation = false;
                _menu.ClearMenuItems();
            };

            menu.OnListIndexChange += (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                //Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");
            };

            menu.OnCheckboxChange += Menu_OnCheckboxChange;
        }

        private static void SetupDoorsMenu()
        {
            Menu doorsMenu = new Menu("Doors");

            VehicleDoor[] doors = currentVehicle.Doors.GetAll();
            doors.ToList().ForEach(door =>
            {
                if (!door.IsBroken)
                    doorsMenu.AddMenuItem(new MenuCheckboxItem($"Open {door.Index.ToString().AddSpacesToCamelCase()}")
                    {
                        Checked = door.IsOpen,
                        ItemData = door.Index
                    });
            });

            doorsMenu.OnCheckboxChange += (Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState) => {
                VehicleDoor door = currentVehicle.Doors[menuItem.ItemData];
                if (menuItem.Checked) door.Open(); else door.Close();
            };

            doorsMenu.OnMenuOpen += (_menu) =>
            {
                Environment.UI.Location.HideLocation = true;
            };

            doorsMenu.OnMenuClose += (_menu) =>
            {
                Environment.UI.Location.HideLocation = false;
                _menu.ClearMenuItems();
            };

            AddSubMenu(menu, doorsMenu);
        }

        private static void SetupWindowsMenu()
        {
            Menu windowMenu = new Menu("Windows");
            windowMenu.OnMenuOpen += (_menu) =>
            {
                Environment.UI.Location.HideLocation = true;
                if (windowStates != null)
                    windowStates.Clear();

                windowStates = VehicleWindowValues.ToDictionary(v => v, v => false);

                VehicleWindowValues.Select((window, index) => new { window, index }).ToList().ForEach(o =>
                {
                    var window = currentVehicle.Windows[o.window];
                    windowMenu.AddMenuItem(new MenuCheckboxItem($"Roll Down {window.Index.ToString().AddSpacesToCamelCase()}") {
                        Checked = windowStates[window.Index],
                        ItemData = window.Index
                    });
                });
            };

            windowMenu.OnCheckboxChange += (Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState) => {
                VehicleWindow window = currentVehicle.Windows[menuItem.ItemData];
                if (menuItem.Checked) window.RollDown(); else window.RollUp();
                windowStates[(VehicleWindowIndex)menuItem.Index] = menuItem.Checked;
            };

            windowMenu.OnMenuClose += (_menu) =>
            {
                Environment.UI.Location.HideLocation = false;
                _menu.ClearMenuItems();
            };

            AddSubMenu(menu, windowMenu);
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

        public static void AddSubMenu(Menu menu, Menu submenu)
        {
            MenuController.AddSubmenu(menu, submenu);
            MenuItem submenuButton = new MenuItem(submenu.MenuTitle, submenu.MenuSubtitle) { Label = "→→→" };
            menu.AddMenuItem(submenuButton);
            MenuController.BindMenuItem(menu, submenu, submenuButton);
        }
    }
}
