using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Shared.Client.net.GameData;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net;

namespace Curiosity.Police.Client.net.Classes.Menus
{
    class MenuPoliceOptions
    {
        static Client client = Client.GetInstance();
        static Menu MainMenu;

        static PatrolZone _patrolZone = PatrolZone.City;
        static bool _IsOnDuty = false;

        static List<string> patrolAreas = new List<string>();

        static MenuCheckboxItem menuDuty = new MenuCheckboxItem("On Duty", _IsOnDuty);
        static MenuListItem menuListPatrolZone = new MenuListItem("Patrol Zone", patrolAreas, (int)_patrolZone);

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Police:ShowOptions", new Action(OpenMenu));

            patrolAreas.Add($"{PatrolZone.City}");
            patrolAreas.Add($"{PatrolZone.Country}");
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

                MenuController.AddMenu(MainMenu);
                MenuController.EnableMenuToggleKeyOnController = false;
            }

            // MainMenu.ClearMenuItems();
            MainMenu.OpenMenu();
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

            MainMenu.AddMenuItem(menuDuty);
            MainMenu.AddMenuItem(menuListPatrolZone);
        }
    }
}
