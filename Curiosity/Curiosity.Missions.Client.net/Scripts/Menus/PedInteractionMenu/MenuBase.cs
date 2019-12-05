using Curiosity.Missions.Client.net.MissionPeds;
using MenuAPI;
using System;

namespace Curiosity.Missions.Client.net.Scripts.Menus.PedInteractionMenu
{
    class MenuBase
    {
        static Client client = Client.GetInstance();
        static public Menu MainMenu;
        static InteractivePed _interactivePed;

        // Options

        // buttons
        static MenuItem mItemHello = new MenuItem("Hello");

        // Dead Interactions
        static MenuItem mItemCpr = new MenuItem("CPR");

        static public void Open(InteractivePed interactivePed)
        {
            MenuController.DontOpenAnyMenu = false;
            _interactivePed = interactivePed;

            if (MainMenu == null)
            {
                MainMenu = new Menu("", "Interaction Menu");

                // menu actions
                MainMenu.OnMenuClose += MainMenu_OnMenuClose;
                MainMenu.OnMenuOpen += MainMenu_OnMenuOpen;
                MainMenu.OnItemSelect += MainMenu_OnItemSelect;
                MainMenu.OnListIndexChange += MainMenu_OnListIndexChange;

                // menu config
                MainMenu.EnableInstructionalButtons = true;

                MenuController.AddMenu(MainMenu);
            }
            MainMenu.OpenMenu();
        }

        private static void MainMenu_OnListIndexChange(Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex)
        {
            throw new NotImplementedException();
        }

        private async static void MainMenu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            // DEAD
            if (menuItem == mItemCpr)
            {
                Scripts.PedInteractions.Cpr.Init();

                await Client.Delay(1000);

                _interactivePed.IsPerformingCpr = true;
                PedInteractions.Cpr.InteractionCPR(_interactivePed);
                MainMenu.CloseMenu();
            }
            // ALIVE
            if (menuItem == mItemHello)
                PedInteractions.Social.Hello(_interactivePed);
        }

        private static void MainMenu_OnMenuOpen(Menu menu)
        {
            MainMenu.ClearMenuItems();

            if (_interactivePed.IsDead)
            {
                // CPR / Coroner
                MainMenu.AddMenuItem(mItemCpr);
            }
            else
            {
                MainMenu.AddMenuItem(mItemHello);

                if (_interactivePed.IsTrafficStop)
                {

                }
            }
        }

        private static void MainMenu_OnMenuClose(Menu menu)
        {
            _interactivePed.IsPerformingCpr = true;
        }
    }
}
