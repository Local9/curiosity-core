using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Enums.Patrol;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

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
        static bool _IsArrestsActive = false;
        static bool _IsRandomEventsActive = false;
        static bool _IsBackupActive = false;

        static string _ActiveJob;

        static List<string> patrolAreas = new List<string>();

        static MenuListItem menuListPatrolZone = new MenuListItem("Area of Patrol", patrolAreas, _patrolZone);
        static MenuItem menuItemShowRadar = new MenuItem("Open RS9000");
        static MenuItem menuItemDispatch = new MenuItem("Dispatch");
        static MenuItem menuItemBreaker = new MenuItem(":: Options ::") { Enabled = false };

        static MenuCheckboxItem menuCheckboxDuty = new MenuCheckboxItem("Accepting Dispatch Calls", _IsOnDuty);
        static MenuCheckboxItem menuCheckboxBackup = new MenuCheckboxItem("Receive Back Up Calls", _IsBackupActive);
        static MenuCheckboxItem menuCheckboxTrafficStops = new MenuCheckboxItem("Enable Traffic Stops", _IsTrafficStopsActive);
        static MenuCheckboxItem menuCheckboxArrests = new MenuCheckboxItem("Enable Arrests", _IsArrestsActive);
        static MenuCheckboxItem menuCheckboxRandomEvents = new MenuCheckboxItem("Enable Random Events", _IsRandomEventsActive) { Enabled = false, Description = "Coming Soon™" };

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

            if (menuItem == menuCheckboxDuty)
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

            if (menuItem == menuCheckboxArrests)
            {
                _IsArrestsActive = newCheckedState;
                BaseScript.TriggerEvent("curiosity:Client:Mission:Arrests", _IsArrestsActive);
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

            menuCheckboxDuty.Enabled = false;
            menuListPatrolZone.Enabled = false;

            menu.AddMenuItem(menuListPatrolZone);

            bool canPullover = false;
            int policexp = 0;
            int knowledge = 0;

            if (Player.PlayerInformation.playerInfo.Skills.ContainsKey("policexp"))
            {
                policexp = Player.PlayerInformation.playerInfo.Skills["policexp"].Value;

                if (Player.PlayerInformation.playerInfo.Skills.ContainsKey("knowledge"))
                {
                    knowledge = Player.PlayerInformation.playerInfo.Skills["knowledge"].Value;
                    canPullover = (policexp >= 2500 && knowledge >= 1000);
                }
            }

            menuItemShowRadar.Enabled = canPullover;
            menuCheckboxTrafficStops.Enabled = canPullover;

            if (!menuItemShowRadar.Enabled)
            {
                string description = "~b~Require Additional;";
                if (policexp < 2500)
                    description += $"~n~- ~y~{2500 - policexp:#,##0} ~s~Police Experience";
                if (knowledge < 1000)
                    description += $"~n~- ~y~{1000 - knowledge:#,##0} ~s~Knowledge";

                menuItemShowRadar.Description = description;
                menuCheckboxTrafficStops.Description = description;
            }
            else
            {
                if (GetVehicleDriving(Game.PlayerPed) == null)
                {
                    menuItemShowRadar.Description = "Must be in an ~b~Emergency Vehicle~s~ to open.";
                }
                else
                {
                    menuItemShowRadar.Description = "Open Radar";
                }
            }

            menu.AddMenuItem(menuItemShowRadar);
            menu.AddMenuItem(menuItemDispatch);
            
            menu.AddMenuItem(menuItemBreaker);
            
            menu.AddMenuItem(menuCheckboxDuty);
            menu.AddMenuItem(menuCheckboxBackup);

            menu.AddMenuItem(menuCheckboxTrafficStops);
            menu.AddMenuItem(menuCheckboxArrests);

            if (Classes.Player.PlayerInformation.IsDeveloper())
            {
                menu.AddMenuItem(menuCheckboxRandomEvents);
            }

            await Client.Delay(100);

            menuCheckboxDuty.Enabled = true;
            menuListPatrolZone.Enabled = true;

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
