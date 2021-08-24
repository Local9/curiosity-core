using Curiosity.Core.Client.Events;
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

        UIMenu menuHealth;
        HealthMenu _healthMenu = new HealthMenu();

        PlayerOptionsManager playerOptionsManager = PlayerOptionsManager.GetModule();

        UIMenuCheckboxItem miScubaEquipment = new UIMenuCheckboxItem("Scuba Equipment", false);
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

            baseMenu.OnMenuStateChanged += BaseMenu_OnMenuStateChanged;
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
        }
    }
}
