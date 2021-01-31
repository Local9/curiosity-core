using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class VehicleMenu
    {
        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.OnMenuOpen += Menu_OnMenuOpen;

            return menu;
        }

        private void Menu_OnMenuOpen(UIMenu menu)
        {
            menu.Clear();


        }
    }
}
