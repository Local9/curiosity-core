using CitizenFX.Core;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Interface.Menus.SubMenu.Inventory;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Managers.UI;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using NativeUI;
using System;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class InventoryMenu
    {
        UIMenu baseMenu;
        EventSystem EventSystem => EventSystem.GetModule();

        UIMenu menuBodyArmor;
        BodyArmorMenu _bodyArmorMenu = new BodyArmorMenu();

        UIMenu menuHealth;
        HealthMenu _healthMenu = new HealthMenu();

        PlayerOptionsManager playerOptionsManager = PlayerOptionsManager.GetModule();

        UIMenuCheckboxItem miScubaEquipment = new UIMenuCheckboxItem("Scuba Equipment", false);

        UIMenuItem miRepairKit = new UIMenuItem("Repair Kits");
        UIMenuListItem miLstRepairKit;
        List<dynamic> repairKitList = new List<dynamic>() { "Vehicle", "Trailer", "Boat", "Plane", "Helicopter" };
        /*
         * Health Kits
         * Armor Kits
         * Scuba Kit
         * Repair Kits
         * 
         * */

        public void CreateMenu(UIMenu menu)
        {
            baseMenu = menu;

            menuHealth = InteractionMenu.MenuPool.AddSubMenu(menu, "Health Kits");
            _healthMenu.CreateMenu(menuHealth);

            menuBodyArmor = InteractionMenu.MenuPool.AddSubMenu(menu, "Body Armor");
            _bodyArmorMenu.CreateMenu(menuBodyArmor);

            miRepairKit.SetRightLabel($"0");
            miRepairKit.Enabled = false;
            baseMenu.AddItem(miRepairKit);

            miLstRepairKit = new UIMenuListItem("Repair", repairKitList, 0);
            baseMenu.AddItem(miLstRepairKit);

            baseMenu.AddItem(miScubaEquipment);

            baseMenu.OnMenuStateChanged += BaseMenu_OnMenuStateChanged;
            baseMenu.OnCheckboxChange += BaseMenu_OnCheckboxChange;
            baseMenu.OnListSelect += BaseMenu_OnListSelect;
        }

        private async void BaseMenu_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            NotificationManager notificationManager = NotificationManager.GetModule();
            if (listItem == miLstRepairKit)
            {
                bool canRepair = false;
                Vehicle vehicle = null;

                switch (newIndex)
                {
                    case 1: // Trailer
                        vehicle = Cache.PersonalTrailer?.Vehicle ?? null;
                        break;
                    case 2: // Boat
                        vehicle = Cache.PersonalBoat?.Vehicle ?? null;
                        break;
                    case 3: // Plane
                        vehicle = Cache.PersonalPlane?.Vehicle ?? null;
                        break;
                    case 4: // Heli
                        vehicle = Cache.PersonalHelicopter?.Vehicle ?? null;
                        break;
                    default: // Vehicle
                        vehicle = Cache.PersonalVehicle?.Vehicle ?? null;
                        break;
                }

                if (vehicle is null)
                {
                    notificationManager.Error($"Vehicle not found.");
                    return;
                }

                canRepair = vehicle.Exists() && !vehicle.IsDead && vehicle.IsDriveable;

                if (!canRepair)
                {
                    notificationManager.Error($"Cannot repair that vehicle.");
                    return;
                }

                ExportMessage exportMessage = await InventoryManager.GetModule().UseItem(447, vehicle);

                if (exportMessage.Success)
                {
                    UpdateRepairKits();
                }
            }
        }

        private async void UpdateRepairKits()
        {
            List<CharacterKit> kits = await EventSystem.Request<List<CharacterKit>>("character:inventory:repair");
            if (kits is null)
            {
                miRepairKit.SetRightLabel($"0");
            }
            else if (kits.Count == 0)
            {
                miRepairKit.SetRightLabel($"0");
            }
            else
            {
                miRepairKit.SetRightLabel($"{kits[0].NumberOwned}");
            }
        }

        private async void BaseMenu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            if (checkboxItem == miScubaEquipment)
            {
                miScubaEquipment.Enabled = false;
                playerOptionsManager.ToggleScubaEquipment();
                await BaseScript.Delay(1000);
                miScubaEquipment.Enabled = true;
            }
        }

        private void BaseMenu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            UpdateMenuItems();
        }

        private async void UpdateMenuItems()
        {
            miScubaEquipment.Enabled = false;
            miScubaEquipment.Checked = playerOptionsManager.IsScubaGearEnabled;
            bool hasScubaGear = await EventSystem.Request<bool>("character:inventory:hasItem", 446);
            miScubaEquipment.Enabled = hasScubaGear;

            UpdateRepairKits();
        }
    }
}
