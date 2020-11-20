using CitizenFX.Core;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.Systems.Library.Enums;
using NativeUI;
using System.Collections.Generic;

namespace Curiosity.MissionManager.Client.Menu.Submenu
{
    class MenuSettings
    {
        private PluginManager PluginInstance => PluginManager.Instance;

        UIMenu Menu;
        UIMenuCheckboxItem menuCheckboxEnableCallouts;
        UIMenuListItem menuListItemPatrolZone;

        int patrolZoneIndex = 0;
        List<dynamic> lst = new List<dynamic>() { "City", "County" };

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuCheckboxEnableCallouts = new UIMenuCheckboxItem("Callouts", MissionDirectorManager.MissionDirectorState, "If enabled ~b~Dispatch A.I.~s~ will assign random callouts.");
            menu.AddItem(menuCheckboxEnableCallouts);

            menuListItemPatrolZone = new UIMenuListItem("Patrol Zone", lst, patrolZoneIndex, "This will change the area of missions provided by ~b~Dispatch A.I.");
            menu.AddItem(menuListItemPatrolZone);

            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnListChange += Menu_OnListChange;
            menu.OnCheckboxChange += Menu_OnCheckboxChange;
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            Menu = menu;
            return menu;
        }

        private async void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            listItem.Enabled = false;

            if (listItem == menuListItemPatrolZone)
            {
                patrolZoneIndex = newIndex;
                switch (listItem.Items[newIndex].ToString())
                {
                    case "City":
                        JobManager.PatrolZone = PatrolZone.City;
                        break;
                    case "County":
                        JobManager.PatrolZone = PatrolZone.County;
                        break;
                }

                Notify.DispatchAI($"~b~Patrol Zone Updated", $"~g~~h~New Zone~h~~w~: {JobManager.PatrolZone}");
            }

            await BaseScript.Delay(1000);
            listItem.Enabled = true;
        }

        private async void Menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            checkboxItem.Enabled = false;

            if (checkboxItem == menuCheckboxEnableCallouts)
            {
                MissionDirectorManager.ToggleMissionDirector();
                await BaseScript.Delay(100);
                menuCheckboxEnableCallouts.Checked = MissionDirectorManager.MissionDirectorState;
                await BaseScript.Delay(1000);
            }

            await BaseScript.Delay(500);
            checkboxItem.Enabled = true;
        }

        private void Menu_OnMenuClose(UIMenu sender)
        {
            MenuManager.OnMenuState();
        }

        private void Menu_OnMenuOpen(UIMenu sender)
        {
            MenuManager.OnMenuState(true);
        }

        private void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {

        }
    }

    class PatrolListItem
    {
        public string Description;
    }
}
