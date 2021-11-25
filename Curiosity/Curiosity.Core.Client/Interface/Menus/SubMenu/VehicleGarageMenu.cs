using CitizenFX.Core;
using Curiosity.Core.Client.Events;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class VehicleGarageMenu
    {
        UIMenu baseMenu;

        UIMenuItem loadingItem = new UIMenuItem("🔍 Loading");
        bool isLoading = false;
        
        internal void CreateMenu(UIMenu menu)
        {
            baseMenu = menu;

            baseMenu.AddItem(loadingItem);

            baseMenu.OnItemSelect += BaseMenu_OnItemSelect;
            baseMenu.OnMenuStateChanged += BaseMenu_OnMenuStateChanged;
        }

        private async void BaseMenu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.Opened || state == MenuState.ChangeForward)
            {
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
                while(isLoading)
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

        private void BaseMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            
        }

        private async void LoadVehicles()
        {
            List<VehicleItem> vehicles = await EventSystem.GetModule().Request<List<VehicleItem>>("garage:get:list:cars");

            if (vehicles == null)
            {
                loadingItem.Text = $"No vehicles owned.";
                goto END;
            }

            if (vehicles.Count == 0)
            {
                loadingItem.Text = $"No vehicles owned.";
                goto END;
            }

            foreach(VehicleItem vehicle in vehicles)
            {
                UIMenuItem uIMenuItem = new UIMenuItem($"{vehicle.Label} [{vehicle.VehicleInfo.plateText}]");
                uIMenuItem.ItemData = vehicle;
                baseMenu.AddItem(uIMenuItem);
            }

        END:
            isLoading = false;
            baseMenu.RemoveItemAt(0);
        }
    }
}
