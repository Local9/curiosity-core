using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu.PoliceSubMenu
{
    internal class PolicePlayerInteractionMenu
    {
        UIMenu _menu;

        UIMenuItem _jail = new UIMenuItem("Jail");

        public UIMenu CreateMenu(UIMenu m)
        {
            _menu = m;

            _menu.AddItem(_jail);

            _menu.OnMenuStateChanged += _menu_OnMenuStateChanged;

            return _menu;
        }

        private void _menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeForward || state == MenuState.Opened)
            {

            }
        }
    }
}
