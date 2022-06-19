using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Managers;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu.PoliceSubMenu
{
    internal class PolicePlayerInteractionMenu
    {
        VehicleManager VehicleManager => VehicleManager.GetModule();

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

                if (Game.PlayerPed.IsDead)
                {
                    Notify.Alert($"Come on, you're dead, this isn't the Zombie Society.");
                    return;
                }

                if ((GetGameTimer() - VehicleManager.GameTimeLeftVehicle) < 2000)
                {
                    Interface.Notify.Alert($"Unless you're called Clark Kent, I'm sorry, no more flying arrests.");
                    return;
                }

                if (Game.PlayerPed.IsRagdoll)
                {
                    Interface.Notify.Alert($"You need to be on your feet to make an arrest.");
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
