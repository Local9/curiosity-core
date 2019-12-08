using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Shared.Client.net.Extensions;
using MenuAPI;
using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions;
using Curiosity.Shared.Client.net;

namespace Curiosity.Missions.Client.net.Scripts.Menus.PedInteractionMenu.SubMenus
{
    class MenuDispatch
    {
        private const string MENU_TITLE = "Dispatch";
        static Menu menu;
        static InteractivePed _interactivePed;

        static public void SetupMenu(InteractivePed interactivePed)
        {
            if (menu == null)
            {
                menu = new Menu(MENU_TITLE, MENU_TITLE);
                menu.OnMenuClose += Menu_OnMenuClose;
                menu.OnMenuOpen += Menu_OnMenuOpen;
                menu.OnItemSelect += Menu_OnItemSelect;
                menu.OnListIndexChange += Menu_OnListIndexChange;
                menu.EnableInstructionalButtons = true;
            }
            menu.MenuTitle = MENU_TITLE;
            _interactivePed = interactivePed;
            MenuBase.AddSubMenu(MenuBase.MainMenu, menu);
        }

        private static void Menu_OnListIndexChange(Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex)
        {
            throw new NotImplementedException();
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            throw new NotImplementedException();
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            menu.MenuTitle = "";
            menu.MenuSubtitle = MENU_TITLE;
            MenuBase.MenuState(true);
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuController.MainMenu.OpenMenu();
        }
    }
}
