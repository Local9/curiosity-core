using CitizenFX.Core;
using Curiosity.MissionManager.Client.Events;
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
        UIMenuCheckboxItem menuCheckboxEnableNotificationsBackup;
        UIMenuListItem menuListItemPatrolZone;

        UIMenuCheckboxItem menuChkNpcDebug; // 3
        UIMenuCheckboxItem menuChkVehDebug; // 4

        static new List<dynamic> notificationList = new List<dynamic>() { "Success", "Info", "Warning", "Error", "Show" };
        UIMenuListItem menuListNotification = new UIMenuListItem("Notifications", notificationList, 0);

        int patrolZoneIndex = 0;
        List<dynamic> lst = new List<dynamic>() { "City", "County" };

        bool notificationBackup = false;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuCheckboxEnableCallouts = new UIMenuCheckboxItem("Callouts", MissionDirectorManager.MissionDirectorState, "If enabled ~b~Dispatch A.I.~s~ will assign random callouts.");
            menu.AddItem(menuCheckboxEnableCallouts);

            menuCheckboxEnableNotificationsBackup = new UIMenuCheckboxItem("Back Up Notifications", notificationBackup, "Show Notifications when a player is requesting back up");
            menu.AddItem(menuCheckboxEnableNotificationsBackup);

            menuListItemPatrolZone = new UIMenuListItem("Patrol Zone", lst, patrolZoneIndex, "This will change the area of missions provided by ~b~Dispatch A.I.");
            menu.AddItem(menuListItemPatrolZone);

            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnListChange += Menu_OnListChange;
            menu.OnCheckboxChange += Menu_OnCheckboxChange;
            menu.OnListSelect += Menu_OnListSelect;

            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;

            Menu = menu;
            return menu;
        }

        private void Menu_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == menuListNotification)
            {
                string notification = (string)notificationList[newIndex];
                switch(notification)
                {
                    case "Success":
                        string message = "<span>Dispatch AI</span>" +
                            "<table class=\"w-100\">" +
                            "<tr>" +
                            "<td class=\"text-left\">Mission</td>" +
                            "<td class=\"text-right\">Example</td>" +
                            "</tr>" +
                            "<tr>" +
                            "<td class=\"text-left\">Reward</td>" +
                            "<td class=\"text-right\">$0</td>" +
                            "</tr>" +
                            "<tr>" +
                            "<td class=\"text-left\">XP Gain</td>" +
                            "<td class=\"text-right\">0xp</td>" +
                            "</tr>" +
                            "<tr>" +
                            "<td class=\"text-left\">Reputation Gain</td>" +
                            "<td class=\"text-right\">0</td>" +
                            "</tr>" +
                            "</table>";
                        Notify.Success(message, "bottom-center", "snackbar");
                        break;
                    case "Info":
                        Notify.Info("Test Info", "bottom-center", "toast");
                        break;
                    case "Warning":
                        Notify.Warning("Test Warning", "bottom-center", "toast");
                        break;
                    case "Error":
                        Notify.Error("Test Error", "bottom-center", "toast");
                        break;
                    case "Show":
                        Notify.Show("Test Show", "bottom-center", "toast");
                        break;
                }
            }
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeBackward)
                MenuManager.OnMenuState();

            if (state == MenuState.ChangeForward)
                OnMenuOpen(newMenu);
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

                Notify.Info($"~g~New Zone~w~: {JobManager.PatrolZone}");
            }

            await BaseScript.Delay(1000);
            listItem.Enabled = true;
        }

        private async void Menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            checkboxItem.Enabled = false;

            if (checkboxItem == menuCheckboxEnableCallouts)
            {
                MissionDirectorManager.Director.ToggleMissionDirector();
                await BaseScript.Delay(100);
                menuCheckboxEnableCallouts.Checked = MissionDirectorManager.MissionDirectorState;
                await BaseScript.Delay(1000);
            }

            if (checkboxItem == menuCheckboxEnableNotificationsBackup)
            {
                notificationBackup = Checked;
                bool setup = await EventSystem.GetModule().Request<bool>("user:job:notification:backup", notificationBackup);

                if (setup)
                {
                    Notify.Success($"Accepting Backup Notifications");
                }
                else
                {
                    Notify.Info($"Disabled Backup Notifications");
                }
            }

            if (checkboxItem == menuChkNpcDebug)
                Cache.PlayerPed.State.Set(StateBagKey.PLAYER_DEBUG_NPC, Checked, true);

            if (checkboxItem == menuChkVehDebug)
                Cache.PlayerPed.State.Set(StateBagKey.PLAYER_DEBUG_VEH, Checked, true);

            await BaseScript.Delay(500);
            checkboxItem.Enabled = true;
        }

        private void OnMenuOpen(UIMenu sender)
        {
            if (Cache.Player.User.IsDeveloper)
            {
                if (menuChkNpcDebug == null)
                {
                    menuChkNpcDebug = new UIMenuCheckboxItem("NPC Debug UI", false);
                    sender.AddItem(menuChkNpcDebug);
                    menuChkVehDebug = new UIMenuCheckboxItem("Vehicle Debug UI", false);
                    sender.AddItem(menuChkVehDebug);
                    sender.AddItem(menuListNotification);
                }
            }
            else
            {
                if (menuChkNpcDebug != null)
                {
                    sender.RemoveItemAt(3);
                    sender.RemoveItemAt(4);
                    sender.RemoveItemAt(5);
                }
            }

            MenuManager.OnMenuState(true);

            menuCheckboxEnableCallouts.Checked = MissionDirectorManager.MissionDirectorState;
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
