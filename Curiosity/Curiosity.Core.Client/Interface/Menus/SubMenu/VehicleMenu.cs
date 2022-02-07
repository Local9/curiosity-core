using CitizenFX.Core;
using Curiosity.Core.Client.Interface.Menus.VehicleMods;
using NativeUI;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class VehicleMenu
    {
        private UIMenu menuVehicleDoors;
        private VehicleDoorMenu _VehicleDoorMenu = new VehicleDoorMenu();
        private UIMenu menuVehicleWindows;
        private VehicleWindowMenu _VehicleWindowMenu = new VehicleWindowMenu();

        private UIMenu menuVehicleRemote;
        private VehicleRemoteMenu _VehicleRemoteMenu = new VehicleRemoteMenu();

        static List<dynamic> lockList = new List<dynamic>() { "Allow Everyone", "Lock for Everyone", "Passengers Only" };
        UIMenuListItem uiVehicleLock = new UIMenuListItem("Lock", lockList, 2);
        UIMenuItem uiOpenModMenu = new UIMenuItem("Modify Vehicle");

        UIMenuCheckboxItem uiChkDriftTires;
        bool driftTiresEnabled = false;

        List<int> driftVehicleHashes = new List<int>();

        List<string> driftVehicles = new List<string>() {
            "growler",
            "vectre",
            "dominator7",
            "comet6",
            "remus",
            "jester4",
            "tailgater2",
            "warrener2",
            "rt3000",
            "zr350",
            "dominator8",
            "euros",
            "futo2",
            "calico",
            "sultan3",
            "cypher",
            "previon"
        };

        public UIMenu CreateMenu(UIMenu menu)
        {
            foreach (string hash in driftVehicles)
            {
                int sultanRS = GetHashKey(hash);
                driftVehicleHashes.Add(sultanRS);
            }

            menuVehicleDoors = InteractionMenu.MenuPool.AddSubMenu(menu, "Doors");
            _VehicleDoorMenu.CreateMenu(menuVehicleDoors);

            menuVehicleWindows = InteractionMenu.MenuPool.AddSubMenu(menu, "Windows");
            _VehicleWindowMenu.CreateMenu(menuVehicleWindows);

            menuVehicleRemote = InteractionMenu.MenuPool.AddSubMenu(menu, "Vehicle Remote Functions");
            _VehicleRemoteMenu.CreateMenu(menuVehicleRemote);

            uiChkDriftTires = new UIMenuCheckboxItem("Enable Drift Tires", driftTiresEnabled);

            menu.AddItem(uiChkDriftTires);
            menu.AddItem(uiVehicleLock);
            menu.AddItem(uiOpenModMenu);

            menu.OnListChange += Menu_OnListChange;
            menu.OnCheckboxChange += Menu_OnCheckboxChange;
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == uiOpenModMenu)
                {
                    InteractionMenu.MenuPool.CloseAllMenus();
                    VehicleModMenu.GetModule().OpenMenu();
                }
            };

            menu.MouseControlsEnabled = false;
            menu.MouseEdgeEnabled = false;

            return menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (Equals(MenuState.Opened, state) || Equals(MenuState.ChangeForward, state))
            {
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

                if (IsDriftSupported())
                {
                    driftTiresEnabled = GetDriftTyresEnabled(vehicle.Handle);
                    uiChkDriftTires.Checked = driftTiresEnabled;
                    uiChkDriftTires.Enabled = true;
                }
                else
                {
                    uiChkDriftTires.Checked = false;
                    uiChkDriftTires.Enabled = false;
                }
            }
        }

        private bool IsDriftSupported()
        {
            if (!Game.PlayerPed.IsInVehicle()) return false;

            int vehicleHash = Game.PlayerPed.CurrentVehicle.Model.Hash;
            return driftVehicleHashes.Contains(vehicleHash);
        }

        private void Menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

            if (checkboxItem == uiChkDriftTires)
            {
                SetDriftTyresEnabled(vehicle.Handle, Checked);
                SetReduceDriftVehicleSuspension(vehicle.Handle, Checked);
            }
        }

        private async void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == uiVehicleLock)
            {
                string type = lockList[newIndex];

                if (type == "Allow Everyone")
                    UnlockAllVehicles();

                if (type == "Lock for Everyone")
                    LockAllVehicles();

                if (type == "Passengers Only")
                    PassengersOnly();

                await BaseScript.Delay(150);
            }
        }

        private void UnlockAllVehicles()
        {
            if (Cache.PersonalVehicle is not null && Cache.PersonalVehicle.Vehicle.Exists())
            {
                Cache.PersonalVehicle.ToggleLock(false);
                SetVehicleExclusiveDriver_2(Cache.PersonalVehicle.Vehicle.Handle, 0, 0);
            }

            if (Cache.PersonalPlane is not null && Cache.PersonalPlane.Vehicle.Exists())
            {
                Cache.PersonalPlane.ToggleLock(false);
                SetVehicleExclusiveDriver_2(Cache.PersonalPlane.Vehicle.Handle, 0, 0);
            }

            if (Cache.PersonalHelicopter is not null && Cache.PersonalHelicopter.Vehicle.Exists())
            {
                Cache.PersonalHelicopter.ToggleLock(false);
                SetVehicleExclusiveDriver_2(Cache.PersonalHelicopter.Vehicle.Handle, 0, 0);
            }

            if (Cache.PersonalBoat is not null && Cache.PersonalBoat.Vehicle.Exists())
            {
                Cache.PersonalBoat.ToggleLock(false);
                SetVehicleExclusiveDriver_2(Cache.PersonalBoat.Vehicle.Handle, 0, 0);
            }
        }

        private void LockAllVehicles()
        {
            if (Cache.PersonalVehicle is not null && Cache.PersonalVehicle.Vehicle.Exists())
                Cache.PersonalVehicle.ToggleLock(true);

            if (Cache.PersonalPlane is not null && Cache.PersonalPlane.Vehicle.Exists())
                Cache.PersonalPlane.ToggleLock(true);

            if (Cache.PersonalHelicopter is not null && Cache.PersonalHelicopter.Vehicle.Exists())
                Cache.PersonalHelicopter.ToggleLock(true);

            if (Cache.PersonalBoat is not null && Cache.PersonalBoat.Vehicle.Exists())
                Cache.PersonalBoat.ToggleLock(true);
        }

        private void PassengersOnly()
        {
            if (Cache.PersonalVehicle is not null && Cache.PersonalVehicle.Vehicle.Exists())
            {
                Cache.PersonalVehicle.ToggleLock(false);
                SetVehicleExclusiveDriver_2(Cache.PersonalVehicle.Vehicle.Handle, Game.PlayerPed.Handle, 0);
            }

            if (Cache.PersonalPlane is not null && Cache.PersonalPlane.Vehicle.Exists())
            {
                Cache.PersonalPlane.ToggleLock(false);
                SetVehicleExclusiveDriver_2(Cache.PersonalPlane.Vehicle.Handle, Game.PlayerPed.Handle, 0);
            }

            if (Cache.PersonalHelicopter is not null && Cache.PersonalHelicopter.Vehicle.Exists())
            {
                Cache.PersonalHelicopter.ToggleLock(false);
                SetVehicleExclusiveDriver_2(Cache.PersonalHelicopter.Vehicle.Handle, Game.PlayerPed.Handle, 0);
            }

            if (Cache.PersonalBoat is not null && Cache.PersonalBoat.Vehicle.Exists())
            {
                Cache.PersonalBoat.ToggleLock(false);
                SetVehicleExclusiveDriver_2(Cache.PersonalBoat.Vehicle.Handle, Game.PlayerPed.Handle, 0);
            }
        }
    }
}
