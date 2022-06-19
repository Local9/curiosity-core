using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System.Linq;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class VehicleGarageMenu
    {
        NotificationManager Notify => NotificationManager.GetModule();
        VehicleManager vehicleManager => VehicleManager.GetModule();
        UIMenu baseMenu;

        UIMenuItem loadingItem = new UIMenuItem("🔍 Loading");
        bool isLoading = false;

        internal void CreateMenu(UIMenu menu)
        {
            baseMenu = menu;

            baseMenu.OnItemSelect += BaseMenu_OnItemSelect;
            baseMenu.OnMenuStateChanged += BaseMenu_OnMenuStateChanged;
        }

        private async void BaseMenu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeForward)
            {
                baseMenu.Clear();
                baseMenu.AddItem(loadingItem);

                isLoading = true;
                UpdateLoadingDisplay();
                LoadVehicles();
            }
        }

        private async Task UpdateLoadingDisplay()
        {
            int timerDelay = 500;
            if (loadingItem is not null)
            {
                while (isLoading)
                {
                    await BaseScript.Delay(timerDelay);

                    if (loadingItem is null)
                    {
                        isLoading = false;
                        return;
                    }

                    await BaseScript.Delay(timerDelay);
                    loadingItem.Text = "🔍 Loading.";
                    await BaseScript.Delay(timerDelay);
                    loadingItem.Text = "🔎 Loading..";
                    await BaseScript.Delay(timerDelay);
                    loadingItem.Text = "🔍 Loading...";
                    await BaseScript.Delay(timerDelay);
                    loadingItem.Text = "🔎 Loading....";
                }
            }
        }

        private async void BaseMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem.ItemData is null) return;

            VehicleItem vehicleItem = (VehicleItem)selectedItem.ItemData;

            var response = await vehicleManager.CreateVehicle(vehicleItem.CharacterVehicleId, vehicleItem.Hash);
            if (!response.success)
            {
                Notify.Error($"Error when trying to spawn vehicle.");
            }
        }

        private async void LoadVehicles()
        {
            List<VehicleItem> vehicles = await EventSystem.GetModule().Request<List<VehicleItem>>("garage:get:list:cars");

            if (vehicles == null || vehicles.Count == 0)
            {
                loadingItem.Text = $"No vehicles owned.";
                loadingItem.Description = $"You can buy vehicles from the Store";
                goto END;
            }

            foreach (VehicleItem vehicle in vehicles.OrderBy(x => x.Label))
            {
                UIMenuItem uIMenuItem = new UIMenuItem($"{vehicle.Label}");
                uIMenuItem.SetRightLabel(vehicle.VehicleInfo.plateText);
                uIMenuItem.ItemData = vehicle;
                baseMenu.AddItem(uIMenuItem);
            }

        END:
            isLoading = false;
            baseMenu.RemoveItemAt(0);
        }
    }
}
