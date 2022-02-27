using CitizenFX.Core;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Utils;
using NativeUI;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    internal class PoliceMenu
    {
        private UIMenu menu;
        EventSystem EventSystem => EventSystem.GetModule();

        private UIMenu playerListMenu;
        private PoliceSubMenu.PolicePlayerListMenu _playerListMenu = new PoliceSubMenu.PolicePlayerListMenu();

        UIMenuItem miRequestBackup = new UIMenuItem("10-78: Need Assistance", "This will call on other players for assistance.");
        UIMenuCheckboxItem miDisableNotifications = new UIMenuCheckboxItem("Notifications", true, "Toggle Notifications");

        private UIMenu menuAssistanceRequesters;
        private PoliceSubMenu.PoliceBackupMenu _policeBackupMenu = new PoliceSubMenu.PoliceBackupMenu();

        const string COMMAND_BACKUP = "lv_police_request_backup";

        JobManager jobManager => JobManager.GetModule();
        PlayerOptionsManager playerOptionsManager => PlayerOptionsManager.GetModule();

        public UIMenu CreateMenu(UIMenu m)
        {
            menu = m;

            playerListMenu = InteractionMenu.MenuPool.AddSubMenu(m, "Nearby Players");
            _playerListMenu.CreateMenu(playerListMenu);

            menu.AddItem(miDisableNotifications);

            //RegisterKeyMapping(COMMAND_BACKUP, "POLICE: Request Backup", "keyboard", "");
            //RegisterCommand(COMMAND_BACKUP, new Action(OnRequestBackup), false);
            //menu.AddItem(miRequestBackup);

            //menuAssistanceRequesters = InteractionMenu.MenuPool.AddSubMenu(m, "Respond to Backup", "Users requesting back up will be found here.");
            //_policeBackupMenu.CreateMenu(menuAssistanceRequesters);

            menu.OnItemSelect += Menu_OnItemSelect;

            return menu;
        }

        private async void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == miRequestBackup)
                OnRequestBackup();
            else if (selectedItem == miDisableNotifications)
            {
                bool res = await EventSystem.Request<bool>("police:report:notification:toggle");
                if (res)
                    Notify.Success($"Police Notification State has been changed");
            }
        }

        private void OnRequestBackup()
        {
            EventSystem.Request<bool>("mission:assistance:request");
            Notify.DispatchAI("Back Up Requested", "We have informed all available officers that you have requested back up at your location.");
        }

        public void Init()
        {
            PluginManager.Instance.AttachTickHandler(CheckControls);
        }

        public void Dispose()
        {
            PluginManager.Instance.DetachTickHandler(CheckControls);
        }

        private async Task CheckControls()
        {
            if (!jobManager.IsOfficer || playerOptionsManager.IsWanted)
            {
                Dispose();
                return;
            }

            if (!Game.IsPaused
                && !IsPauseMenuRestarting()
                && IsScreenFadedIn()
                && !IsPlayerSwitchInProgress()
                && Cache.Character.MarkedAsRegistered)
            {
                if (InteractionMenu.MenuPool.IsAnyMenuOpen() && menu.Visible)
                {
                    if (ControlHelper.IsControlJustPressed(Control.Context, true, ControlModifier.Alt))
                    {
                        menu.Visible = false;
                        InteractionMenu.MenuPool.CloseAllMenus();
                    }
                }
                else if (!menu.Visible)
                {
                    if (ControlHelper.IsControlJustPressed(Control.Context, true, ControlModifier.Alt))
                    {
                        menu.Visible = true;
                    }
                }
            }
        }
    }
}
