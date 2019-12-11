using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Shared.Client.net.Extensions;
using MenuAPI;
using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
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
        // tests
        static MenuItem mItemSuspectVehicle = new MenuItem("Suspect: Ask to Leave Vehicle");
        static MenuItem mItemBreathalyzer = new MenuItem("Suspect: Breathalyzer");
        static MenuItem mItemSearch = new MenuItem("Suspect: Search");
        static MenuItem mItemDrugTest = new MenuItem("Suspect: Drug test");

        static MenuItem mItemWarn = new MenuItem("Suspect: Warn");
        static MenuItem mItemRelease = new MenuItem("Suspect: Release");

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
                MenuController.CloseAllMenus();
                return;
            }
            if (menuItem == mItemCallCoroner)
            {
                Extras.Coroner.RequestService();
                MenuController.CloseAllMenus();
                return;
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
                return;
            }
            if (menuItem == mItemDetainInCurrentVehicle)
            {
                ArrestInteractions.InteractionPutInVehicle(_interactivePed);
                mItemDetainInCurrentVehicle.Enabled = false;
                await Client.Delay(4000);
                mItemDetainInCurrentVehicle.Enabled = CanDetainPed();
                mItemDetainInCurrentVehicle.Text = _interactivePed.Ped.IsInVehicle() ? "Remove from Vehicle" : "Detain in Vehicle";
                return;
            }

            // Main Interactions
            if (menuItem == mItemBreathalyzer)
            {
                Generic.InteractionBreathalyzer(_interactivePed);
                return;
            }

            if (menuItem == mItemDrugTest)
            {
                Generic.InteractionDrugTest(_interactivePed);
                return;
            }

            if (menuItem == mItemSearch)
            {
                Generic.InteractionSearch(_interactivePed);
                return;
            }

            if (menuItem == mItemWarn)
            {
                ArrestInteractions.InteractionIssueWarning(_interactivePed);
                MenuController.CloseAllMenus();
                Client.TriggerEvent("curiosity:interaction:closeMenu");
                return;
            }

            if (menuItem == mItemRelease)
            {
                ArrestInteractions.InteractionRelease(_interactivePed);
                MenuController.CloseAllMenus();
                Client.TriggerEvent("curiosity:interaction:closeMenu");
                return;
            }

            if (menuItem == mItemSuspectVehicle && _interactivePed.Ped.IsInVehicle())
            {
                Generic.InteractionLeaveVehicle(_interactivePed);
                mItemSuspectVehicle.Enabled = false;
                await Client.Delay(2000);
                mItemSuspectVehicle.Text = "Suspect: Return to Vehicle";
                mItemSuspectVehicle.Enabled = true;
                return;
            }

            if (menuItem == mItemSuspectVehicle && !_interactivePed.Ped.IsInVehicle() && DecorExistOn(_interactivePed.Ped.Handle, Client.NPC_CURRENT_VEHICLE))
            {
                Generic.InteractionEnterVehicle(_interactivePed);
                mItemSuspectVehicle.Enabled = false;
                await Client.Delay(2000);
                mItemSuspectVehicle.Text = "Suspect: Ask to Leave Vehicle";
                mItemSuspectVehicle.Enabled = true;

                if (_interactivePed.Ped.IsInGroup)
                    _interactivePed.Ped.LeaveGroup();

                return;
            }
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            Log.Info($"{MENU_TITLE} Open");

            menu.MenuTitle = "";
            menu.MenuSubtitle = MENU_TITLE;

            MenuBase.MenuState(true);
            menu.ClearMenuItems();

            bool IsInVehicle = _interactivePed.Ped.IsInVehicle();

            if (_interactivePed.IsDead)
            {
                // CPR / Coroner
                if (!_interactivePed.HasCprFailed)
                    menu.AddMenuItem(mItemCpr);

                menu.AddMenuItem(mItemCallCoroner);
            }
            else
            {
                mItemSuspectVehicle.Text = "Suspect: Ask to Leave Vehicle";
                if (!_interactivePed.Ped.IsInVehicle() && DecorExistOn(_interactivePed.Ped.Handle, Client.NPC_CURRENT_VEHICLE))
                {
                    mItemSuspectVehicle.Text = "Suspect: Return to Vehicle";
                }
                menu.AddMenuItem(mItemSuspectVehicle);


                mItemHandcuffs.Text = _interactivePed.IsHandcuffed ? "Remove Handcuffs" : "Apply Handcuffs";
                mItemHandcuffs.Enabled = !IsInVehicle;
                menu.AddMenuItem(mItemHandcuffs);

                mItemDetainInCurrentVehicle.Enabled = CanDetainPed();
                mItemDetainInCurrentVehicle.Text = IsInVehicle ? "Remove from Vehicle" : "Detain in Vehicle";
                menu.AddMenuItem(mItemDetainInCurrentVehicle);

                menu.AddMenuItem(mItemBreathalyzer);
                menu.AddMenuItem(mItemDrugTest);

                mItemSearch.Enabled = !IsInVehicle;
                mItemSearch.Description = IsInVehicle ? "Suspect must be removed from the vehicle before searching." : string.Empty;
                menu.AddMenuItem(mItemSearch);

                menu.AddMenuItem(mItemWarn);

                menu.AddMenuItem(mItemRelease);
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
