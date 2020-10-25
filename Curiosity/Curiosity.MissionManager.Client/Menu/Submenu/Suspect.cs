﻿using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Utils;
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

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuItemHandcuff = new UIMenuItem("Apply Handcuffs");
            menu.AddItem(menuItemHandcuff);
            menuItemDetain = new UIMenuItem("Detain");
            menu.AddItem(menuItemDetain);

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
                menuItemHandcuff.Text = Ped.IsHandcuffed ? "Remove Handcuffs" : "Apply Handcuffs";
                menuItemDetain.Enabled = Ped.IsHandcuffed;

                await BaseScript.Delay(500);
                return;
            }

            if (selectedItem == menuItemDetain)
            {
                if (Ped.Fx.IsInVehicle())
                {
                    Ped.RunSequence(Ped.Sequence.LEAVE_VEHICLE);
                    menuItemDetain.Text = "Detain from Vehicle";
                }
                else
                {
                    Ped.RunSequence(Ped.Sequence.DETAIN_IN_CURRENT_VEHICLE);
                    menuItemDetain.Text = "Remove in Vehicle";
                }
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
