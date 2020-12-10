using CitizenFX.Core;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Handler;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Manager;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Shared.Entity;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


using Ped = Curiosity.MissionManager.Client.Classes.Ped;

namespace Curiosity.MissionManager.Client.Menu.Submenu
{
    class MenuSuspect
    {
        private PluginManager PluginInstance => PluginManager.Instance;
        private Ped Ped;

        UIMenu Menu;
        UIMenu menuQuestions;
        DefinedMenus.MenuQuestions _menuQuestions = new DefinedMenus.MenuQuestions();

        UIMenuItem menuItemHandcuff;
        UIMenuItem menuItemDetain;
        UIMenuItem menuItemGrab;
        UIMenuItem menuItemSearch;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuQuestions = MenuManager._MenuPool.AddSubMenu(menu, "Questions", "Ask the suspect questions.");
            menuQuestions.MouseControlsEnabled = false;
            _menuQuestions.CreateMenu(menuQuestions);

            menuItemHandcuff = new UIMenuItem("Apply Handcuffs");
            menu.AddItem(menuItemHandcuff);
            menuItemDetain = new UIMenuItem("Detain in Vehicle");
            menu.AddItem(menuItemDetain);
            menuItemGrab = new UIMenuItem("Lead Suspect");
            menu.AddItem(menuItemGrab);
            menuItemSearch = new UIMenuItem("Search Suspect");
            menu.AddItem(menuItemSearch);

            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            Menu = menu;
            return menu;
        }

        private void Menu_OnMenuClose(UIMenu sender)
        {
            MenuManager.OnMenuState();
            PluginInstance.DetachTickHandler(OnSuspectDistanceCheck);
        }

        private void Menu_OnMenuOpen(UIMenu sender)
        {
            MenuManager.OnMenuState(true);

            if (Game.PlayerPed.IsInVehicle())
            {
                MenuManager._MenuPool.CloseAllMenus();

                Notify.Alert(CommonErrors.OutsideVehicle);

                Menu.MenuItems.ForEach(m => m.Enabled = false);

                return;
            }

            bool isCalloutActive = Mission.isOnMission;

            if (!isCalloutActive)
            {
                Menu.MenuItems.ForEach(m => m.Enabled = false);
            }
            else
            {
                Ped = MenuManager.GetClosestInteractivePed();
                bool isControlable = PedCanBeControled();

                if (Ped == null)
                {
                    Notify.Alert(CommonErrors.MustBeCloserToSubject);
                    MenuManager._MenuPool.CloseAllMenus();
                    Menu.MenuItems.ForEach(m => m.Enabled = false);
                    return;
                }

                Menu.MenuItems.ForEach(m => m.Enabled = true);

                menuItemHandcuff.Text = Ped.IsHandcuffed ? "Remove Handcuffs" : "Apply Handcuffs";
                menuItemHandcuff.Enabled = isControlable;

                menuItemDetain.Text = Ped.Fx.IsInVehicle() ? "Remove from Vehicle" : "Detain in Vehicle";
                menuItemDetain.Enabled = Ped.IsHandcuffed;

                menuItemGrab.Text = Ped.IsGrabbed ? "Let go of Suspect" : "Grab Suspect";
                menuItemGrab.Enabled = isControlable && Ped.IsHandcuffed;

                PluginInstance.AttachTickHandler(OnSuspectDistanceCheck);
            }
        }

        private async Task OnSuspectDistanceCheck()
        {
            if (Ped.Position.Distance(Game.PlayerPed.Position) > 3f)
                MenuManager._MenuPool.CloseAllMenus();
        }

        private async void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (Ped == null)
            {
                Notify.Alert(CommonErrors.SubjectNotFound);
                return;
            }

            if (selectedItem == menuItemHandcuff)
            {
                Ped.IsHandcuffed = !Ped.IsHandcuffed;
                selectedItem.Text = Ped.IsHandcuffed ? "Remove Handcuffs" : "Apply Handcuffs";
                menuItemDetain.Enabled = Ped.IsHandcuffed;
                menuItemGrab.Enabled = Ped.IsHandcuffed;

                await BaseScript.Delay(500);
                return;
            }

            if (selectedItem == menuItemDetain)
            {
                if (PlayerManager.PersonalVehicle == null)
                {
                    Notify.Alert($"Unable to find vehicle vehicle.");
                    return;
                }

                if (!PlayerManager.PersonalVehicle.Exists())
                {
                    Notify.Alert($"Unable to find vehicle vehicle.");
                    return;
                }

                if (Ped.Position.DistanceTo(PlayerManager.PersonalVehicle.Position) > 10f)
                {
                    Notify.Alert($"You're too far from your vehicle.");
                    return;
                }

                if (Ped.Fx.IsInVehicle())
                {
                    Ped.RunSequence(Ped.Sequence.LEAVE_VEHICLE);
                    selectedItem.Text = "Detain in Vehicle";
                }
                else
                {
                    Ped.RunSequence(Ped.Sequence.DETAIN_IN_CURRENT_VEHICLE);
                    selectedItem.Text = "Remove from Vehicle";
                }

                await BaseScript.Delay(500);
                return;
            }

            if (selectedItem == menuItemGrab)
            {
                if (Ped.IsGrabbed)
                {
                    Ped.RunSequence(Ped.Sequence.GRAB_RELEASE);
                    selectedItem.Text = "Grab Suspect";
                }
                else
                {
                    Ped.RunSequence(Ped.Sequence.GRAB_HOLD);
                    selectedItem.Text = "Let go of Suspect";
                }

                await BaseScript.Delay(500);
                return;
            }

            if (selectedItem == menuItemSearch)
            {
                CitizenFX.Core.Ped ped = Game.PlayerPed.GetPedInFront(pedToCheck: Ped.Fx);

                if (ped == null)
                {
                    Notify.Alert(CommonErrors.PedMustBeInFront);
                    return;
                }

                AnimationHandler.AnimationSearch();

                MissionDataPed missionDataPed = await EventSystem.GetModule().Request<MissionDataPed>("mission:update:ped:search", Ped.NetworkId);

                DateTime searchStart = DateTime.Now;

                while(DateTime.Now.Subtract(searchStart).TotalSeconds < 2)
                {
                    await BaseScript.Delay(500);
                }

                if (missionDataPed == null) return;

                if (missionDataPed.Items.Count > 0)
                {
                    List<string> items = new List<string>();
                    foreach(KeyValuePair<string, bool> kvp in missionDataPed.Items)
                    {
                        string item = kvp.Value ? $"~o~{kvp.Key}" : $"~g~{kvp.Key}";
                        items.Add(item);
                    }

                    string found = string.Join("~n~", items);
                    Notify.Info($"~n~{found}");
                }
                else
                {
                    Notify.Info($"Found nothing");
                }

                await BaseScript.Delay(500);
                return;
            }
        }

        private bool PedCanBeControled()
        {
            if (Ped != null)
            {
                if (Ped.Exists())
                    return Ped.IsAlive;
            }
            return false;
        }
    }
}
