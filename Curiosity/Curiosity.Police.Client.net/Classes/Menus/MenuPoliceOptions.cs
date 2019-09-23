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

        static public void Init()
        {

        }

        static public void OpenMenu()
        {
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

            MainMenu.ClearMenuItems();
            MainMenu.OpenMenu();
        }

        private static void OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {

        }

        private static void OnMenuClose(Menu menu)
        {
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
        }
    }
}
