using CitizenFX.Core;
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
        static string _ActiveJob;

        static List<string> patrolAreas = new List<string>();

        static MenuCheckboxItem menuDuty = new MenuCheckboxItem("Accepting Dispatch Calls", _IsOnDuty);
        static MenuListItem menuListPatrolZone = new MenuListItem("Area of Patrol", patrolAreas, _patrolZone);
        static MenuItem menuItemShowRadar = new MenuItem("Open Radar");
        static MenuItem menuItemDispatch = new MenuItem("Dispatch");

        // Request


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
            _ActiveJob = job;

            if (_IsOnDuty != onduty)
                _IsOnDuty = onduty;
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

            menuDuty.Enabled = false;
            menuListPatrolZone.Enabled = false;

            menu.AddMenuItem(menuDuty);
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
                    canPullover = (policexp >= 4500 && knowledge >= 1000);
                }
            }

            menuItemShowRadar.Enabled = canPullover;
            
            if (!menuItemShowRadar.Enabled)
            {
                string description = "~b~Require Additional;";
                if (policexp < 2500)
                    description += $"~n~- ~y~{2500 - policexp:#,##0} ~s~Police Experience";
                if (knowledge < 1000)
                    description += $"~n~- ~y~{1000 - knowledge:#,##0} ~s~Knowledge";

                menuItemShowRadar.Description = description;
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

            await Client.Delay(100);

            menuDuty.Enabled = true;
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
