using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Manager;
using Curiosity.Systems.Library.Enums;
using NativeUI;
using System;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client.Menu.Submenu
{
    class MenuDispatch
    {
        EventSystem eventSystem => EventSystem.GetModule();

        UIMenuItem menuItemCode4;
        UIMenuItem menuItemCode16;
        UIMenuItem menuItemCode51;
        UIMenuItem menuItemCode55d;
        UIMenuItem menuItemCode78;
        UIMenuItem menuItemOpenComputer;

        UIMenuSeparatorItem menuSeparatorItem1 = new UIMenuSeparatorItem();
        UIMenuSeparatorItem menuSeparatorItem2 = new UIMenuSeparatorItem();

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuItemCode4 = new UIMenuItem("Code 4: End Callout", "~o~This will end the current callout.");
            menuItemOpenComputer = new UIMenuItem("Open LSPD:NC", "Open the LSPD:NC, must be in vehicle");

            menuItemCode78 = new UIMenuItem("10-78: Need Assistance", "This will call on other players for assistance. ~b~Shortcut: ~g~ALT+E");

            menuItemCode16 = new UIMenuItem("10-16: Request prison transport", "Will remove the suspect from the world, ~o~50% of completion reward~w~.");
            menuItemCode51 = new UIMenuItem("10-51: Tow Vehicle", "Will remove the vehicle from the world.~n~~o~Callout Only");
            menuItemCode55d = new UIMenuItem("10-55d: Send Coroner", "Will clear deceased NPC(s).~n~~o~Callout Only");
            

            menu.AddItem(menuItemCode4);
            menu.AddItem(menuItemOpenComputer);

            menu.AddItem(menuSeparatorItem1);

            menu.AddItem(menuItemCode78);

            menu.AddItem(menuSeparatorItem2);

            menu.AddItem(menuItemCode16); //
            menu.AddItem(menuItemCode51); //
            menu.AddItem(menuItemCode55d); // 

            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;

            return menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeBackward)
                MenuManager.OnMenuState();

            if (state == MenuState.ChangeForward)
                OnMenuOpen();
        }

        private void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            Ped ped = MenuManager.GetClosestInteractivePed();
            Vehicle suspectVehicle = MenuManager.GetClosestVehicle();

            if (selectedItem == menuItemOpenComputer)
            {
                if (!(Cache.PlayerPed.IsInVehicle() && Cache.PlayerPed.CurrentVehicle == PlayerManager.GetModule().PersonalVehicle))
                {
                    Notify.Alert(CommonErrors.InsideVehicle);
                }

                DepartmentComputer.ComputerBase.Instance.Open();
                MenuManager._MenuPool?.CloseAllMenus();
                return;
            }

            if (selectedItem == menuItemCode4) // end callout
            {
                try
                {
                    if (Mission.currentMissionType == MissionType.TrafficStop)
                    {
                        if (Mission.NumberPedsArrested > 0)
                        {
                            Mission.currentMission.Pass();
                        }
                        else
                        {
                            Mission.currentMission.Stop(EndState.TrafficStop);
                        }
                    }
                    else
                    {
                        Mission.currentMission?.Stop(EndState.ForceEnd);
                    }

                    MenuManager._MenuPool?.CloseAllMenus();
                }
                catch(Exception ex)
                {
                    Logger.Debug($"CODE 4: {ex}");
                }
                return;
            }

            if (selectedItem == menuItemCode78) // end callout
            {
                try
                {
                    eventSystem.Request<bool>("mission:assistance:request");
                    Notify.DispatchAI("Back Up Requested", "We have informed all available officers that you have requested back up at your location.");
                    MenuManager._MenuPool?.CloseAllMenus();
                }
                catch (Exception ex)
                {
                    Logger.Debug($"10-78: Need Assistance: {ex}");
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
                    if (!ped.IsHandcuffed)
                    {
                        Notify.Alert(CommonErrors.MustBeHandcuffed);
                        return;
                    }

                    if (ped.Fx.IsInVehicle())
                    {
                        Notify.Alert(CommonErrors.NpcOutsideVehicle);
                        return;
                    }

                    Mission.CountArrest();
                    Mission.CountTransportArrest();
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

        private void OnMenuOpen()
        {
            MenuManager.OnMenuState(true);

            bool isCalloutActive = Mission.isOnMission;
            bool isVehicleTowable = MenuManager.GetClosestVehicle() != null;

            menuItemCode4.Enabled = isCalloutActive;
            menuItemCode16.Enabled = isCalloutActive;
            menuItemCode51.Enabled = isCalloutActive && isVehicleTowable;
            menuItemCode78.Enabled = isCalloutActive;
            menuItemCode55d.Enabled = isCalloutActive;
            menuItemOpenComputer.Enabled = (Cache.PlayerPed.IsInVehicle() && Cache.PlayerPed.CurrentVehicle == PlayerManager.GetModule().PersonalVehicle);
        }
    }
}
