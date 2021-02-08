using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class VehicleMenu
    {
        private UIMenu menuVehicleRemote;
        private VehicleRemoteMenu _VehicleRemoteMenu = new VehicleRemoteMenu();

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.OnMenuOpen += Menu_OnMenuOpen;

            _VehicleRemoteMenu.CreateMenu(menuVehicleRemote);
            menuVehicleRemote = InteractionMenu.MenuPool.AddSubMenu(menu, "Vehicle Remote Functions");

            return menu;
        }

        private void Menu_OnMenuOpen(UIMenu menu)
        {

        }
    }
}
