using Curiosity.Core.Client.Events;
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
            List<dynamic> list = await eventSystem.Request<List<dynamic>>("mission:assistance:list");
        }

        private async void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            
        }
    }
}
