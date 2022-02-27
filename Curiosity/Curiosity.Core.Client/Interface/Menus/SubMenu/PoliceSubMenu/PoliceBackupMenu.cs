using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Events;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu.PoliceSubMenu
{
    internal class PoliceBackupMenu
    {
        private EventSystem eventSystem => EventSystem.GetModule();
        UIMenu _menu;

        Dictionary<UIMenuItem, int> playersRequestingBackup = new();

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;

            _menu = menu;
            return _menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeForward)
                OnMenuOpen();
        }

        private async void OnMenuOpen()
        {
            _menu.Clear();
            List<GenericUserListItem> list = await eventSystem.Request<List<GenericUserListItem>>("mission:assistance:list");

            foreach (GenericUserListItem item in list)
            {
                UIMenuItem uIMenuItem = new UIMenuItem(item.Name, $"Assist {item.Name}");
                uIMenuItem.ItemData = item;
            }
        }

        private async void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            GenericUserListItem item = selectedItem.ItemData;
            Logger.Debug($"Responding to {item.Name}");

            dynamic pos = await eventSystem.Request<dynamic>("mission:assistance:accept", item.ServerId);
            Logger.Debug($"Location {pos.x}/{pos.y}/{pos.z}");

            // get sent back the position and show a blip for a small amount of time
        }
    }
}
