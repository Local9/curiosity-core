using CitizenFX.Core;
using Curiosity.Core.Client.Events;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu.PoliceSubMenu
{
    internal class PolicePlayerInteractionMenu
    {
        EventSystem EventSystem => EventSystem.GetModule();
        UIMenu _menu;
        int _playerServerId;

        UIMenuItem _jail = new UIMenuItem("Jail");

        public UIMenu CreateMenu(UIMenu m, int playerServerId)
        {
            _menu = m;
            _playerServerId = playerServerId;

            _menu.AddItem(_jail);

            _menu.OnMenuStateChanged += _menu_OnMenuStateChanged;
            _menu.OnItemSelect += _menu_OnItemSelect;

            return _menu;
        }

        private async void _menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == _jail)
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    Notify.Alert($"You must exit the vehicle to arrest the player.");
                    return;
                }

                bool res = await EventSystem.Request<bool>("police:suspect:jailed", _playerServerId);
                if (res)
                    InteractionMenu.MenuPool.CloseAllMenus();
            }
        }

        private void _menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeForward || state == MenuState.Opened)
            {

            }
        }
    }
}
