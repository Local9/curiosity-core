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

        // static List<dynamic> lockList = new List<dynamic>() { "Allow Everyone", "Lock for Everyone", "Passengers Only" };
        static List<dynamic> lockList = new List<dynamic>() { "Allow Everyone", "Lock for Everyone" };
        UIMenuListItem uiVehicleLock = new UIMenuListItem("Lock", lockList, 0);
        UIMenuItem uiOpenModMenu = new UIMenuItem("Modify Vehicle");

        UIMenuCheckboxItem uiChkDriftTires;
        bool driftTiresEnabled = false;

        List<int> driftVehicleHashes = new List<int>();

        public UIMenu CreateMenu(UIMenu menu)
        {
            int growler = GetHashKey("growler");
            driftVehicleHashes.Add(growler);
            int vectre = GetHashKey("vectre");
            driftVehicleHashes.Add(vectre);
            int dominatorASP = GetHashKey("dominator7");
            driftVehicleHashes.Add(dominatorASP);
            int comet = GetHashKey("comet6");
            driftVehicleHashes.Add(comet);
            int remus = GetHashKey("remus");
            driftVehicleHashes.Add(remus);
            int jester = GetHashKey("jester4");
            driftVehicleHashes.Add(jester);
            int tailgater = GetHashKey("tailgater2");
            driftVehicleHashes.Add(tailgater);
            int warrener = GetHashKey("warrener2");
            driftVehicleHashes.Add(warrener);
            int rt3000 = GetHashKey("rt3000");
            driftVehicleHashes.Add(rt3000);
            int zr350 = GetHashKey("zr350");
            driftVehicleHashes.Add(zr350);
            int dominatorGTT = GetHashKey("dominator8");
            driftVehicleHashes.Add(dominatorGTT);
            int euros = GetHashKey("euros");
            driftVehicleHashes.Add(euros);
            int futo2 = GetHashKey("futo2");
            driftVehicleHashes.Add(futo2);
            int calicoGTF = GetHashKey("calico");
            driftVehicleHashes.Add(calicoGTF);

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

                await BaseScript.Delay(150);
            }
        }

        private void UnlockAllVehicles()
        {
            if (Cache.PersonalVehicle is not null && Cache.PersonalVehicle.Vehicle.Exists())
            {
                Cache.PersonalVehicle.ToggleLock(false);
            }

            if (Cache.PersonalPlane is not null && Cache.PersonalPlane.Vehicle.Exists())
            {
                Cache.PersonalPlane.ToggleLock(false);
            }

            if (Cache.PersonalHelicopter is not null && Cache.PersonalHelicopter.Vehicle.Exists())
            {
                Cache.PersonalHelicopter.ToggleLock(false);
            }

            if (Cache.PersonalBoat is not null && Cache.PersonalBoat.Vehicle.Exists())
            {
                Cache.PersonalBoat.ToggleLock(false);
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
            }

            if (Cache.PersonalPlane is not null && Cache.PersonalPlane.Vehicle.Exists())
            {
                Cache.PersonalPlane.ToggleLock(false);
            }

            if (Cache.PersonalHelicopter is not null && Cache.PersonalHelicopter.Vehicle.Exists())
            {
                Cache.PersonalHelicopter.ToggleLock(false);
            }

            if (Cache.PersonalBoat is not null && Cache.PersonalBoat.Vehicle.Exists())
            {
                Cache.PersonalBoat.ToggleLock(false);
            }
        }
    }
}
