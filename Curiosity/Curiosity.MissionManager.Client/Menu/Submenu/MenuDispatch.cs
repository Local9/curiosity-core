using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Enums;
using NativeUI;
using System;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client.Menu.Submenu
{
    class MenuDispatch
    {
        UIMenuItem menuItemCode4;
        UIMenuItem menuItemCode16;
        UIMenuItem menuItemCode27;
        UIMenuItem menuItemCode28;
        UIMenuItem menuItemCode29;
        UIMenuItem menuItemCode51;
        UIMenuItem menuItemCode55d;
        UIMenuItem menuItemCode78;
        UIMenuItem menuItemCode80;
        UIMenuItem menuItemCode92;

        UIMenuSeparatorItem menuSeparatorItem1 = new UIMenuSeparatorItem();
        UIMenuSeparatorItem menuSeparatorItem2 = new UIMenuSeparatorItem();

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuItemCode4 = new UIMenuItem("Code 4: Cancel Callout", "~o~This will complete/end the current callout, no rewards earned, will lose reputation.");
            menuItemCode92 = new UIMenuItem("10-92: Suspect in Custody", "This will inform other players.");

            menuItemCode78 = new UIMenuItem("10-78: Need Assistance", "This will call on other players for assistance.");
            menuItemCode80 = new UIMenuItem("10-80: Persuit in progress", "This will call on other players for assistance.");

            menuItemCode16 = new UIMenuItem("10-16: Request prison transport", "Will remove the suspect from the world for ~o~$100.");
            menuItemCode27 = new UIMenuItem("10-27: Request drivers license check", "Contact dispatch for more information on the Suspect.");
            menuItemCode28 = new UIMenuItem("10-28: Check registration on vehicle", "Contact dispatch for more information on the vehicle.");
            menuItemCode29 = new UIMenuItem("10-29: Check Wants", "Contact dispatch to find out if the suspect is wanted.");
            menuItemCode51 = new UIMenuItem("10-51: Tow Vehicle", "Will remove the vehicle from the world.");
            menuItemCode55d = new UIMenuItem("10-55d: Send Coroner", "Will clear deceased NPC(s).");
            

            menu.AddItem(menuItemCode4);
            menu.AddItem(menuItemCode92);

            menu.AddItem(menuSeparatorItem1);

            menu.AddItem(menuItemCode78);
            menu.AddItem(menuItemCode80);

            menu.AddItem(menuSeparatorItem2);

            menu.AddItem(menuItemCode16); //
            menu.AddItem(menuItemCode27);
            menu.AddItem(menuItemCode28);
            menu.AddItem(menuItemCode29);
            menu.AddItem(menuItemCode51); //
            menu.AddItem(menuItemCode55d); // 

            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;
            menu.OnItemSelect += Menu_OnItemSelect;

            return menu;
        }

        private void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            Ped ped = MenuManager.GetClosestInteractivePed();
            Vehicle suspectVehicle = MenuManager.GetClosestVehicle();

            if (selectedItem == menuItemCode4) // end callout
            {
                try
                {
                    Mission.currentMission?.Stop(EndState.ForceEnd); // need a success state
                    MenuManager._MenuPool?.CloseAllMenus();
                }
                catch(Exception ex)
                {
                    Logger.Debug($"CODE 4: {ex}");                
                }
                return;
            }

            if (selectedItem == menuItemCode51) // impound
            {
                
                if (suspectVehicle != null)
                {
                    suspectVehicle?.Dismiss();

                    Notify.Impound("Vehicle Impounded", "");
                }
                else
                {
                    Notify.Impound("Sorry...?", "");
                }
            }

            if (selectedItem == menuItemCode16) // prison transport
            {
                if (ped != null)
                {
                    ped?.Dismiss();

                    Notify.Dispatch("Prison Transport", "Suspect has been picked up.");
                }
                else
                {
                    Notify.Alert(CommonErrors.MustBeCloserToSubject);
                }
            }

            if (selectedItem == menuItemCode55d)
            {
                if (ped != null)
                {
                    if (ped.IsAlive)
                    {
                        Notify.Dispatch("Coroner", "We don't collect the living.");
                        return;
                    }

                    ped?.Dismiss();

                    Notify.Dispatch("Coroner", "Coroner has collected the body.");
                }
                else
                {
                    Notify.Alert(CommonErrors.MustBeCloserToSubject);
                }
            }
        }

        private void Menu_OnMenuClose(UIMenu sender)
        {
            MenuManager.OnMenuState();
        }

        private void Menu_OnMenuOpen(UIMenu sender)
        {
            MenuManager.OnMenuState(true);

            bool isCalloutActive = MenuManager.IsCalloutActive;
            bool isVehicleTowable = MenuManager.GetClosestVehicle() != null;

            menuItemCode4.Enabled = isCalloutActive;
            menuItemCode16.Enabled = isCalloutActive;
            menuItemCode27.Enabled = isCalloutActive;
            menuItemCode28.Enabled = isCalloutActive;
            menuItemCode29.Enabled = isCalloutActive;
            menuItemCode51.Enabled = isCalloutActive && isVehicleTowable;
            menuItemCode55d.Enabled = isCalloutActive;
            menuItemCode78.Enabled = isCalloutActive;
            menuItemCode80.Enabled = isCalloutActive;
            menuItemCode92.Enabled = isCalloutActive;
        }
    }
}
