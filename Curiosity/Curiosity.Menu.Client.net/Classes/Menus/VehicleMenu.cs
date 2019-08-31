using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Enums;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class VehicleMenu
    {
        private const string VTOL = "vtol";
        static Client client = Client.GetInstance();
        static Menu mainVehicleMenu = new Menu("Vehicle", "Vehicle Settings and Options");

        static string CRUISE_CONTROL = "CruiseControl";
        static string ENGINE = "Engine";
        static VehicleLock lockState = VehicleLock.Unlocked;
        static bool lockBool = false;
        static bool carbootBool = false;
        static bool carBackDoorsBool = false;
        static bool inVTOL = false;

        static List<VehicleWindowIndex> VehicleWindowValues = Enum.GetValues(typeof(VehicleWindowIndex)).OfType<VehicleWindowIndex>().Where(w => (int)w < 4).ToList();
        static List<string> VehicleWindowNames = VehicleWindowValues.Select(d => d.ToString().AddSpacesToCamelCase()).ToList();
        static Dictionary<VehicleWindowIndex, bool> windowStates;

        static List<VehicleDoorIndex> VehicleDoorValues = Enum.GetValues(typeof(VehicleDoorIndex)).OfType<VehicleDoorIndex>().ToList();
        static List<string> VehicleDoorNames = Enum.GetNames(typeof(VehicleDoorIndex)).Select(d => d.AddSpacesToCamelCase()).ToList();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Menu:CarLock", new Action<int>(OnToggleLockState));
            client.RegisterEventHandler("curiosity:Client:Menu:Carboot", new Action<int>(OnToggleCarbootState));

            mainVehicleMenu.OnMenuOpen += OnMainMenuOpen;
            mainVehicleMenu.OnMenuClose += OnMainMenuClose;
            mainVehicleMenu.OnListIndexChange += OnMainMenuListIndexChange;
            mainVehicleMenu.OnItemSelect += Menu_OnItemSelect;
            mainVehicleMenu.OnCheckboxChange += Menu_OnCheckboxChange;

            MenuBase.AddSubMenu(mainVehicleMenu);
        }

        private static void OnMainMenuOpen(Menu menu)
        {
            MenuBase.MenuOpen(true);

            if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
            {
                mainVehicleMenu.AddMenuItem(new MenuItem("DETINATE VEHICLE") { ItemData = "KABOOM" });
            }

            List<string> vehicleLocking = Enum.GetNames(typeof(VehicleLock)).Select(d => d.AddSpacesToCamelCase()).ToList();
            MenuListItem mliVehicleLocks = new MenuListItem("Access Grant", vehicleLocking, (int)lockState, "Select to set vehicle access rights\n~r~Warning:~s~ Changing from a locked state to unlocked may cause your ped to break the window.") { ItemData = "VEHICLE_LOCK" };
            mliVehicleLocks.Enabled = Client.CurrentVehicle != null;
            mainVehicleMenu.AddMenuItem(mliVehicleLocks);

            MenuCheckboxItem cruiseControlMenuItem = new MenuCheckboxItem("Cruise Control")
            {
                //Checked = !Vehicle.CruiseControl.IsCruiseControlDisabled,
                Description = "Enables or disables the cruise control feature",
                ItemData = CRUISE_CONTROL
            };
            mainVehicleMenu.AddMenuItem(cruiseControlMenuItem);

            MenuCheckboxItem engineMenuItem = new MenuCheckboxItem("Engine")
            {
                Checked = Client.CurrentVehicle.IsEngineRunning,
                Description = "Turn the engine on/off",
                ItemData = ENGINE
            };
            mainVehicleMenu.AddMenuItem(engineMenuItem);

            SetupWindowsMenu();
            SetupDoorsMenu();

            if (Game.PlayerPed.IsInVehicle())
            {
                if (Game.PlayerPed.CurrentVehicle.Driver.Handle == Game.PlayerPed.Handle)
                {
                    if (Game.PlayerPed.CurrentVehicle.ClassType == VehicleClass.Planes)
                    {
                        Model model = new Model("avenger");
                        if (API.IsVehicleModel(Game.PlayerPed.CurrentVehicle.GetHashCode(), (uint)model.Hash))
                        {

                            inVTOL = API.GetPlaneHoverModePercentage(Game.PlayerPed.CurrentVehicle.Handle) > 0.0f;

                            MenuCheckboxItem vtol = new MenuCheckboxItem("VTOL")
                            {
                                Checked = inVTOL,
                                Description = "Turn VTOL on/off",
                                ItemData = VTOL
                            };
                            mainVehicleMenu.AddMenuItem(vtol);
                        }
                        model.MarkAsNoLongerNeeded();
                    }
                }
            }

            if (Player.PlayerInformation.IsDeveloper() && Game.PlayerPed.IsInVehicle()) DeveloperMenu();
        }
    

        private static void OnMainMenuListIndexChange(Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex)
        {
            if (listItem.ItemData == "VEHICLE_LOCK")
            {
                OnToggleLockState(Client.CurrentVehicle.Handle);
            }
        }

        private static void OnMainMenuClose(Menu menu)
        {
            menu.ClearMenuItems();
            MenuBase.MenuOpen(false);
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
                Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle", "", "Sorry, you can only affect a car you own.", 2);
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

        static void OnToggleBackDoors()
        {
            if (Client.CurrentVehicle == null) return;

            carBackDoorsBool = !carBackDoorsBool;

            if (carBackDoorsBool)
            {
                API.SetVehicleDoorOpen(Client.CurrentVehicle.Handle, 2, false, false);
                API.SetVehicleDoorOpen(Client.CurrentVehicle.Handle, 3, false, false);
            }
            else
            {
                API.SetVehicleDoorShut(Client.CurrentVehicle.Handle, 2, false);
                API.SetVehicleDoorShut(Client.CurrentVehicle.Handle, 3, false);
            }
        }

        static void OnToggleLockState(int vehicleId)
        {
            if (Client.CurrentVehicle == null) return;

            if (vehicleId != Client.CurrentVehicle.Handle)
            {
                Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle Lock", "", "Sorry, you can only lock a car you own.", 2);
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
                Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle Locked", "", "", 2);
            }
            else
            {
                API.PlayVehicleDoorOpenSound(Client.CurrentVehicle.Handle, 0);
                Client.CurrentVehicle.LockStatus = VehicleLockStatus.None;
                lockState = VehicleLock.Unlocked;
                Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle Unlocked", "", "", 2);
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

            AddSubMenu(mainVehicleMenu, developerMenu);
        }

        private static void SetupDoorsMenu()
        {
            Menu doorsMenu = new Menu("Doors");

            if (Client.CurrentVehicle != null)
            {
                CitizenFX.Core.Vehicle attachedVehicle = null;

                VehicleDoor[] doors = Client.CurrentVehicle.Doors.GetAll();
                VehicleDoor[] attachedDoors = null;
                bool addBothBackDoors = false;

                doors.ToList().ForEach(door =>
                {
                    if (!door.IsBroken)
                    {
                        doorsMenu.AddMenuItem(new MenuCheckboxItem($"Open {door.Index.ToString().AddSpacesToCamelCase()}")
                        {
                            Checked = door.IsOpen,
                            ItemData = new MyDoor() { Type = 1, index = door.Index }
                        });

                        if (door.Index == VehicleDoorIndex.BackLeftDoor || door.Index == VehicleDoorIndex.BackRightDoor)
                        {
                            addBothBackDoors = true;
                        }
                    }
                });

                if (addBothBackDoors)
                {
                    doorsMenu.AddMenuItem(new MenuCheckboxItem($"Open Both Back Doors")
                    {
                        Checked = carBackDoorsBool,
                        ItemData = new MyDoor() { Type = 3, index = VehicleDoorIndex.BackRightDoor }
                    });
                }

                if (Client.CurrentVehicle != null)
                {
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
                }

                doorsMenu.OnCheckboxChange += (Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState) =>
                {
                    VehicleDoor door = null;

                    if (menuItem.ItemData.Type == 1)
                        door = Client.CurrentVehicle.Doors[menuItem.ItemData.index];

                    if (menuItem.ItemData.Type == 2)
                        door = attachedVehicle.Doors[menuItem.ItemData.index];

                    if (menuItem.ItemData.Type == 3)
                    {
                        OnToggleBackDoors();
                    }
                    else
                    {
                        if (menuItem.Checked) door.Open(); else door.Close((menuItem.ItemData.Type == 2));
                    }
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
            }

            AddSubMenu(mainVehicleMenu, doorsMenu, Client.CurrentVehicle != null);
        }

        private static void SetupWindowsMenu()
        {
            Menu windowMenu = new Menu("Windows");

            if (Client.CurrentVehicle != null)
            {
                windowMenu.OnMenuOpen += (_menu) =>
                {
                    MenuBase.MenuOpen(true);
                    if (windowStates != null)
                        windowStates.Clear();

                    windowStates = VehicleWindowValues.ToDictionary(v => v, v => false);

                    VehicleWindowValues.Select((window, index) => new { window, index }).ToList().ForEach(o =>
                    {
                        var window = Client.CurrentVehicle.Windows[o.window];
                        windowMenu.AddMenuItem(new MenuCheckboxItem($"Roll Down {window.Index.ToString().AddSpacesToCamelCase()}")
                        {
                            Checked = windowStates[window.Index],
                            ItemData = window.Index
                        });
                    });
                };

                windowMenu.OnCheckboxChange += (Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState) =>
                {
                    VehicleWindow window = Client.CurrentVehicle.Windows[menuItem.ItemData];
                    if (menuItem.Checked) window.RollDown(); else window.RollUp();
                    windowStates[(VehicleWindowIndex)menuItem.Index] = menuItem.Checked;
                };

                windowMenu.OnMenuClose += (_menu) =>
                {
                    MenuBase.MenuOpen(false);
                    _menu.ClearMenuItems();
                };
            }

            AddSubMenu(mainVehicleMenu, windowMenu, Client.CurrentVehicle != null);
        }

        private static void Menu_OnCheckboxChange(Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState)
        {
            if (menuItem.ItemData == CRUISE_CONTROL)
                Client.TriggerEvent("curiosity:Player:Vehicle:CruiseControl");
            if (menuItem.ItemData == ENGINE)
                Client.CurrentVehicle.IsEngineRunning = menuItem.Checked;

            if (menuItem.ItemData == VTOL)
            {
                int vehicleId = Game.PlayerPed.CurrentVehicle.Handle;
                API.SetDesiredVerticalFlightPhase(vehicleId, inVTOL ? 0.0f : 1.0f);
            }
                
        }

        public static void AddSubMenu(Menu menu, Menu submenu, bool enabled = true)
        {
            MenuController.AddSubmenu(menu, submenu);
            MenuItem submenuButton = new MenuItem(submenu.MenuTitle, submenu.MenuSubtitle) { Label = "→→→", Enabled = enabled };
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
