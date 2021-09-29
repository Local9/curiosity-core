using Curiosity.Core.Client.Managers;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu.SettingsSubMenu
{
    public class DurationItem
    {
        public int Duration;
        public int DurationMillis => Duration * 1000;

        public DurationItem(int seconds)
        {
            Duration = seconds;
        }

        public override string ToString()
        {
            return $"{Duration} seconds";
        }
    }

    class PoliceSettingsMenu
    {
        const string PASSIVE_MODE_ENABLED = "Disabled while passive mode is enabled.";

        static List<dynamic> lstDuration = new List<dynamic>()
        {
            new DurationItem(10),
            new DurationItem(20),
            new DurationItem(30),
            new DurationItem(40),
            new DurationItem(50),
            new DurationItem(60),
            new DurationItem(70),
            new DurationItem(80),
            new DurationItem(90),
            new DurationItem(100),
            new DurationItem(110),
            new DurationItem(120),
            new DurationItem(130),
            new DurationItem(140),
            new DurationItem(150),
            new DurationItem(160),
            new DurationItem(170),
            new DurationItem(180),
        };

        UIMenuCheckboxItem chkDisplayDispatchUI = new UIMenuCheckboxItem("Show UI", true);
        UIMenuCheckboxItem chkDisplayNumberOfRemainingPolice = new UIMenuCheckboxItem("Show Remaining Police", false);
        UIMenuCheckboxItem chkResetCopsWhenCleared = new UIMenuCheckboxItem("Reset to base on Clear", false);
        UIMenuCheckboxItem chkRemoveCopsWhenFarAway = new UIMenuCheckboxItem("Remove distance Police", true);
        UIMenuCheckboxItem chkRemoveCopsWhenFarAwayChase = new UIMenuCheckboxItem("Remove Police in Chase", true);
        UIMenuCheckboxItem chkClearWantedLevelWhenClear = new UIMenuCheckboxItem("Clear wanted when clear", true);
        UIMenuSeparatorItem UIMenuSeparatorItem = new UIMenuSeparatorItem();
        UIMenuCheckboxItem chkBackupEnabled = new UIMenuCheckboxItem("Enable Backup", false);
        UIMenuCheckboxItem chkBackupPauseWhenSearching = new UIMenuCheckboxItem("Pause Backup on Search", false);
        UIMenuListItem lstBackupDuration = new UIMenuListItem("Backup Duration", lstDuration, 5);
        UIMenuListItem lstBackupCooldown = new UIMenuListItem("Backup Cooldown", lstDuration, 0);

        public UIMenu CreateMenu(UIMenu menu)
        {
            chkResetCopsWhenCleared.Enabled = false;
            chkResetCopsWhenCleared.Description = "Currently disabled.";
            menu.AddItem(chkResetCopsWhenCleared);

            menu.AddItem(chkDisplayDispatchUI);
            menu.AddItem(chkDisplayNumberOfRemainingPolice);
            menu.AddItem(chkRemoveCopsWhenFarAway);
            menu.AddItem(chkRemoveCopsWhenFarAwayChase);
            menu.AddItem(chkClearWantedLevelWhenClear);
            menu.AddItem(UIMenuSeparatorItem);
            menu.AddItem(chkBackupEnabled);
            menu.AddItem(chkBackupPauseWhenSearching);
            menu.AddItem(lstBackupDuration);
            menu.AddItem(lstBackupCooldown);

            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;
            menu.OnCheckboxChange += Menu_OnCheckboxChange;
            menu.OnListChange += Menu_OnListChange;

            return menu;
        }

        private void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            DispatchManager dispatchManager = DispatchManager.GetModule();

            if (listItem == lstBackupDuration)
            {
                dispatchManager.BackupDuration = lstDuration[newIndex].DurationMillis;
            }

            if (listItem == lstBackupCooldown)
            {
                dispatchManager.BackupCooldown = lstDuration[newIndex].DurationMillis;
            }
        }

        private void Menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            DispatchManager dispatchManager = DispatchManager.GetModule();

            if (checkboxItem == chkDisplayDispatchUI)
                dispatchManager.DisplayDispatchUI = Checked;

            if (checkboxItem == chkDisplayNumberOfRemainingPolice)
                dispatchManager.DisplayNumberOfRemainingPolice = Checked;

            if (checkboxItem == chkRemoveCopsWhenFarAway)
                dispatchManager.RemoveCopsWhenFarAway = Checked;

            if (checkboxItem == chkRemoveCopsWhenFarAwayChase)
                dispatchManager.RemoveCopsWhenFarAwayChase = Checked;

            if (checkboxItem == chkClearWantedLevelWhenClear)
                dispatchManager.ClearWantedLevelWhenClear = Checked;

            if (checkboxItem == chkBackupEnabled)
                dispatchManager.BackupEnabled = Checked;

            if (checkboxItem == chkBackupPauseWhenSearching)
                dispatchManager.BackupPauseWhenSearching = Checked;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeForward || state == MenuState.Opened)
            {
                bool isPassive = PlayerOptionsManager.GetModule().IsPassive;
                DispatchManager dispatchManager = DispatchManager.GetModule();

                chkDisplayDispatchUI.Checked = dispatchManager.DisplayDispatchUI;
                chkDisplayDispatchUI.Enabled = !isPassive;
                chkDisplayDispatchUI.Description = !isPassive ? "Show UI." : PASSIVE_MODE_ENABLED;

                chkDisplayNumberOfRemainingPolice.Checked = dispatchManager.DisplayNumberOfRemainingPolice;
                chkDisplayNumberOfRemainingPolice.Enabled = !isPassive;
                chkDisplayNumberOfRemainingPolice.Description = !isPassive ? "Show number of remaining police." : PASSIVE_MODE_ENABLED;

                chkRemoveCopsWhenFarAway.Checked = dispatchManager.RemoveCopsWhenFarAway;
                chkRemoveCopsWhenFarAway.Enabled = !isPassive;
                chkRemoveCopsWhenFarAway.Description = !isPassive ? "Decrease number of police when evading." : PASSIVE_MODE_ENABLED;

                chkRemoveCopsWhenFarAwayChase.Checked = dispatchManager.RemoveCopsWhenFarAwayChase;
                chkRemoveCopsWhenFarAwayChase.Enabled = !isPassive;
                chkRemoveCopsWhenFarAwayChase.Description = !isPassive ? "Decrease number of police when evading in a car chase." : PASSIVE_MODE_ENABLED;

                chkClearWantedLevelWhenClear.Checked = dispatchManager.ClearWantedLevelWhenClear;
                chkClearWantedLevelWhenClear.Enabled = !isPassive;
                chkClearWantedLevelWhenClear.Description = !isPassive ? "Clear wanted level when all police are cleared." : PASSIVE_MODE_ENABLED;

                chkBackupEnabled.Checked = dispatchManager.BackupEnabled;
                chkBackupEnabled.Enabled = !isPassive;
                chkBackupEnabled.Description = !isPassive ? "Enable police backup." : PASSIVE_MODE_ENABLED;

                chkBackupPauseWhenSearching.Checked = dispatchManager.BackupPauseWhenSearching;
                chkBackupPauseWhenSearching.Enabled = !isPassive;
                chkBackupPauseWhenSearching.Description = !isPassive ? "Pause backup progress when searching." : PASSIVE_MODE_ENABLED;

                lstBackupDuration.Index = GetIndex(dispatchManager.BackupDuration);
                lstBackupDuration.Enabled = !isPassive;
                lstBackupDuration.Description = !isPassive ? "Time it takes for backup to complete." : PASSIVE_MODE_ENABLED;

                lstBackupCooldown.Index = GetIndex(dispatchManager.BackupCooldown);
                lstBackupCooldown.Enabled = !isPassive;
                lstBackupCooldown.Description = !isPassive ? "Cooldown before next backup check." : PASSIVE_MODE_ENABLED;
            }
        }

        int GetIndex(int mills)
        {
            for(int i = 0; i < lstDuration.Count; i++)
            {
                DurationItem d = lstDuration[i];

                if (d.DurationMillis == mills)
                    return i;
            }
            return 0;
        }
    }
}
