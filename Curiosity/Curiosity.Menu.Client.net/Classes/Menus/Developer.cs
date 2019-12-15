﻿using MenuAPI;
using System;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class Developer
    {
        static bool menuSetup = false;

        static bool _debugSafezones = false, _developerNpcUiEnable = false, __developerVehicleUiEnable = false;

        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Developer Menu", "Various Settings or options");

        // Vehicle Options
        static MenuItem mItemRepair = new MenuItem("Repair Vehicle");
        static MenuItem mItemRefuel = new MenuItem("Refuel Vehicle");
        // Debuging
        static MenuCheckboxItem menuCheckboxItemDebugAreas = new MenuCheckboxItem("Draw Safezones", _debugSafezones);
        static MenuCheckboxItem menuCheckboxItemDeveloperNpcUi = new MenuCheckboxItem("Show Interactive NPC UI", _developerNpcUiEnable);
        static MenuCheckboxItem menuCheckboxItemDeveloperVehUi = new MenuCheckboxItem("Show Interactive Veh UI", __developerVehicleUiEnable);

        public static void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
            SetupMenu();
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            SetupMenu();
        }

        static void OnPlayerSpawned(dynamic dynData)
        {
            SetupMenu();
        }

        static void SetupMenu()
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;

            if (menuSetup) return;

            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnCheckboxChange += Menu_OnCheckboxChange;

            menuSetup = true;

            MenuBase.AddSubMenu(menu);
        }

        private static void Menu_OnCheckboxChange(Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState)
        {
            if (menuItem == menuCheckboxItemDebugAreas)
            {
                _debugSafezones = newCheckedState;
                Client.TriggerEvent("curiosity:Client:Player:Environment:DrawAreas");
            }

            if (menuItem == menuCheckboxItemDeveloperNpcUi)
            {
                _developerNpcUiEnable = newCheckedState;
                Client.TriggerEvent("curiosity:Player:Mission:ShowDeveloperNpcUI", _developerNpcUiEnable);
            }

            if (menuItem == menuCheckboxItemDeveloperVehUi)
            {
                __developerVehicleUiEnable = newCheckedState;
                Client.TriggerEvent("curiosity:Player:Mission:ShowDeveloperVehUI", __developerVehicleUiEnable);
            }
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuBase.MenuOpen(false);
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            MenuBase.MenuOpen(true);
            menu.ClearMenuItems();

            bool enableVehicleOptions = Client.CurrentVehicle != null;

            mItemRefuel.Enabled = enableVehicleOptions;
            menu.AddMenuItem(mItemRefuel);
            mItemRepair.Enabled = enableVehicleOptions;
            menu.AddMenuItem(mItemRepair);

            menu.AddMenuItem(menuCheckboxItemDebugAreas);
            menu.AddMenuItem(menuCheckboxItemDeveloperNpcUi);
            menu.AddMenuItem(menuCheckboxItemDeveloperVehUi);
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemRefuel)
                Client.TriggerEvent("curiosity:Client:Vehicle:DevRefuel");

            if (menuItem == mItemRepair)
                Client.TriggerEvent("curiosity:Client:Vehicle:DevRepair");
        }
    }
}
