using CitizenFX.Core;
using Curiosity.Shared.Client.net.Enums.Patrol;
using MenuAPI;
using System;
using System.Collections.Generic;

namespace Curiosity.Police.Client.net.Classes.Menus
{
    class MenuPoliceOptions
    {
        static Client client = Client.GetInstance();
        static Menu MainMenu;

        static int _patrolZone = 0;

        static bool _IsActive = false;
        static bool _IsOnDuty = false;
        static string _ActiveJob;

        static List<string> patrolAreas = new List<string>();

        static MenuCheckboxItem menuDuty = new MenuCheckboxItem("On Active Duty", _IsOnDuty);
        static MenuListItem menuListPatrolZone = new MenuListItem("Set Patrol Zone", patrolAreas, _patrolZone);

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Police:ShowOptions", new Action(OpenMenu));

            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(OnDutyState));

            patrolAreas.Add($"{PatrolZone.City}");
            patrolAreas.Add($"{PatrolZone.Country}");
        }

        static void OnDutyState(bool active, bool onduty, string job)
        {
            _IsActive = active;
            _IsOnDuty = onduty;
            _ActiveJob = job;
        }

        static public void OpenMenu()
        {
            MenuController.DontOpenAnyMenu = false;
            MenuBaseFunctions.MenuOpen();
            MenuController.EnableMenuToggleKeyOnController = false;

            if (MainMenu == null)
            {
                MainMenu = new Menu("Police Options", "Additional options for Police");
                
                MainMenu.OnMenuOpen += OnMenuOpen;
                MainMenu.OnListItemSelect += OnListItemSelect;
                MainMenu.OnItemSelect += OnItemSelect;
                MainMenu.OnMenuClose += OnMenuClose;
                MainMenu.OnListIndexChange += OnListIndexChange;
                MainMenu.OnCheckboxChange += OnCheckboxChange;

                MenuController.AddMenu(MainMenu);
                MenuController.EnableMenuToggleKeyOnController = false;
            }

            // MainMenu.ClearMenuItems();
            MainMenu.OpenMenu();
        }

        private static async void OnCheckboxChange(Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState)
        {
            MenuBaseFunctions.MenuOpen();

            if (menuItem == menuDuty)
            {
                menuItem.Enabled = false;

                _IsOnDuty = newCheckedState;
                BaseScript.TriggerEvent("curiosity:Client:Interface:Duty", _IsActive, _IsOnDuty, _ActiveJob);

                await Client.Delay(3000);
                menuItem.Enabled = true;
            }
        }

        private static async void OnListIndexChange(Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex)
        {
            MenuBaseFunctions.MenuOpen();

            if (listItem == menuListPatrolZone)
            {
                listItem.Enabled = false;
                _patrolZone = newSelectionIndex;

                string selectedItem = patrolAreas[_patrolZone];
                Enum.TryParse(selectedItem, out PatrolZone patrolZone);

                Client.TriggerEvent("curiosity:Client:Police:PatrolZone", (int)patrolZone);
                await Client.Delay(3000);
                listItem.Enabled = true;
            }
        }

        private static void OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {

        }

        private static void OnMenuClose(Menu menu)
        {
            MenuController.DontOpenAnyMenu = true;
            MenuBaseFunctions.MenuClose();
            MainMenu.ClearMenuItems();
            MainMenu = null;
        }

        private static void OnListItemSelect(Menu menu, MenuListItem listItem, int selectedIndex, int itemIndex)
        {
            
        }

        private static void OnMenuOpen(Menu menu)
        {
            MainMenu.ClearMenuItems();

            MenuBaseFunctions.MenuOpen();

            MainMenu.AddMenuItem(menuDuty);
            MainMenu.AddMenuItem(menuListPatrolZone);
        }
    }
}
