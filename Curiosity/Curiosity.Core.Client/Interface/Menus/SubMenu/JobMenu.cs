using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class JobMenu
    {
        private UIMenu jobMenu;
        UIMenuListItem uiLstJobs;

        EventSystem EventSystem = EventSystem.GetModule();

        public UIMenu CreateMenu(UIMenu menu)
        {
            jobMenu = menu;

            List<dynamic> list = ConfigurationManager.GetModule().Jobs().Cast<dynamic>().ToList();

            uiLstJobs = new UIMenuListItem("Job", list, 0);
            jobMenu.AddItem(uiLstJobs);

            jobMenu.OnListSelect += Menu_OnListSelect;

            return menu;
        }

        private async void Menu_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            listItem.Enabled = false;
            if (listItem == uiLstJobs)
            {
                Job job = (Job)listItem.Items[newIndex];

                if (job.LegacyEvent)
                {
                    BaseScript.TriggerEvent(job.JobEvent);
                    goto EndMethod;
                }

                EventSystem.Send(job.JobEvent);

            EndMethod:
                await BaseScript.Delay(1000);
                listItem.Enabled = true;
                Logger.Debug($"Job: {job.Label}");
            }
        }
    }
}
