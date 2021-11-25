using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class VehicleGarageMenu
    {
        UIMenu baseMenu;
        
        internal void CreateMenu(UIMenu menu)
        {
            baseMenu = menu;

            baseMenu.OnItemSelect += BaseMenu_OnItemSelect;
        }

        private void BaseMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            
        }
    }
}
