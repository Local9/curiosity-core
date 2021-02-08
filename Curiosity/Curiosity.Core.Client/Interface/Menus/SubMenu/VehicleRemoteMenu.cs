using CitizenFX.Core;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class VehicleRemoteMenu
    {
        UIMenuCheckboxItem miEngine;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnCheckboxChange += Menu_OnCheckboxChange;

            return menu;
        }

        private async void Menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            if (checkboxItem == miEngine)
            {
                miEngine.Enabled = false;
                Cache.PersonalVehicle.IsEngineRunning = Checked;

                await BaseScript.Delay(1000);

                miEngine.Enabled = true;

                return;
            }
        }

        private void Menu_OnMenuOpen(UIMenu menu)
        {
            menu.Clear();
            miEngine = new UIMenuCheckboxItem("Engine", false);

            if (Cache.PersonalVehicle == null)
            {
                miEngine.Enabled = false;
            }
            else
            {
                miEngine.Checked = Cache.PersonalVehicle.IsEngineRunning;
                miEngine.Enabled = true;
            }
        }
    }
}
