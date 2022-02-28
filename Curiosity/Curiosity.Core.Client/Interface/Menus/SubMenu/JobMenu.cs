using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using NativeUI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class JobMenu
    {
        private UIMenu jobMenu;
        UIMenuListItem uiLstJobs;

        EventSystem EventSystem => EventSystem.GetModule();

        PlayerOptionsManager playerOptionsManager = PlayerOptionsManager.GetModule();

        private UIMenu menuPolice;
        private SubMenu.PoliceMenu _MenuPolice = new SubMenu.PoliceMenu();

        public UIMenu CreateMenu(UIMenu menu)
        {
            jobMenu = menu;

            List<dynamic> list = ConfigurationManager.GetModule().Jobs().Cast<dynamic>().ToList();

            uiLstJobs = new UIMenuListItem("Job", list, 0);
            uiLstJobs.Description = "Press ENTER when you want to select a job. Selecting the same item will toggle.";
            jobMenu.AddItem(uiLstJobs);

            menuPolice = InteractionMenu.MenuPool.AddSubMenu(jobMenu, "Police Options");
            _MenuPolice.CreateMenu(menuPolice);

            jobMenu.OnListSelect += Menu_OnListSelect;
            jobMenu.OnMenuStateChanged += JobMenu_OnMenuStateChanged;

            return menu;
        }

        private void JobMenu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            uiLstJobs.Enabled = true;
            uiLstJobs.Description = "Press ENTER when you want to select a job. Selecting the same item will toggle.";

            if (playerOptionsManager.IsPassiveModeCooldownEnabled)
            {
                uiLstJobs.Enabled = false;
                uiLstJobs.Description = "Passive Mode cooldown is active, cannot swap jobs.";
            }

            if (playerOptionsManager.IsPassive)
            {
                uiLstJobs.Enabled = false;
                uiLstJobs.Description = "Passive Mode is active, cannot swap jobs.";
            }

            if (state == MenuState.Opened || state == MenuState.ChangeForward)
            {
                menuPolice.ParentItem.Enabled = (playerOptionsManager.CurrentJob == ePlayerJobs.POLICE_OFFICER);
            }
        }

        private async void Menu_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (playerOptionsManager.IsPassiveModeCooldownEnabled) return;
            if (playerOptionsManager.IsPassive) return;

            listItem.Enabled = false;
            menuPolice.ParentItem.Enabled = false;

            if (listItem == uiLstJobs)
            {
                Job job = (Job)listItem.Items[newIndex];

                Logger.Debug($"{JsonConvert.SerializeObject(job)}");

                if (job.LegacyEvent)
                {
                    BaseScript.TriggerEvent(job.JobEvent);
                    goto EndMethod;
                }

                EventSystem.Send(job.JobEvent);

            EndMethod:
                await BaseScript.Delay(1000);
                listItem.Enabled = true;

                menuPolice.ParentItem.Enabled = (playerOptionsManager.CurrentJob == ePlayerJobs.POLICE_OFFICER);
                if (menuPolice.ParentItem.Enabled)
                {
                    InteractionMenu.MenuPool.CloseAllMenus();
                    Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CHARACTER_WHEEL~ + ~INPUT_CONTEXT~ as a shortcut to the Police Menu.");
                    _MenuPolice.Init();
                }
                else
                {
                    _MenuPolice.Dispose();
                }
            }
        }
    }
}
