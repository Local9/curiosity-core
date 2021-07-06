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

            return menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.Opened)
                OnMenuOpen();
        }

        private async void Menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            if (checkboxItem == miEngine)
            {
                miEngine.Enabled = false;
                Cache.PersonalVehicle.Vehicle.IsEngineRunning = Checked;
                await BaseScript.Delay(1000);
                miEngine.Enabled = true;
                return;
            }

            if (checkboxItem == miHeadlights)
            {
                miHeadlights.Enabled = false;
                Cache.PersonalVehicle.Vehicle.AreLightsOn = Checked;
                await BaseScript.Delay(1000);
                miHeadlights.Enabled = true;
                return;
            }

            if (checkboxItem == miNeon)
            {
                miHeadlights.Enabled = false;

                ToggleNeonLights(Checked);
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
                miEngine.Checked = Cache.PersonalVehicle.Vehicle.IsEngineRunning;
                miEngine.Enabled = true;

                miHeadlights.Checked = Cache.PlayerPed.IsInVehicle() ? Cache.PersonalVehicle.Vehicle.AreLightsOn : headlights;
                miHeadlights.Enabled = true;

                miNeon.Checked = neonLights;
                miNeon.Enabled = AreNeonsEnabled();
            }
        }

        private bool AreNeonsEnabled()
        {
            int handle = Cache.PersonalVehicle.Vehicle.Handle;
            return API.IsVehicleNeonLightEnabled(handle, 0)
                || API.IsVehicleNeonLightEnabled(handle, 1)
                || API.IsVehicleNeonLightEnabled(handle, 2)
                || API.IsVehicleNeonLightEnabled(handle, 3);
        }

        private void ToggleNeonLights(bool enabled)
        {
            neonLights = enabled;
            API.DisableVehicleNeonLights(Cache.PersonalVehicle.Vehicle.Handle, enabled);
        }
    }
}
