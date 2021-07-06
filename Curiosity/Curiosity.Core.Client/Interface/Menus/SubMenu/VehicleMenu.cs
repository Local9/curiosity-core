using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class VehicleMenu
    {
        private UIMenu menuVehicleRemote;
        private VehicleRemoteMenu _VehicleRemoteMenu = new VehicleRemoteMenu();

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuVehicleRemote = InteractionMenu.MenuPool.AddSubMenu(menu, "Vehicle Remote Functions");
            _VehicleRemoteMenu.CreateMenu(menuVehicleRemote);

            menu.MouseControlsEnabled = false;
            menu.MouseEdgeEnabled = false;

            return menu;
        }

    }
}
