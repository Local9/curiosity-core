using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Events;
using Curiosity.Systems.Shared.Entity;
using NativeUI;
using System.Collections.Generic;

namespace Curiosity.MissionManager.Client.Menu.Submenu
{
    class MenuAssistanceRequesters
    {
        private PluginManager PluginInstance => PluginManager.Instance;
        private EventSystem eventSystem => EventSystem.GetModule();

        UIMenu Menu;

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

        }
    }
}
