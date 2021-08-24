using Curiosity.Core.Client.Interface.Menus.SubMenu.Inventory;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class InventoryMenu
    {
        UIMenu baseMenu;

        UIMenu menuBodyArmor;
        BodyArmorMenu _bodyArmorMenu = new BodyArmorMenu();

        UIMenu menuHealth;
        HealthMenu _healthMenu = new HealthMenu();
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
            
        }

        private void UpdateMenuItems()
        {
            
        }
    }
}
