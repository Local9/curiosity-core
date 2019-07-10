using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Enums;
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

                if (currentVehicle != Game.PlayerPed.CurrentVehicle)
                    currentVehicle = Game.PlayerPed.CurrentVehicle;

                if (Player.PlayerInformation.IsDeveloper())
                {
                    List<string> vehicleLocking = Enum.GetNames(typeof(VehicleLock)).Select(d => d.AddSpacesToCamelCase()).ToList();
                    MenuListItem mliVehicleLocks = new MenuListItem("DEV: Access Grant", vehicleLocking, 0, "Select to set vehicle access rights\n~r~Warning:~s~ Changing from a locked state to unlocked may cause your ped to break the window.") { ItemData = "VEHICLE_LOCK" };
                    menu.AddMenuItem(mliVehicleLocks);
                }

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
                    MenuCheckboxItem engineMenuItem = new MenuCheckboxItem("Engine")
                    {
                        Checked = Game.PlayerPed.CurrentVehicle.IsEngineRunning,
                        Description = "Turn the engine on/off",
                        ItemData = ENGINE
                    };

                    menu.AddMenuItem(engineMenuItem);

                    SetupDoorsMenu();
                    SetupWindowsMenu();

                    if (Player.PlayerInformation.IsDeveloper()) DeveloperMenu();
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
                if (_listItem.ItemData == "VEHICLE_LOCK")
                {
                    API.SetVehicleExclusiveDriver(currentVehicle.Handle, Client.PedHandle); // Should only do this on spawn

                    if (_newIndex == (int)VehicleLock.Everyone)
                    {
                        API.SetVehicleAllowNoPassengersLockon(currentVehicle.Handle, false);
                        API.SetVehicleDoorsLockedForAllPlayers(currentVehicle.Handle, false);
                        currentVehicle.LockStatus = VehicleLockStatus.None;
                    }

                    if (_newIndex == (int)VehicleLock.PassengersOnly)
                    {
                        API.SetVehicleDoorsLockedForAllPlayers(currentVehicle.Handle, false);
                        API.SetVehicleAllowNoPassengersLockon(currentVehicle.Handle, true);
                        currentVehicle.LockStatus = VehicleLockStatus.None;
                    }

                    if (_newIndex == (int)VehicleLock.NoOne)
                    {
                        currentVehicle.LockStatus = VehicleLockStatus.Locked;
                        API.SetVehicleAllowNoPassengersLockon(currentVehicle.Handle, true);
                        API.SetVehicleDoorsLockedForAllPlayers(currentVehicle.Handle, true);
                    }

                    API.SetVehicleDoorsLockedForPlayer(currentVehicle.Handle, Client.PedHandle, false);
                }
            };

            menu.OnCheckboxChange += Menu_OnCheckboxChange;
        }

        private static void DeveloperMenu()
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;

            Menu developerMenu = new Menu("Developer Options");
            developerMenu.OnMenuOpen += (_menu) =>
            {
                _menu.AddMenuItem(new MenuItem("Repair", "Repair Vehicle") { ItemData = "VEHICLE_REPAIR" });
                _menu.AddMenuItem(new MenuItem("Refuel", "Refuel Vehicle") { ItemData = "VEHICLE_REFUEL" });
            };

            developerMenu.OnMenuOpen += (_menu) =>
            {
                Environment.UI.Location.HideLocation = true;
            };

            developerMenu.OnMenuClose += (_menu) =>
            {
                Environment.UI.Location.HideLocation = false;
                _menu.ClearMenuItems();
            };

            developerMenu.OnItemSelect += (Menu menu, MenuItem menuItem, int itemIndex) =>
            {
                if (menuItem.ItemData == "VEHICLE_REPAIR")
                {
                    Vehicle.VehicleDamage.Fix();
                }

                if (menuItem.ItemData == "VEHICLE_REFUEL")
                {
                    Vehicle.FuelManager.DevRefuel();
                }

            };

            AddSubMenu(menu, developerMenu);
        }

        private static void SetupDoorsMenu()
        {
            Menu doorsMenu = new Menu("Doors");

            CitizenFX.Core.Vehicle attachedVehicle = null;

            VehicleDoor[] doors = currentVehicle.Doors.GetAll();
            VehicleDoor[] attachedDoors = null;

            doors.ToList().ForEach(door =>
            {
                if (!door.IsBroken)
                    doorsMenu.AddMenuItem(new MenuCheckboxItem($"Open {door.Index.ToString().AddSpacesToCamelCase()}")
                    {
                        Checked = door.IsOpen,
                        ItemData = new MyDoor() { Type = 1, index = door.Index }
                    });
            });

            int trailerHandle = 0;
            CitizenFX.Core.Native.API.GetVehicleTrailerVehicle(currentVehicle.Handle, ref trailerHandle);

            if (trailerHandle != 0)
            {
                attachedVehicle = new CitizenFX.Core.Vehicle(trailerHandle);
                attachedDoors = attachedVehicle.Doors.GetAll();

                attachedDoors.ToList().ForEach(door =>
                {
                    if (!door.IsBroken)
                        doorsMenu.AddMenuItem(new MenuCheckboxItem($"Open {door.Index.ToString().AddSpacesToCamelCase()}")
                        {
                            Checked = door.IsOpen,
                            ItemData = new MyDoor() { Type = 2, index = door.Index }
                        });
                });
            }
            else
            {
                attachedVehicle = null;
            }

            doorsMenu.OnCheckboxChange += (Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState) => {
                VehicleDoor door = null;

                if (menuItem.ItemData.Type == 1)
                    door = currentVehicle.Doors[menuItem.ItemData.index];

                if (menuItem.ItemData.Type == 2)
                    door = attachedVehicle.Doors[menuItem.ItemData.index];

                if (menuItem.Checked) door.Open(); else door.Close((menuItem.ItemData.Type == 2));
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

    public class MyDoor
    {
        public int Type;
        public VehicleDoorIndex index;
    }
}
