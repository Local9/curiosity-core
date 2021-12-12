using CitizenFX.Core;
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

                foreach(Player player in PluginManager.Instance.PlayerList)
                {
                    if (!Game.PlayerPed.IsInRangeOf(player.Character.Position, 10f)) continue;

                    UIMenu uIMenu = InteractionMenu.MenuPool.AddSubMenu(oldMenu, player.Name);
                    new PolicePlayerInteractionMenu().CreateMenu(uIMenu);
                }

            }
        }
    }
}
