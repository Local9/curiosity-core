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
    class MenuInteraction
    {
        private const string MENU_TITLE = "Interactions";
        static Menu menu;
        static InteractivePed _interactivePed;

        static MenuItem mItemHandcuffs = new MenuItem("Apply Handcuffs");
        static MenuItem mItemDetainInCurrentVehicle = new MenuItem("Detain in Vehicle");
        // Dead Interactions
        static MenuItem mItemCallCoroner = new MenuItem("Call Coroner");
        static MenuItem mItemCpr = new MenuItem("CPR");

        static public void SetupMenu(InteractivePed interactivePed)
        {
            if (menu == null)
            {
                menu = new Menu(MENU_TITLE, MENU_TITLE);
                menu.OnMenuClose += Menu_OnMenuClose;
                menu.OnMenuOpen += Menu_OnMenuOpen;
                menu.OnItemSelect += Menu_OnItemSelect;
                menu.EnableInstructionalButtons = true;
            }
            menu.MenuTitle = MENU_TITLE;
            _interactivePed = interactivePed;
            MenuBase.AddSubMenu(MenuBase.MainMenu, menu);
        }

        private async static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            // DEAD
            if (menuItem == mItemCpr)
            {
                Cpr.Init();
                await Client.Delay(1000);
                Cpr.InteractionCPR(_interactivePed);
                menu.CloseMenu();
            }
            if (menuItem == mItemCallCoroner)
            {
                Extras.Coroner.RequestService();
                menu.CloseMenu();
            }
            // ALIVE
            if (menuItem == mItemHandcuffs)
            {
                ArrestInteractions.InteractionHandcuff(_interactivePed);
                mItemHandcuffs.Enabled = false;
                await Client.Delay(4000);
                mItemHandcuffs.Text = _interactivePed.IsHandcuffed ? "Remove Handcuffs" : "Apply Handcuffs";
                mItemHandcuffs.Enabled = true;
                mItemDetainInCurrentVehicle.Enabled = CanDetainPed();
            }
            if (menuItem == mItemDetainInCurrentVehicle)
            {
                ArrestInteractions.InteractionPutInVehicle(_interactivePed);
                mItemDetainInCurrentVehicle.Enabled = false;
                await Client.Delay(4000);
                mItemDetainInCurrentVehicle.Enabled = CanDetainPed();
                mItemDetainInCurrentVehicle.Text = _interactivePed.Ped.IsInVehicle() ? "Remove from Vehicle" : "Detain in Vehicle";
            }
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            Log.Info($"{MENU_TITLE} Open");

            menu.MenuTitle = "";
            menu.MenuSubtitle = MENU_TITLE;

            MenuBase.MenuState(true);
            menu.ClearMenuItems();

            if (_interactivePed.IsDead)
            {
                // CPR / Coroner
                if (!_interactivePed.HasCprFailed)
                    menu.AddMenuItem(mItemCpr);

                menu.AddMenuItem(mItemCallCoroner);
            }
            else
            {
                mItemHandcuffs.Text = _interactivePed.IsHandcuffed ? "Remove Handcuffs" : "Apply Handcuffs";
                mItemHandcuffs.Enabled = !_interactivePed.Ped.IsInVehicle();
                menu.AddMenuItem(mItemHandcuffs);

                mItemDetainInCurrentVehicle.Enabled = CanDetainPed();
                mItemDetainInCurrentVehicle.Text = _interactivePed.Ped.IsInVehicle() ? "Remove from Vehicle" : "Detain in Vehicle";
                menu.AddMenuItem(mItemDetainInCurrentVehicle);
            }
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuController.MainMenu.OpenMenu();
        }

        private static bool CanDetainPed()
        {
            return _interactivePed.IsHandcuffed && Client.CurrentVehicle != null;
        }
    }
}
