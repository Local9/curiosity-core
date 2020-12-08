﻿using CitizenFX.Core;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Handler;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Enums;
using NativeUI;
using System.Threading.Tasks;


using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client.Menu.Submenu
{
    class MenuVehicle
    {
        private PluginManager PluginInstance => PluginManager.Instance;
        private Vehicle vehicle;

        UIMenu Menu;
        UIMenuItem menuItemSearchVehicle;
        UIMenuItem menuItemRecordNumberPlate;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuItemRecordNumberPlate = new UIMenuItem("Note Number Plate", "Take recording of the number plate");
            menu.AddItem(menuItemRecordNumberPlate);

            menuItemSearchVehicle = new UIMenuItem("Search");
            menu.AddItem(menuItemSearchVehicle);

            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            Menu = menu;
            return menu;
        }

        private void Menu_OnMenuClose(UIMenu sender)
        {
            MenuManager.OnMenuState();
            PluginInstance.DetachTickHandler(OnSuspectVehicleDistanceCheck);
        }

        private void Menu_OnMenuOpen(UIMenu sender)
        {
            MenuManager.OnMenuState(true);

            bool isCalloutActive = Mission.isOnMission;

            if (!isCalloutActive)
            {
                Menu.MenuItems.ForEach(m => m.Enabled = false);
            }
            else
            {
                vehicle = MenuManager.GetClosestVehicle();

                if (vehicle == null)
                {
                    Notify.Alert(CommonErrors.SubjectNotFound);
                    MenuManager._MenuPool.CloseAllMenus();
                    Menu.MenuItems.ForEach(m => m.Enabled = false);
                    return;
                }

                menuItemSearchVehicle.Enabled = vehicle.IsSearchable;
                menuItemSearchVehicle.Description = vehicle.IsSearchable ? "Able to search the vehicle" : "Unable to search this vehicle";

                menuItemSearchVehicle.Enabled = true;

                PluginInstance.AttachTickHandler(OnSuspectVehicleDistanceCheck);
            }
        }

        private async Task OnSuspectVehicleDistanceCheck()
        {
            if (vehicle.Position.Distance(Game.PlayerPed.Position) > 3f)
                MenuManager._MenuPool.CloseAllMenus();
        }

        private async void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == menuItemSearchVehicle)
            {
                vehicle.Sequence(Vehicle.VehicleSequence.SEARCH);
                await BaseScript.Delay(500);
                return;
            }

            if (selectedItem == menuItemRecordNumberPlate)
            {
                Game.PlayerPed.AnimationTicket();
                vehicle.RecordLicensePlate();
            }
        }
    }
}
