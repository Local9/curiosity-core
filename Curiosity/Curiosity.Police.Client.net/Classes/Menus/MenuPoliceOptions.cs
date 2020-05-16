using CitizenFX.Core;
using CitizenFX.Core.UI;
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
        static bool _IsTrafficStopsActive = false;
        static bool _IsRandomEventsActive = false;
        static bool _IsBackupActive = false;

        static string _ActiveJob;

        static List<string> patrolAreas = new List<string>();

        static MenuListItem menuListPatrolZone = new MenuListItem("Area of Patrol", patrolAreas, _patrolZone);
        static MenuItem menuItemShowRadar = new MenuItem("Open RS9000");
        static MenuItem menuItemDispatch = new MenuItem("Dispatch");
        static MenuItem menuItemBreaker = new MenuItem(":: Options ::") { Enabled = false };

        static MenuCheckboxItem menuCheckboxBackup = new MenuCheckboxItem("Receive Back Up Calls", _IsBackupActive) { Description = "Show information when a player requests backup" };
        static MenuCheckboxItem menuCheckboxTrafficStops = new MenuCheckboxItem("Enable Traffic Stops", _IsTrafficStopsActive);
        static MenuCheckboxItem menuCheckboxRandomCallouts = new MenuCheckboxItem("Accept Dispatch Calls", _IsOnDuty) { Description = "Random Dispatch Mission" };

        static MenuCheckboxItem menuCheckboxRandomEvents = new MenuCheckboxItem("Enable Random Events", _IsRandomEventsActive) { Description = "Still in development, may be buggy" };

        // Request


        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Police:ShowOptions", new Action(OpenMenu));
            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(OnDutyState));

            client.RegisterEventHandler("curiosity:Client:Police:TrafficStops", new Action<bool>(OnTrafficStopState));

            patrolAreas.Add($"{PatrolZone.City}");
            patrolAreas.Add($"{PatrolZone.Country}");
        }

        static void OnDutyState(bool active, bool onduty, string job)
        {
            _IsActive = active;
            _ActiveJob = job;

            if (_IsOnDuty != onduty)
                _IsOnDuty = onduty;
        }

        static void OnTrafficStopState(bool state)
        {
            _IsTrafficStopsActive = state;
            menuCheckboxTrafficStops.Checked = state;
        }

        static public void OpenMenu()
        {
            MenuBaseFunctions.MenuOpen();

            MenuController.DontOpenAnyMenu = false;
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

            MainMenu.OpenMenu();
        }

        private static async void OnCheckboxChange(Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState)
        {
            MenuBaseFunctions.MenuOpen();

            if (menuItem == menuCheckboxRandomCallouts)
            {
                menuItem.Enabled = false;

                _IsOnDuty = newCheckedState;
                BaseScript.TriggerEvent("curiosity:Client:Interface:Duty", _IsActive, _IsOnDuty, _ActiveJob);

                await Client.Delay(3000);
                menuItem.Enabled = true;
            }

            if (menuItem == menuCheckboxTrafficStops)
            {
                _IsTrafficStopsActive = newCheckedState;
                BaseScript.TriggerEvent("curiosity:Client:Mission:TrafficStops", _IsTrafficStopsActive);
            }

            if (menuItem == menuCheckboxRandomEvents)
            {
                _IsRandomEventsActive = newCheckedState;
                BaseScript.TriggerEvent("curiosity:Client:Mission:RandomEvents", _IsRandomEventsActive);
            }

            if (menuItem == menuCheckboxBackup)
            {
                _IsBackupActive = newCheckedState;
                Environment.Job.BackupMessages.IsAcceptingBackupCalls = _IsBackupActive;
                if (_IsBackupActive)
                {
                    Screen.ShowNotification("~g~Accepting Backup Calls");
                }
                else
                {
                    Screen.ShowNotification("~r~No longer accepting Backup Calls");
                }
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
            if (menuItem == menuItemShowRadar)
            {
                if (menuItem.Enabled)
                {
                    Client.TriggerEvent("rs9000:ToggleRadar");

                    MainMenu.CloseMenu();
                }
            }

            if (menuItem == menuItemDispatch)
            {
                MainMenu.CloseMenu();

                PoliceDispatchMenu.OpenMenu();
            }
        }

        private static void OnMenuClose(Menu menu)
        {
            MenuController.DontOpenAnyMenu = true;
            MenuBaseFunctions.MenuClose();
        }

        private static void OnListItemSelect(Menu menu, MenuListItem listItem, int selectedIndex, int itemIndex)
        {

        }

        private static async void OnMenuOpen(Menu menu)
        {
            MainMenu.ClearMenuItems();

            MenuBaseFunctions.MenuOpen();

            menuCheckboxRandomCallouts.Enabled = false;
            menuListPatrolZone.Enabled = false;
            menuCheckboxTrafficStops.Enabled = false;
            menuItemShowRadar.Enabled = false;

            menu.AddMenuItem(menuListPatrolZone);

            bool canDoCallouts = false;
            int policexp = 0;
            int knowledge = 0;

            if (Player.PlayerInformation.playerInfo.Skills.ContainsKey("policexp"))
            {
                policexp = Player.PlayerInformation.playerInfo.Skills["policexp"].Value;

                if (Player.PlayerInformation.playerInfo.Skills.ContainsKey("knowledge"))
                {
                    knowledge = Player.PlayerInformation.playerInfo.Skills["knowledge"].Value;
                    canDoCallouts = (policexp >= 1000 && knowledge >= 500);
                }
            }

            menuCheckboxRandomCallouts.Enabled = canDoCallouts;

            if (!menuCheckboxRandomCallouts.Enabled)
            {
                string description = "~b~Require Additional;";
                if (policexp < 1000)
                    description += $"~n~- ~y~{1000 - policexp:#,##0} ~s~Police Experience";
                if (knowledge < 500)
                    description += $"~n~- ~y~{500 - knowledge:#,##0} ~s~Knowledge";

                menuCheckboxRandomCallouts.Description = description;
            }
            else
            {
                menuCheckboxRandomCallouts.Description = "Random Callouts";
            }

            menu.AddMenuItem(menuItemShowRadar);
            menu.AddMenuItem(menuItemDispatch);
            menu.AddMenuItem(menuItemBreaker);
            menu.AddMenuItem(menuCheckboxRandomCallouts);
            menu.AddMenuItem(menuCheckboxBackup);
            menu.AddMenuItem(menuCheckboxTrafficStops);
            // menu.AddMenuItem(menuCheckboxRandomEvents);

            await Client.Delay(100);

            menuListPatrolZone.Enabled = true;
            menuCheckboxTrafficStops.Enabled = true;
            menuItemShowRadar.Enabled = true;

            MenuBaseFunctions.MenuOpen();
        }
        public static Vehicle GetVehicleDriving(Ped ped)
        {
            Vehicle v = ped.CurrentVehicle;
            bool driving = ped.SeatIndex == VehicleSeat.Driver;

            if (v == null || !driving || v.ClassType != VehicleClass.Emergency)
            {
                return null;
            }

            return v;
        }
    }
}
