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
        static VehicleLock lockState = VehicleLock.Unlocked;
        static bool lockBool = false;
        static bool carbootBool = false;

        static List<VehicleWindowIndex> VehicleWindowValues = Enum.GetValues(typeof(VehicleWindowIndex)).OfType<VehicleWindowIndex>().Where(w => (int)w < 4).ToList();
        static List<string> VehicleWindowNames = VehicleWindowValues.Select(d => d.ToString().AddSpacesToCamelCase()).ToList();
        static Dictionary<VehicleWindowIndex, bool> windowStates;

        static List<VehicleDoorIndex> VehicleDoorValues = Enum.GetValues(typeof(VehicleDoorIndex)).OfType<VehicleDoorIndex>().ToList();
        static List<string> VehicleDoorNames = Enum.GetNames(typeof(VehicleDoorIndex)).Select(d => d.AddSpacesToCamelCase()).ToList();

        public static void Init()
        {

            client.RegisterEventHandler("curiosity:Client:Menu:CarLock", new Action<int>(OnToggleLockState));
            client.RegisterEventHandler("curiosity:Client:Menu:Carboot", new Action<int>(OnToggleCarbootState));

            MenuBase.AddSubMenu(menu);

            menu.OnMenuOpen += (_menu) => {

                if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
                {
                    menu.AddMenuItem(new MenuItem("DETINATE THE CAR") { ItemData = "KABOOM" });
                }

                List<string> vehicleLocking = Enum.GetNames(typeof(VehicleLock)).Select(d => d.AddSpacesToCamelCase()).ToList();
                MenuListItem mliVehicleLocks = new MenuListItem("Access Grant", vehicleLocking, (int)lockState, "Select to set vehicle access rights\n~r~Warning:~s~ Changing from a locked state to unlocked may cause your ped to break the window.") { ItemData = "VEHICLE_LOCK" };
                menu.AddMenuItem(mliVehicleLocks);

                MenuCheckboxItem cruiseControlMenuItem = new MenuCheckboxItem("Cruise Control")
                {
                    //Checked = !Vehicle.CruiseControl.IsCruiseControlDisabled,
                    Description = "Enables or disables the cruise control feature",
                    ItemData = CRUISE_CONTROL
                };
                menu.AddMenuItem(cruiseControlMenuItem);

                //MenuCheckboxItem hideThreeDSpeedoMenuItem = new MenuCheckboxItem("3D Speed-o-meter")
                //{
                //    Checked = !Environment.UI.Speedometer3D.Hide,
                //    Description = "Hide or show the 3D Speed-o-meter",
                //    ItemData = THREE_D_SPEEDO
                //};
                //menu.AddMenuItem(hideThreeDSpeedoMenuItem);

                if (Game.PlayerPed.IsInVehicle())
                {
                    MenuCheckboxItem engineMenuItem = new MenuCheckboxItem("Engine")
                    {
                        Checked = Game.PlayerPed.CurrentVehicle.IsEngineRunning,
                        Description = "Turn the engine on/off",
                        ItemData = ENGINE
                    };

                    menu.AddMenuItem(engineMenuItem);

                    SetupWindowsMenu();
                }

                if (Client.CurrentVehicle != null)
                    SetupDoorsMenu();

                if (Game.PlayerPed.IsInVehicle())
                {
                    if (Player.PlayerInformation.IsDeveloper()) DeveloperMenu();
                }
            };

            menu.OnMenuOpen += (_menu) =>
            {
                MenuBase.MenuOpen(true);
            };

            menu.OnMenuClose += (_menu) =>
            {
                MenuBase.MenuOpen(false);
                _menu.ClearMenuItems();
            };

            menu.OnListIndexChange += (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                if (_listItem.ItemData == "VEHICLE_LOCK")
                {
                    OnToggleLockState(Client.CurrentVehicle.Handle);
                }
            };

            menu.OnItemSelect += Menu_OnItemSelect;

            menu.OnCheckboxChange += Menu_OnCheckboxChange;
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem.ItemData == "KABOOM")
            {
                API.NetworkRegisterEntityAsNetworked(Client.CurrentVehicle.Handle);
                BaseScript.TriggerServerEvent("curiosity:Server:Vehicle:Detonate", API.NetworkGetNetworkIdFromEntity(Client.CurrentVehicle.Handle));

                if (Client.CurrentVehicle.IsDead)
                    Client.CurrentVehicle = null;
            }
        }

        static void OnToggleCarbootState(int vehicleId)
        {
            if (Client.CurrentVehicle == null) return;

            if (vehicleId != Client.CurrentVehicle.Handle)
            {
                Environment.UI.Notifications.LifeV(1, "Vehicle", "", "Sorry, you can only affect a car you own.", 2);
                return;
            }

            carbootBool = !carbootBool;

            if (carbootBool)
            {
                API.SetVehicleDoorOpen(vehicleId, 5, false, false);
            }
            else
            {
                API.SetVehicleDoorShut(vehicleId, 5, false);
            }
        }

        static void OnToggleLockState(int vehicleId)
        {
            if (Client.CurrentVehicle == null) return;

            if (vehicleId != Client.CurrentVehicle.Handle)
            {
                Environment.UI.Notifications.LifeV(1, "Vehicle Lock", "", "Sorry, you can only lock a car you own.", 2);
                return;
            }

            lockBool = !lockBool;

            API.SetVehicleAllowNoPassengersLockon(Client.CurrentVehicle.Handle, lockBool);
            API.SetVehicleDoorsLockedForAllPlayers(Client.CurrentVehicle.Handle, lockBool);

            if (lockBool)
            {
                API.PlayVehicleDoorCloseSound(Client.CurrentVehicle.Handle, 1);
                Client.CurrentVehicle.LockStatus = VehicleLockStatus.Locked;
                lockState = VehicleLock.Locked;
                Environment.UI.Notifications.LifeV(1, "Vehicle Locked", "", "", 2);
            }
            else
            {
                API.PlayVehicleDoorOpenSound(Client.CurrentVehicle.Handle, 0);
                Client.CurrentVehicle.LockStatus = VehicleLockStatus.None;
                lockState = VehicleLock.Unlocked;
                Environment.UI.Notifications.LifeV(1, "Vehicle Unlocked", "", "", 2);
            }
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
                MenuBase.MenuOpen(true);
            };

            developerMenu.OnMenuClose += (_menu) =>
            {
                MenuBase.MenuOpen(false);
                _menu.ClearMenuItems();
            };

            developerMenu.OnItemSelect += (Menu menu, MenuItem menuItem, int itemIndex) =>
            {
                if (menuItem.ItemData == "VEHICLE_REPAIR")
                {
                    BaseScript.TriggerEvent("curiosity:Client:Vehicle:DevRepair");
                }

                if (menuItem.ItemData == "VEHICLE_REFUEL")
                {
                    BaseScript.TriggerEvent("curiosity:Client:Vehicle:DevRefuel");
                }
            };

            AddSubMenu(menu, developerMenu);
        }

        private static void SetupDoorsMenu()
        {
            Menu doorsMenu = new Menu("Doors");

            CitizenFX.Core.Vehicle attachedVehicle = null;

            VehicleDoor[] doors = Client.CurrentVehicle.Doors.GetAll();
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
            CitizenFX.Core.Native.API.GetVehicleTrailerVehicle(Client.CurrentVehicle.Handle, ref trailerHandle);

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
                    door = Client.CurrentVehicle.Doors[menuItem.ItemData.index];

                if (menuItem.ItemData.Type == 2)
                    door = attachedVehicle.Doors[menuItem.ItemData.index];

                if (menuItem.Checked) door.Open(); else door.Close((menuItem.ItemData.Type == 2));
            };

            doorsMenu.OnMenuOpen += (_menu) =>
            {
                MenuBase.MenuOpen(true);
            };

            doorsMenu.OnMenuClose += (_menu) =>
            {
                MenuBase.MenuOpen(false);
                _menu.ClearMenuItems();
            };

            AddSubMenu(menu, doorsMenu);
        }

        private static void SetupWindowsMenu()
        {
            Menu windowMenu = new Menu("Windows");
            windowMenu.OnMenuOpen += (_menu) =>
            {
                MenuBase.MenuOpen(true);
                if (windowStates != null)
                    windowStates.Clear();

                windowStates = VehicleWindowValues.ToDictionary(v => v, v => false);

                VehicleWindowValues.Select((window, index) => new { window, index }).ToList().ForEach(o =>
                {
                    var window = Client.CurrentVehicle.Windows[o.window];
                    windowMenu.AddMenuItem(new MenuCheckboxItem($"Roll Down {window.Index.ToString().AddSpacesToCamelCase()}") {
                        Checked = windowStates[window.Index],
                        ItemData = window.Index
                    });
                });
            };

            windowMenu.OnCheckboxChange += (Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState) => {
                VehicleWindow window = Client.CurrentVehicle.Windows[menuItem.ItemData];
                if (menuItem.Checked) window.RollDown(); else window.RollUp();
                windowStates[(VehicleWindowIndex)menuItem.Index] = menuItem.Checked;
            };

            windowMenu.OnMenuClose += (_menu) =>
            {
                MenuBase.MenuOpen(false);
                _menu.ClearMenuItems();
            };

            AddSubMenu(menu, windowMenu);
        }

        private static void Menu_OnCheckboxChange(Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState)
        {
            if (menuItem.ItemData == CRUISE_CONTROL)
                //Vehicle.CruiseControl.IsCruiseControlDisabled = !menuItem.Checked;
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
