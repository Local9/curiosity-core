using ScaleformUI;

namespace Curiosity.Framework.Client.Managers.Interface.Menu.InteractiveMenu.SubMenu
{
    internal class PlayerSubmenu
    {
        UIMenu _menu;

        UIMenuListItem _menuListMood = new UIMenuListItem("Mood", new List<dynamic>() { "Loading..." }, 0);
        UIMenuListItem _menuListWalkingStyle = new UIMenuListItem("Walking Style", new List<dynamic>() { "Loading..." }, 0);

        internal void CreateMenu(UIMenu menu)
        {
            _menu = menu;

            _menu.EnableAnimation = false;
            _menu.MouseControlsEnabled = false;
            _menu.ControlDisablingEnabled = false;
            _menu.MouseWheelControlEnabled = true;
            _menu.BuildingAnimation = MenuBuildingAnimation.NONE;
            _menu.Glare = true;

            _menu.AddItem(_menuListMood);
            _menu.AddItem(_menuListWalkingStyle);
        }
    }
}
