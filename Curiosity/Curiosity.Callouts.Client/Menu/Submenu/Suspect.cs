using CitizenFX.Core;
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

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuItemHandcuff = new UIMenuItem("Apply Handcuffs");
            menu.AddItem(menuItemHandcuff);

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
            bool isCalloutActive = (callout != null);

            if (!isCalloutActive)
            {
                menuItemHandcuff.Enabled = false;
            }
            else
            {
                Ped = GetClosestSuspect();
                menuItemHandcuff.Enabled = Ped != null;

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
                await BaseScript.Delay(500);
            }
        }

        private Ped GetClosestSuspect()
        {
            return callout.RegisteredPeds.Select(x => x).Where(p => p.Position.Distance(Game.PlayerPed.Position) < 1.5f && p.IsSuspect && p.IsMission).FirstOrDefault();
        }
    }
}
