﻿using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Shared.Client.net.Extensions;
using NativeUI;
using System.Threading.Tasks;


using Ped = Curiosity.MissionManager.Client.Classes.Ped;

namespace Curiosity.MissionManager.Client.Menu.Submenu
{
    class Suspect
    {
        private PluginManager PluginInstance => PluginManager.Instance;
        private Ped Ped;

        UIMenu Menu;
        UIMenuItem menuItemHandcuff;
        UIMenuItem menuItemDetain;
        UIMenuItem menuItemGrab;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuItemHandcuff = new UIMenuItem("Apply Handcuffs");
            menu.AddItem(menuItemHandcuff);
            menuItemDetain = new UIMenuItem("Detain in Vehicle");
            menu.AddItem(menuItemDetain);
            menuItemGrab = new UIMenuItem("Lead Suspect");
            menu.AddItem(menuItemGrab);

            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            Menu = menu;
            return menu;
        }

        private void Menu_OnMenuClose(UIMenu sender)
        {
            MenuBase.OnMenuState();
            PluginInstance.DeregisterTickHandler(OnSuspectDistanceCheck);
        }

        private void Menu_OnMenuOpen(UIMenu sender)
        {
            if (Game.PlayerPed.IsInVehicle())
            {
                MenuBase._MenuPool.CloseAllMenus();
                Screen.ShowNotification($"Cannot interact with suspects while in a vehicle.");
                return;
            }

            bool isCalloutActive = MenuBase.IsCalloutActive;

            if (!isCalloutActive)
            {
                menuItemHandcuff.Enabled = false;
                menuItemDetain.Enabled = false;
            }
            else
            {
                Ped = MenuBase.GetClosestSuspect();
                bool isControlable = PedCanBeControled();

                if (Ped == null)
                {
                    UiTools.Dispatch("No suspect nearby", $"");
                    MenuBase._MenuPool.CloseAllMenus();
                    return;
                }

                menuItemHandcuff.Text = Ped.IsHandcuffed ? "Remove Handcuffs" : "Apply Handcuffs";
                menuItemHandcuff.Enabled = isControlable;

                menuItemDetain.Text = Ped.Fx.IsInVehicle() ? "Remove from Vehicle" : "Detain in Vehicle";
                menuItemDetain.Enabled = Ped.IsHandcuffed;

                menuItemGrab.Text = Ped.IsGrabbed ? "Let go of Suspect" : "Grab Suspect";
                menuItemGrab.Enabled = isControlable && Ped.IsHandcuffed;

                PluginInstance.RegisterTickHandler(OnSuspectDistanceCheck);
            }

            MenuBase.OnMenuState(true);
        }

        private async Task OnSuspectDistanceCheck()
        {
            if (Ped.Position.Distance(Game.PlayerPed.Position) > 3f)
                MenuBase._MenuPool.CloseAllMenus();
        }

        private async void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (Ped == null)
            {
                UiTools.Dispatch("Suspect", "Currently you don't have one", true, true);
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
                if (Ped.Fx.IsInVehicle())
                {
                    Ped.RunSequence(Ped.Sequence.LEAVE_VEHICLE);
                    selectedItem.Text = "Detain from Vehicle";
                }
                else
                {
                    Ped.RunSequence(Ped.Sequence.DETAIN_IN_CURRENT_VEHICLE);
                    selectedItem.Text = "Remove in Vehicle";
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
