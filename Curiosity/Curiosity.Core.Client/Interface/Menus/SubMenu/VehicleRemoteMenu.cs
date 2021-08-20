using CitizenFX.Core;
using CitizenFX.Core.Native;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class VehicleRemoteMenu
    {
        UIMenuCheckboxItem miEngine = new UIMenuCheckboxItem("Engine", false);
        UIMenuCheckboxItem miHeadlights = new UIMenuCheckboxItem("Headlights", false);
        UIMenuCheckboxItem miNeon = new UIMenuCheckboxItem("Neon Lights", false);
        private bool neonLights = false;
        private bool headlights = false;
        private UIMenu _menu;
        // UIMenuCheckboxItem miRadio = new UIMenuCheckboxItem("Engine", false);

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.AddItem(miEngine);
            menu.AddItem(miHeadlights);
            menu.AddItem(miNeon);

            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;
            menu.OnCheckboxChange += Menu_OnCheckboxChange;

            menu.MouseControlsEnabled = false;
            menu.MouseEdgeEnabled = false;

            _menu = menu;

            return menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeForward)
                OnMenuOpen();

            if (state == MenuState.ChangeBackward)
                _menu.InstructionalButtons.Clear();
        }

        private async void Menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            Vehicle vehicle = Cache.PersonalVehicle.Vehicle;

            if (Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed)
            {
                vehicle = Game.PlayerPed.CurrentVehicle;
            }

            if (checkboxItem == miEngine)
            {
                miEngine.Enabled = false;
                vehicle.IsEngineRunning = Checked;
                await BaseScript.Delay(1000);
                miEngine.Enabled = true;
                return;
            }

            if (checkboxItem == miHeadlights)
            {
                miHeadlights.Enabled = false;
                vehicle.AreLightsOn = Checked;
                await BaseScript.Delay(1000);
                miHeadlights.Enabled = true;
                return;
            }

            if (checkboxItem == miNeon)
            {
                miHeadlights.Enabled = false;

                ToggleNeonLights(vehicle, Checked);
                await BaseScript.Delay(1000);
                miHeadlights.Enabled = true;
                return;
            }
        }

        private void OnMenuOpen()
        {
            if (Cache.PersonalVehicle == null)
            {
                miEngine.Enabled = false;
                miHeadlights.Enabled = false;
                miNeon.Enabled = false;
            }
            else
            {
                Vehicle vehicle = Cache.PersonalVehicle.Vehicle;

                if (Game.PlayerPed.IsInVehicle())
                {
                    vehicle = Game.PlayerPed.CurrentVehicle;
                }

                miEngine.Checked = vehicle.IsEngineRunning;
                miEngine.Enabled = true;

                miHeadlights.Checked = Cache.PlayerPed.IsInVehicle() ? vehicle.AreLightsOn : headlights;
                miHeadlights.Enabled = true;

                miNeon.Checked = neonLights;
                miNeon.Enabled = AreNeonsEnabled(vehicle);
            }
        }

        private bool AreNeonsEnabled(Vehicle vehicle)
        {
            int handle = vehicle.Handle;
            return API.IsVehicleNeonLightEnabled(handle, 0)
                || API.IsVehicleNeonLightEnabled(handle, 1)
                || API.IsVehicleNeonLightEnabled(handle, 2)
                || API.IsVehicleNeonLightEnabled(handle, 3);
        }

        private void ToggleNeonLights(Vehicle vehicle, bool enabled)
        {
            neonLights = enabled;
            API.DisableVehicleNeonLights(vehicle.Handle, enabled);
        }
    }
}
