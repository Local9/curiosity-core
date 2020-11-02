using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Shared.Client.net.Extensions;
using NativeUI;
using System.Threading.Tasks;


using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client.Menu.Submenu
{
    class SuspectVehicle
    {
        private PluginManager PluginInstance => PluginManager.Instance;
        private Vehicle vehicle;

        UIMenu Menu;
        UIMenuItem menuItemSearchVehicle;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuItemSearchVehicle = new UIMenuItem("Search");
            menu.AddItem(menuItemSearchVehicle);

            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            Menu = menu;
            return menu;
        }

        private void Menu_OnMenuClose(UIMenu sender)
        {
            MenuBase.OnMenuState();
            PluginInstance.DeregisterTickHandler(OnSuspectVehicleDistanceCheck);
        }

        private void Menu_OnMenuOpen(UIMenu sender)
        {
            bool isCalloutActive = MenuBase.IsCalloutActive;

            if (!isCalloutActive)
            {
                Menu.MenuItems.ForEach(m => m.Enabled = false);
            }
            else
            {
                vehicle = MenuBase.GetClosestVehicle();

                if (vehicle == null)
                {
                    UiTools.Dispatch("No vehicle nearby", $"");
                    MenuBase._MenuPool.CloseAllMenus();
                    Menu.MenuItems.ForEach(m => m.Enabled = false);
                    return;
                }

                menuItemSearchVehicle.Enabled = true;

                PluginInstance.RegisterTickHandler(OnSuspectVehicleDistanceCheck);
            }

            MenuBase.OnMenuState(true);
        }

        private async Task OnSuspectVehicleDistanceCheck()
        {
            if (vehicle.Position.Distance(Game.PlayerPed.Position) > 3f)
                MenuBase._MenuPool.CloseAllMenus();
        }

        private async void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == menuItemSearchVehicle)
            {
                vehicle.Sequence(Vehicle.VehicleSequence.SEARCH);
                await BaseScript.Delay(500);
                return;
            }
        }
    }
}
