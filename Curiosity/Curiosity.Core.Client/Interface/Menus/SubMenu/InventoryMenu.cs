﻿using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Interface.Menus.SubMenu.Inventory;
using Curiosity.Core.Client.Managers;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class InventoryMenu
    {
        UIMenu baseMenu;
        EventSystem EventSystem => EventSystem.GetModule();

        UIMenu menuBodyArmor;
        BodyArmorMenu _bodyArmorMenu = new BodyArmorMenu();

        UIMenu menuRepairKit;
        RepairKitMenu _repairKitMenu = new RepairKitMenu();

        UIMenu menuHealth;
        HealthMenu _healthMenu = new HealthMenu();

        PlayerOptionsManager playerOptionsManager = PlayerOptionsManager.GetModule();

        UIMenuCheckboxItem miScubaEquipment = new UIMenuCheckboxItem("Scuba Equipment", false);
        UIMenuCheckboxItem miWearHelmet = new UIMenuCheckboxItem("Wear Helmet", false);
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

            menuRepairKit = InteractionMenu.MenuPool.AddSubMenu(menu, "Repair Kits");
            _repairKitMenu.CreateMenu(menuRepairKit);

            baseMenu.AddItem(miScubaEquipment);
            baseMenu.AddItem(miWearHelmet);

            baseMenu.OnMenuStateChanged += BaseMenu_OnMenuStateChanged;
            baseMenu.OnCheckboxChange += BaseMenu_OnCheckboxChange;
        }

        private async void BaseMenu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            checkboxItem.Enabled = false;
            if (checkboxItem == miScubaEquipment)
            {
                playerOptionsManager.ToggleScubaEquipment();
            }

            if (checkboxItem == miWearHelmet)
            {
                SetPedHelmet(Cache.PlayerPed.Handle, Checked);
                Cache.Character.AllowHelmet = Checked;
            }
            await BaseScript.Delay(1000);
            checkboxItem.Enabled = true;
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

            miWearHelmet.Checked = Cache.Character.AllowHelmet;
        }
    }
}
