﻿using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Interface.Menus.VehicleMods;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using NativeUI;

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
        public EventSystem EventSystem => EventSystem.GetModule();
        public NotificationManager Notify => NotificationManager.GetModule();

        static List<dynamic> lockList = new List<dynamic>() { "Allow Everyone", "Lock for Everyone", "Passengers Only" };
        UIMenuListItem uiVehicleLock = new UIMenuListItem("Lock", lockList, 0);
        UIMenuItem uiOpenModMenu = new UIMenuItem("Modify Vehicle");
        UIMenuItem uiPayOffSpeedingTickets = new UIMenuItem("Pay off all speeding tickets", "~s~This will pay all of your speeding tickets, if you have the money to do so.");
        UIMenuItem uiPayOffOutstandingSpeedingTickets = new UIMenuItem("Pay off outstanding speeding tickets", "~s~This will pay all of your ~b~Outstanding~s~ speeding tickets, if you have the money to do so.");

        UIMenuCheckboxItem uiChkInverseTorque;

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
            menuVehicleDoors.ParentItem.SetRightLabel(">>>");

            menuVehicleWindows = InteractionMenu.MenuPool.AddSubMenu(menu, "Windows");
            _VehicleWindowMenu.CreateMenu(menuVehicleWindows);
            menuVehicleWindows.ParentItem.SetRightLabel(">>>");

            menuVehicleRemote = InteractionMenu.MenuPool.AddSubMenu(menu, "Vehicle Remote Functions");
            _VehicleRemoteMenu.CreateMenu(menuVehicleRemote);
            menuVehicleRemote.ParentItem.SetRightLabel(">>>");

            uiChkDriftTires = new UIMenuCheckboxItem("Enable Drift Tires", driftTiresEnabled);
            uiChkInverseTorque = new UIMenuCheckboxItem("Enable Inverse Torque", VehicleManager.EnableInverseTorque);

            menu.AddItem(uiChkDriftTires);
            menu.AddItem(uiChkInverseTorque);
            menu.AddItem(uiVehicleLock);
            menu.AddItem(uiPayOffSpeedingTickets);
            menu.AddItem(uiPayOffOutstandingSpeedingTickets);
            menu.AddItem(uiOpenModMenu);

            menu.OnItemSelect += Menu_OnItemSelect;
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

        private async void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            selectedItem.Enabled = false;
            if (selectedItem == uiPayOffSpeedingTickets)
            {
                await EventSystem.Request<ExportMessage>("police:suspect:ticket:pay:all");
            }
            else if (selectedItem == uiPayOffOutstandingSpeedingTickets)
            {
                await EventSystem.Request<ExportMessage>("police:suspect:ticket:pay:overdue");
            }

            await BaseScript.Delay(5000);
            selectedItem.Enabled = true;
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
            else if (checkboxItem == uiChkInverseTorque)
            {
                VehicleManager.EnableInverseTorque = uiChkInverseTorque.Checked;
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
