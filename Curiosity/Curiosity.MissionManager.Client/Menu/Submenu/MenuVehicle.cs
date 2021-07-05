using CitizenFX.Core;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Handler;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Entity;
using Curiosity.Systems.Library.Enums;
using NativeUI;
using System;
using System.Collections.Generic;
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
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;

            Menu = menu;
            return menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.Closed)
                OnMenuClose();

            if (state == MenuState.Opened)
                OnMenuOpen();
        }

        private void OnMenuClose()
        {
            MenuManager.OnMenuState();
            PluginInstance.DetachTickHandler(OnSuspectVehicleDistanceCheck);
        }

        private void OnMenuOpen()
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
            if (vehicle.Position.Distance(Cache.PlayerPed.Position) > 3f)
                MenuManager._MenuPool.CloseAllMenus();
        }

        private async void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == menuItemSearchVehicle)
            {
                vehicle.Sequence(Vehicle.VehicleSequence.SEARCH);

                MissionDataVehicle missionDataVehicle = await EventSystem.GetModule().Request<MissionDataVehicle>("mission:update:vehicle:search", vehicle.NetworkId);

                DateTime searchStart = DateTime.Now;

                while (DateTime.Now.Subtract(searchStart).TotalSeconds < 2)
                {
                    await BaseScript.Delay(500);
                }

                if (missionDataVehicle == null) return;

                if (missionDataVehicle.Items.Count > 0)
                {
                    List<string> items = new List<string>();
                    foreach (KeyValuePair<string, bool> kvp in missionDataVehicle.Items)
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

            if (selectedItem == menuItemRecordNumberPlate)
            {
                Cache.PlayerPed.AnimationTicket();
                vehicle.RecordLicensePlate();
            }
        }
    }
}
