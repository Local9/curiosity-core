using CitizenFX.Core;
using Curiosity.MissionManager.Client.Managers;
using NativeUI;

namespace Curiosity.MissionManager.Client.Menu.Submenu
{
    class MenuSettings
    {
        private PluginManager PluginInstance => PluginManager.Instance;

        UIMenu Menu;
        UIMenuCheckboxItem menuCheckboxEnableCallouts;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuCheckboxEnableCallouts = new UIMenuCheckboxItem("Callouts", MissionDirectorManager.MissionDirectorState, "If enabled ~b~Dispatch A.I.~s~ will assign random callouts.");
            menu.AddItem(menuCheckboxEnableCallouts);

            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            Menu = menu;
            return menu;
        }

        private void Menu_OnMenuClose(UIMenu sender)
        {
            MenuBase.OnMenuState();
        }

        private void Menu_OnMenuOpen(UIMenu sender)
        {
            

            MenuBase.OnMenuState(true);
        }

        private async void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == menuCheckboxEnableCallouts)
            {
                menuCheckboxEnableCallouts.Enabled = false;
                MissionDirectorManager.ToggleMissionDirector();
                await BaseScript.Delay(100);
                menuCheckboxEnableCallouts.Checked = MissionDirectorManager.MissionDirectorState;
                await BaseScript.Delay(500);
                menuCheckboxEnableCallouts.Enabled = true;
                return;
            }
        }
    }
}
