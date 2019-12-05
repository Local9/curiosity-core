using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Shared.Client.net.Extensions;
using MenuAPI;
using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions;

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
        static MenuItem mItemHandcuffs = new MenuItem("Apply Handcuffs");
        static MenuItem mItemCurrentVehicle = new MenuItem("Detain in Vehicle");

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
                Cpr.Init();
                await Client.Delay(1000);
                Cpr.InteractionCPR(_interactivePed);
                MainMenu.CloseMenu();
            }
            if (menuItem == mItemCallCoroner)
            {
                Extras.Coroner.RequestService();
                MainMenu.CloseMenu();
            }
            // ALIVE
            if (menuItem == mItemHello)
                Social.Hello(_interactivePed);

            if (menuItem == mItemHandcuffs)
            {
                ArrestInteractions.InteractionHandcuff(_interactivePed);
                mItemHandcuffs.Enabled = false;
                await Client.Delay(4000);
                mItemHandcuffs.Text = _interactivePed.IsHandcuffed ? "Remove Handcuffs" : "Apply Handcuffs";
                mItemHandcuffs.Enabled = true;
                mItemCurrentVehicle.Enabled = _interactivePed.IsHandcuffed;
            }

            if (menuItem == mItemCurrentVehicle)
            {
                ArrestInteractions.InteractionPutInVehicle(_interactivePed);
                mItemCurrentVehicle.Enabled = false;
                await Client.Delay(4000);
                mItemCurrentVehicle.Enabled = _interactivePed.IsHandcuffed;
                mItemCurrentVehicle.Text = _interactivePed.Ped.IsInVehicle() ? "Remove from Vehicle" : "Detain in Vehicle";
            }
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

                mItemHandcuffs.Text = _interactivePed.IsHandcuffed ? "Remove Handcuffs" : "Apply Handcuffs";
                mItemHandcuffs.Enabled = !_interactivePed.Ped.IsInVehicle();
                MainMenu.AddMenuItem(mItemHandcuffs);

                mItemCurrentVehicle.Enabled = _interactivePed.IsHandcuffed;
                mItemCurrentVehicle.Text = _interactivePed.Ped.IsInVehicle() ? "Remove from Vehicle" : "Detain in Vehicle";
                MainMenu.AddMenuItem(mItemCurrentVehicle);

                if (_interactivePed.IsTrafficStop)
                {
                    // MURDER CHECKS
                }
            }
        }

        private static void MainMenu_OnMenuClose(Menu menu)
        {
            client.DeregisterTickHandler(OnDistanceTask);
            MenuController.DontOpenAnyMenu = true;
        }
    }
}
