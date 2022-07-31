using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.VehicleMods.SubMenu
{
    class VehicleExtrasSubMenu
    {
        UIMenu baseMenu;

        Dictionary<UIMenuCheckboxItem, int> vehicleExtras = new Dictionary<UIMenuCheckboxItem, int>();

        public void Create(UIMenu menu)
        {
            baseMenu = menu;
            baseMenu.MouseControlsEnabled = false;

            baseMenu.OnCheckboxChange += (sender, item, _checked) =>
            {
                if (vehicleExtras.TryGetValue(item, out int extra))
                {
                    Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
                    vehicle.ToggleExtra(extra, _checked);
                };
            };

            baseMenu.OnMenuStateChanged += (newMenu, oldMenu, state) =>
            {
                if (Equals(MenuState.Opened, state) || Equals(MenuState.ChangeForward, state))
                {
                    UpdateMenu();
                };
            };
        }

        void UpdateMenu()
        {
            baseMenu.Clear();
            vehicleExtras.Clear();

            Vehicle veh = Game.PlayerPed.CurrentVehicle;

            for (var extra = 0; extra < 20; extra++)
            {
                // If this extra exists...
                if (veh.ExtraExists(extra))
                {
                    UIMenuCheckboxItem extraCheckbox = new UIMenuCheckboxItem($"Extra #{extra}", veh.IsExtraOn(extra));
                    baseMenu.AddItem(extraCheckbox);

                    // Add it's ID to the dictionary.
                    vehicleExtras[extraCheckbox] = extra;
                }
            }
        }
    }
}
