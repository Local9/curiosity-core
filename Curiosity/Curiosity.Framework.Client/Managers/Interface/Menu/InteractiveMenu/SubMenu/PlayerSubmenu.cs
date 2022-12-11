using ScaleformUI;

namespace Curiosity.Framework.Client.Managers.Interface.Menu.InteractiveMenu.SubMenu
{
    internal class PlayerSubmenu
    {
        UIMenu _menu;

        UIMenuListItem _menuListMood = new UIMenuListItem("Mood", new List<dynamic>(), 0);
        UIMenuListItem _menuListWalkingStyle = new UIMenuListItem("Walking Style", new List<dynamic>(), 0);

        internal void CreateMenu(UIMenu menu)
        {
            _menu = menu;
            _menu.AddItem(_menuListMood);
            _menu.AddItem(_menuListWalkingStyle);
        }
    }
}
