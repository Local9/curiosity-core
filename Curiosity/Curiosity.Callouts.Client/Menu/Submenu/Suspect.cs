using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Classes;
using Curiosity.Callouts.Client.Managers;
using Curiosity.Callouts.Client.Utils;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Ped = Curiosity.Callouts.Client.Classes.Ped;

namespace Curiosity.Callouts.Client.Menu.Submenu
{
    class Suspect
    {
        private PluginManager PluginInstance => PluginManager.Instance;
        private Callout callout => CalloutManager.ActiveCallout;
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

            bool isCalloutActive = (callout != null);

            if (!isCalloutActive)
            {
                menuItemHandcuff.Enabled = false;
                menuItemDetain.Enabled = false;
            }
            else
            {
                Ped = GetClosestSuspect();
                bool isControlable = PedCanBeControled();

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
                    menuItemDetain.Text = "Remove from Vehicle";
                }
                else
                {
                    Ped.RunSequence(Ped.Sequence.DETAIN_IN_CURRENT_VEHICLE);
                    menuItemDetain.Text = "Detain in Vehicle";
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

        private Ped GetClosestSuspect()
        {
            return callout.RegisteredPeds.Select(x => x).Where(p => p.Position.Distance(Game.PlayerPed.Position) < 1.5f && p.IsSuspect && p.IsMission).FirstOrDefault();
        }
    }
}
