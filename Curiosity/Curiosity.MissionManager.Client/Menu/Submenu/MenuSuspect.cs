using CitizenFX.Core;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using NativeUI;
using System.Threading.Tasks;


using Ped = Curiosity.MissionManager.Client.Classes.Ped;

namespace Curiosity.MissionManager.Client.Menu.Submenu
{
    class MenuSuspect
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
            MenuManager.OnMenuState();
            PluginInstance.DeregisterTickHandler(OnSuspectDistanceCheck);
        }

        private void Menu_OnMenuOpen(UIMenu sender)
        {
            if (Game.PlayerPed.IsInVehicle())
            {
                MenuManager._MenuPool.CloseAllMenus();

                Notify.Alert(CommonErrors.OutsideVehicle);

                Menu.MenuItems.ForEach(m => m.Enabled = false);

                return;
            }

            bool isCalloutActive = MenuManager.IsCalloutActive;

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

                menuItemHandcuff.Text = Ped.IsHandcuffed ? "Remove Handcuffs" : "Apply Handcuffs";
                menuItemHandcuff.Enabled = isControlable;

                menuItemDetain.Text = Ped.Fx.IsInVehicle() ? "Remove from Vehicle" : "Detain in Vehicle";
                menuItemDetain.Enabled = Ped.IsHandcuffed;

                menuItemGrab.Text = Ped.IsGrabbed ? "Let go of Suspect" : "Grab Suspect";
                menuItemGrab.Enabled = isControlable && Ped.IsHandcuffed;

                PluginInstance.RegisterTickHandler(OnSuspectDistanceCheck);
            }

            MenuManager.OnMenuState(true);
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
