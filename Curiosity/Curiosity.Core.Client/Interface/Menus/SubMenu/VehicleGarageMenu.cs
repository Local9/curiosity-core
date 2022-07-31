using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System.Linq;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class VehicleGarageMenu
    {
        Dictionary<int, UIMenu> _classMenus = new();

        NotificationManager Notify => NotificationManager.GetModule();
        VehicleManager vehicleManager => VehicleManager.GetModule();
        UIMenu baseMenu;

        UIMenuItem loadingItem = new UIMenuItem("🔍 Loading");
        bool isLoading = false;

        internal void CreateMenu(UIMenu menu)
        {
            baseMenu = menu;

            // baseMenu.OnItemSelect += BaseMenu_OnItemSelect;
            baseMenu.OnMenuStateChanged += BaseMenu_OnMenuStateChanged;
        }

        private async void BaseMenu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeForward && (newMenu == baseMenu))
            {
                foreach (KeyValuePair<int, UIMenu> keyValuePair in _classMenus)
                {
                    keyValuePair.Value.Clear();
                }

                baseMenu.Clear();
                baseMenu.AddItem(loadingItem);

                _classMenus.Clear();

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

            foreach (VehicleItem vehicle in vehicles)
            {
                uint model = (uint)GetHashKey(vehicle.Hash);
                int vehicleClass = GetVehicleClassFromName(model);
                string label = Game.GetGXTEntry($"VEH_CLASS_{vehicleClass}");

                if (_classMenus.ContainsKey(vehicleClass)) continue;

                UIMenu classSubMenu = InteractionMenu.MenuPool.AddSubMenu(baseMenu, label);

                foreach (VehicleItem subVehicle in vehicles.OrderBy(x => x.Label))
                {
                    uint subModel = (uint)GetHashKey(subVehicle.Hash);
                    int subVehicleClass = GetVehicleClassFromName(subModel);

                    if (subVehicleClass == vehicleClass)
                    {
                        string displayName = GetDisplayNameFromVehicleModel(subModel);
                        string vehicleLabel = Game.GetGXTEntry($"{displayName}");

                        if (displayName == "CARNOTFOUND")
                            vehicleLabel = subVehicle.Label;

                        UIMenuItem uIMenuItem = new UIMenuItem(vehicleLabel);
                        uIMenuItem.SetRightLabel(subVehicle.VehicleInfo.plateText);
                        uIMenuItem.ItemData = subVehicle;

                        classSubMenu.AddItem(uIMenuItem);
                    }
                }

                _classMenus.Add(vehicleClass, classSubMenu);
                classSubMenu.OnItemSelect += BaseMenu_OnItemSelect;
            }

        END:
            isLoading = false;
            baseMenu.RemoveItemAt(0);
        }
    }
}
