using CitizenFX.Core;
using Curiosity.Systems.Library.Enums;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu.PoliceSubMenu
{
    internal class PolicePlayerListMenu
    {
        UIMenu _menu;

        public UIMenu CreateMenu(UIMenu m)
        {
            _menu = m;

            _menu.OnMenuStateChanged += _menu_OnMenuStateChanged;

            return _menu;
        }

        private void _menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeForward || state == MenuState.Opened)
            {
                _menu.Clear();

                foreach (Player player in PluginManager.Instance.PlayerList)
                {
                    if (player == Game.Player) continue; // ignore self

                    if (player.Character.IsInVehicle()) continue; // they must be outside a vehicle

                    bool isWanted = player.State.Get(StateBagKey.PLAYER_IS_WANTED) ?? false;
                    if (!isWanted) continue; // only show those who are wanted

                    if (!Game.PlayerPed.IsInRangeOf(player.Character.Position, 10f)) continue;

                    UIMenu uIMenu = InteractionMenu.MenuPool.AddSubMenu(_menu, player.Name);
                    new PolicePlayerInteractionMenu().CreateMenu(uIMenu, player.ServerId);
                }

            }
        }
    }
}
