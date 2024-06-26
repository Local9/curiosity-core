﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Enums;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Extensions;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class VehicleMenu
    {
        static Client client = Client.GetInstance();
        static Menu MainVehicleMenu = new Menu("Vehicle", "Vehicle Settings and Options");

        // Vehicle States
        static VehicleLock VehicleLockState = VehicleLock.Unlocked;

        // Menu Items
        static MenuItem mItemDetinateVehicle = new MenuItem("Blow Up Vehicle");
        static MenuCheckboxItem mItemEngine = new MenuCheckboxItem("Engine") { Description = "Turn the engine on/off" };
        // Vehicle Locking
        static List<string> VehicleLockStateList = Enum.GetNames(typeof(VehicleLock)).Select(d => d.AddSpacesToCamelCase()).ToList();
        static MenuListItem mListItemVehicleLocks = new MenuListItem("Access Grant", VehicleLockStateList, (int)VehicleLockState, "Select to set vehicle access rights\n~r~Warning:~s~ Changing from a locked state to unlocked may cause your ped to break the window.") { Enabled = Client.CurrentVehicle != null };
        // HeadLights
        static List<string> HeadlightColors = new List<string>() { "White", "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Rose", "Sombre" };
        static MenuListItem mItemHeadLights = new MenuListItem("Headlights", HeadlightColors, _selectedHeadlight) { Description = "Colors are 50/50 on if they'll work" };
        static int _selectedHeadlight = 0;

        public static void Init()
        {
            MainVehicleMenu.OnMenuOpen += OnMainMenuOpen;
            MainVehicleMenu.OnMenuClose += OnMainMenuClose;
            MainVehicleMenu.OnListIndexChange += OnMainMenuListIndexChange;
            MainVehicleMenu.OnItemSelect += OnMainMenuOnItemSelect;
            MainVehicleMenu.OnCheckboxChange += OnMainMenuOnCheckboxChange;

            MenuBase.AddSubMenu(MainVehicleMenu, leftIcon: MenuItem.Icon.INV_CAR);
        }

        private static void OnMainMenuOpen(Menu menu)
        {
            MenuBase.MenuOpen(true);

            if (Player.PlayerInformation.privilege == Privilege.DEVELOPER)
            {
                MainVehicleMenu.AddMenuItem(mItemDetinateVehicle);
            }

            mItemEngine.Checked = Client.CurrentVehicle.IsEngineRunning;
            
            MainVehicleMenu.AddMenuItem(mListItemVehicleLocks);

            MainVehicleMenu.AddMenuItem(mItemEngine);
            MainVehicleMenu.AddMenuItem(mItemHeadLights);

            Submenus.VehicleSubMenu.VehicleWindows.SetupMenu();
            Submenus.VehicleSubMenu.VehicleDoors.SetupMenu();

            Submenus.VehicleSubMenu.VehicleLiveries.SetupMenu();
            Submenus.VehicleSubMenu.VehicleExtras.SetupMenu();
        }

        private static void OnMainMenuClose(Menu menu)
        {
            menu.ClearMenuItems();
            MenuBase.MenuOpen(false);
        }

        private static void OnMainMenuListIndexChange(Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex)
        {
            if (listItem == mListItemVehicleLocks)
            {
                OnToggleLockState(Client.CurrentVehicle.Handle, newSelectionIndex);
            }

            if (listItem == mItemHeadLights)
            {
                if (Client.CurrentVehicle == null) return;

                _selectedHeadlight = newSelectionIndex;
                Client.CurrentVehicle.Mods.InstallModKit();
                API.ToggleVehicleMod(Client.CurrentVehicle.Handle, 22, true);

                int headlightColor = 0;

                switch ($"{HeadlightColors[newSelectionIndex]}")
                {
                    case "White":
                        headlightColor = 0;
                        break;
                    case "Red":
                        headlightColor = 8;
                        break;
                    case "Blue":
                        headlightColor = 1;
                        break;
                    case "Green":
                        headlightColor = 4;
                        break;
                    case "Yellow":
                        headlightColor = 5;
                        break;
                    case "Purple":
                        headlightColor = 11;
                        break;
                    case "Orange":
                        headlightColor = 7;
                        break;
                    case "Rose":
                        headlightColor = 10;
                        break;
                    case "Sombre":
                        headlightColor = 12;
                        break;
                }

                if (Player.PlayerInformation.IsDeveloper())
                {
                    Debug.WriteLine($"OnMainMenuListIndexChange -> {HeadlightColors[newSelectionIndex]}, {headlightColor}");
                }

                API.SetVehicleHeadlightsColour(Client.CurrentVehicle.Handle, headlightColor);
            }
        }

        private static void OnMainMenuOnCheckboxChange(Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState)
        {
            if (menuItem == mItemEngine)
                Client.CurrentVehicle.IsEngineRunning = menuItem.Checked;
        }

        private static void OnMainMenuOnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemDetinateVehicle)
            {
                API.NetworkRegisterEntityAsNetworked(Client.CurrentVehicle.Handle);
                BaseScript.TriggerServerEvent("curiosity:Server:Vehicle:Detonate", API.NetworkGetNetworkIdFromEntity(Client.CurrentVehicle.Handle));

                if (Client.CurrentVehicle.IsDead)
                    Client.CurrentVehicle = null;
            }
        }

        static void OnToggleLockState(int vehicleId, int index)
        {
            if (Client.CurrentVehicle == null) return;

            if (vehicleId != Client.CurrentVehicle.Handle)
            {
                Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle Lock", "", "Sorry, you can only lock a car you own.", 2);
                return;
            }

            VehicleLock vehicleLock = (VehicleLock)index;

            switch(vehicleLock)
            {
                case VehicleLock.Locked:
                    API.PlayVehicleDoorCloseSound(Client.CurrentVehicle.Handle, 1);
                    Client.CurrentVehicle.LockStatus = VehicleLockStatus.Locked;
                    VehicleLockState = VehicleLock.Locked;
                    API.SetVehicleDoorsLockedForAllPlayers(Client.CurrentVehicle.Handle, true);
                    Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle Locked", "", "", 2);
                    break;
                case VehicleLock.NoPassengers:
                    API.PlayVehicleDoorCloseSound(Client.CurrentVehicle.Handle, 1);
                    API.SetVehicleDoorsLockedForAllPlayers(Client.CurrentVehicle.Handle, false);
                    API.SetVehicleAllowNoPassengersLockon(Client.CurrentVehicle.Handle, true);
                    VehicleLockState = VehicleLock.NoPassengers;
                    Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "No Passengers", "", "", 2);
                    break;
                default:
                    API.PlayVehicleDoorOpenSound(Client.CurrentVehicle.Handle, 0);
                    Client.CurrentVehicle.LockStatus = VehicleLockStatus.None;
                    VehicleLockState = VehicleLock.Unlocked;
                    API.SetVehicleAllowNoPassengersLockon(Client.CurrentVehicle.Handle, false);
                    API.SetVehicleDoorsLockedForAllPlayers(Client.CurrentVehicle.Handle, false);
                    Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle Unlocked", "", "", 2);
                    break;
            }
        }

        public static void AddSubMenu(Menu submenu, bool enabled = true)
        {
            MenuController.AddSubmenu(MainVehicleMenu, submenu);
            MenuItem submenuButton = new MenuItem(submenu.MenuTitle, submenu.MenuSubtitle) { Label = "→→→", Enabled = enabled };
            MainVehicleMenu.AddMenuItem(submenuButton);
            MenuController.BindMenuItem(MainVehicleMenu, submenu, submenuButton);
        }
    }
}
