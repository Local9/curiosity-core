using Curiosity.Core.Client.Interface.Menus.SubMenu.Inventory;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class InventoryMenu
    {
        UIMenu baseMenu;

        UIMenu menuBodyArmor;
        BodyArmorMenu _bodyArmorMenu = new BodyArmorMenu();
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
