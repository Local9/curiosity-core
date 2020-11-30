using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Shared.Entity;
using NativeUI;
using System.Collections.Generic;

namespace Curiosity.MissionManager.Client.Menu.Submenu
{
    class MenuAssistanceRequesters
    {
        private PluginManager Instance => PluginManager.Instance;
        private EventSystem eventSystem => EventSystem.GetModule();

        UIMenu Menu;

        Dictionary<UIMenuItem, MissionData> menuMissions = new Dictionary<UIMenuItem, MissionData>();

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            Menu = menu;
            return menu;
        }

        private void Menu_OnMenuClose(UIMenu sender)
        {
            MenuManager.OnMenuState();
        }

        private async void Menu_OnMenuOpen(UIMenu sender)
        {
            Menu.MenuItems.Clear();
            menuMissions.Clear();

            bool isCalloutActive = MenuManager.IsCalloutActive;
            List<MissionData> missions = await eventSystem.Request<List<MissionData>>("mission:assistance:list");

            Logger.Debug($"missions: {missions.Count}");

            if (missions?.Count == 0)
            {
                UIMenuItem uIMenuItem = new UIMenuItem($"No requests");
                uIMenuItem.Enabled = false;
                uIMenuItem.Description = "No back up requests active.";
                Menu.AddItem(uIMenuItem);
                return;
            }

            missions.ForEach(mis =>
            {
                Player player = PluginManager.PlayerList[mis.OwnerHandleId];

                Logger.Debug($"Mission: {mis.OwnerHandleId}, {mis.ID}, {mis.Creation} / PlayerID: {player?.Name}#{player?.Handle}");
                
                UIMenuItem uIMenuItem = new UIMenuItem($"{player?.Name}");

                menuMissions.Add(uIMenuItem, mis);

                Menu.AddItem(uIMenuItem);
            });

            if (isCalloutActive)
            {
                Menu.MenuItems.ForEach(m => {
                    m.Enabled = false;
                    m.Description = "~o~Currently on an active callout.";
                });
            }

            MenuManager.OnMenuState(true);
        }

        private async void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (menuMissions.ContainsKey(selectedItem))
            {
                MissionData missionData = menuMissions[selectedItem];

                Logger.Debug($"Selected Response: {missionData}");

                MissionData response = await eventSystem.Request<MissionData>("mission:assistance:accept", missionData.OwnerHandleId);

                if (response != null)
                {
                    MissionDirectorManager.Director.ToggleMissionDirector();

                    if (Mission.currentMission != null)
                    {
                        Mission.currentMission.Stop(EndState.ForceEnd);
                    }

                    await BaseScript.Delay(500);
                    Mission.currentMissionData = response;
                    await BaseScript.Delay(500);
                    Mission.AttachMissionUpdateTick();
                }
            }
        }
    }
}
