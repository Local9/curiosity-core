using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Utils;
using NativeUI;

using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client.Menu.Submenu
{
    class Dispatch
    {
        UIMenuItem menuItemCode4;
        UIMenuItem menuItemCode16;
        UIMenuItem menuItemCode27;
        UIMenuItem menuItemCode28;
        UIMenuItem menuItemCode29;
        UIMenuItem menuItemCode51;
        UIMenuItem menuItemCode52;
        UIMenuItem menuItemCode55d;
        UIMenuItem menuItemCode78;
        UIMenuItem menuItemCode80;
        UIMenuItem menuItemCode92;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuItemCode4 = new UIMenuItem("Code 4: No further assistance required", "~r~This will complete the current callout. ~o~If you have not completed all the required tasks, you will receive a penalty!");
            menuItemCode16 = new UIMenuItem("10-16: Request prison transport", "Will send an NPC to collect prisoners. ~n~~o~5 Minute Cooldown");
            menuItemCode27 = new UIMenuItem("10-27: Request drivers license check", "Contact dispatch for more information on the NPC");
            menuItemCode28 = new UIMenuItem("10-28: Check registration on vehicle", "Contact dispatch for more information on the vehicle");
            menuItemCode29 = new UIMenuItem("10-29: Check Wants", "Contact dispatch to find out if the NPC is wanted");
            menuItemCode51 = new UIMenuItem("10-51: Tow truck needed", "Will send an NPC to remove the vehicle");
            menuItemCode52 = new UIMenuItem("10-52: Ambulance needed", "Will send an NPC to try and save a dying/injured NPC");
            menuItemCode55d = new UIMenuItem("10-55d: Send Coroner", "Will send an NPC Coroner to clear deceased NPCs");
            menuItemCode78 = new UIMenuItem("10-78: Need Assistance", "This will call on other players for assistance. ~n~~o~5 Minute Cooldown");
            menuItemCode80 = new UIMenuItem("10-80: Persuit in progress", "This will call on other players for assistance. ~n~~o~5 Minute Cooldown");
            menuItemCode92 = new UIMenuItem("10-92: Suspect in Custody", "This will inform other players, required on some callouts.");

            menu.AddItem(menuItemCode4);
            menu.AddItem(menuItemCode16);
            menu.AddItem(menuItemCode27);
            menu.AddItem(menuItemCode28);
            menu.AddItem(menuItemCode29);
            menu.AddItem(menuItemCode51);
            menu.AddItem(menuItemCode52);
            menu.AddItem(menuItemCode55d);
            menu.AddItem(menuItemCode78);
            menu.AddItem(menuItemCode80);
            menu.AddItem(menuItemCode92);

            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;
            menu.OnItemSelect += Menu_OnItemSelect;

            return menu;
        }

        private void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == menuItemCode4) // end callout
            {
                Mission.currentMission?.End(); // need a success state
                return;
            }

            if (selectedItem == menuItemCode51) // impound
            {
                Vehicle suspectVehicle = MenuBase.GetClosestVehicle();
                if (suspectVehicle != null)
                {
                    suspectVehicle.Impound();
                }
                else
                {
                    UiTools.Impound("Sorry...?", "Whats the registration again? Get closer so you can read it better.");
                }
            }

            if (selectedItem == menuItemCode16) // prison transport
            {
                Ped ped = MenuBase.GetClosestInteractivePed();
                if (ped != null)
                {
                    ped.PrisonTransport();
                }
                else
                {
                    Screen.ShowSubtitle($"Must be closer to the suspect.");
                }
            }
        }

        private void Menu_OnMenuClose(UIMenu sender)
        {
            MenuBase.OnMenuState();
        }

        private void Menu_OnMenuOpen(UIMenu sender)
        {
            MenuBase.OnMenuState(true);

            bool isCalloutActive = MenuBase.IsCalloutActive;
            bool isVehicleTowable = MenuBase.GetClosestVehicle() != null;

            menuItemCode4.Enabled = isCalloutActive;
            menuItemCode16.Enabled = isCalloutActive;
            menuItemCode27.Enabled = isCalloutActive;
            menuItemCode28.Enabled = isCalloutActive;
            menuItemCode29.Enabled = isCalloutActive;
            menuItemCode51.Enabled = isCalloutActive && isVehicleTowable;
            menuItemCode52.Enabled = isCalloutActive;
            menuItemCode55d.Enabled = isCalloutActive;
            menuItemCode78.Enabled = isCalloutActive;
            menuItemCode80.Enabled = isCalloutActive;
            menuItemCode92.Enabled = isCalloutActive;
        }
    }
}
