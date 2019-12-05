using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Shared.Client.net.Extensions;
using MenuAPI;
using System;
using System.Threading.Tasks;
using CitizenFX.Core;

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
        static MenuItem mItemCallCoroner = new MenuItem("Call Coroner");

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
            client.RegisterTickHandler(OnDistanceTask);
            MainMenu.OpenMenu();
        }

        static async Task OnDistanceTask()
        {
            await Task.FromResult(0);

            if (_interactivePed.Position.Distance(Game.PlayerPed.Position) > 4)
                MainMenu.CloseMenu();
        }

        private static void MainMenu_OnListIndexChange(Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex)
        {
            
        }

        private async static void MainMenu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            // DEAD
            if (menuItem == mItemCpr)
            {
                Scripts.PedInteractions.Cpr.Init();
                await Client.Delay(1000);
                PedInteractions.Cpr.InteractionCPR(_interactivePed);
                MainMenu.CloseMenu();
            }
            if (menuItem == mItemCallCoroner)
            {
                Extras.Coroner.RequestService();
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
                if (!_interactivePed.HasCprFailed)
                    MainMenu.AddMenuItem(mItemCpr);

                MainMenu.AddMenuItem(mItemCallCoroner);
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
            client.DeregisterTickHandler(OnDistanceTask);
        }
    }
}
