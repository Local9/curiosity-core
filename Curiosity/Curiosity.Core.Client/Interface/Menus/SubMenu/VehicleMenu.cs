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

            return menu;
        }

        private void Menu_OnMenuOpen(UIMenu menu)
        {
            menu.Clear();
            _VehicleRemoteMenu.CreateMenu(menuVehicleRemote);
            menuVehicleRemote = InteractionMenu.MenuPool.AddSubMenu(menu, "Vehicle Remote Functions");
        }
    }
}
