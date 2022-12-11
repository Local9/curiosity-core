using ScaleformUI;

namespace Curiosity.Framework.Client.Managers.Interface.Menu.InteractiveMenu
{
    public class InteractiveMenuManager : Manager<InteractiveMenuManager>
    {
        UIMenu _menu;
        MenuPool _menuPool => GameInterface.Hud.MenuPool;

        // Menu GPS
        static List<dynamic> _gpsLocations = new List<dynamic>() { "Loading..." };
        int _gpsIndex = 0;
        UIMenuListItem _menuListGpsLocations = new UIMenuListItem("Navigation", _gpsLocations, 0);
        // Menu Player
        UIMenu _submenuPlayer;
        SubMenu.PlayerSubmenu _playerSubmenu = new();
        // -> Inventory
        // -> Supporter
        // -> Settings (Language)
        // Menu Vehicles
        // Menu Settings
        // Item Passive (checkbox)
        // Item Kill Yourself (button)

        public override void Begin()
        {
            _menu = new UIMenu(Game.Player.Name, "Interactive Menu", GameInterface.Hud.MenuOffset)
            {
                EnableAnimation = false,
                BuildingAnimation = MenuBuildingAnimation.NONE,
                MouseControlsEnabled = false,
                Glare = true
            };
            _menu.EnableAnimation = false;
            _menu.BuildingAnimation = MenuBuildingAnimation.NONE;
            _menu.MouseControlsEnabled = false;

            _menu.AddItem(_menuListGpsLocations);
            _menuListGpsLocations.SetLeftBadge(BadgeIcon.GLOBE_WHITE);

            _submenuPlayer = _menuPool.AddSubMenu(_menu, "Player Options", "Various player options and settings.");
            _playerSubmenu.CreateMenu(_submenuPlayer);
            BadgeIcon badgeIcon = Game.PlayerPed.Gender == Gender.Male ? BadgeIcon.MALE : BadgeIcon.FEMALE;
            _submenuPlayer.ParentItem.SetLeftBadge(badgeIcon);

            _menu.OnListChange += OnListChange;

            _menu.RefreshIndex();
            _menuPool.Add(_menu);
        }

        private void OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == _menuListGpsLocations)
            {
                _gpsIndex = newIndex;
                return;
            }
        }

        [TickHandler]
        private async Task OnInteractiveMenuAsync()
        {
            if (!Game.IsPaused && !IsPauseMenuRestarting() && IsScreenFadedIn() && !IsPlayerSwitchInProgress())
            {
                if (_menuPool.IsAnyMenuOpen)
                {
                    if (Game.CurrentInputMode == InputMode.MouseAndKeyboard)
                    {
                        if ((Game.IsControlJustPressed(0, Control.InteractionMenu) || Game.IsDisabledControlJustPressed(0, Control.InteractionMenu)))
                        {
                            _menu.Visible = false;
                            
                            if (_menuPool.IsAnyMenuOpen)
                                _menuPool.CloseAllMenus();
                        }
                    }
                }
                else
                {
                    if (Game.CurrentInputMode == InputMode.GamePad)
                    {
                        int tmpTimer = API.GetGameTimer();

                        while ((Game.IsControlPressed(0, Control.InteractionMenu) || Game.IsDisabledControlPressed(0, Control.InteractionMenu)) && !Game.IsPaused && API.IsScreenFadedIn() && !API.IsPlayerSwitchInProgress())
                        {
                            if (API.GetGameTimer() - tmpTimer > 400)
                            {
                                if (!_menuPool.IsAnyMenuOpen)
                                {
                                    _menu.Visible = !_menu.Visible;
                                }
                                break;
                            }
                            await BaseScript.Delay(0);
                        }
                    }
                    else
                    {
                        if ((Game.IsControlJustPressed(0, Control.InteractionMenu) || Game.IsDisabledControlJustPressed(0, Control.InteractionMenu)) && !Game.IsPaused && API.IsScreenFadedIn() && !API.IsPlayerSwitchInProgress())
                        {
                            if (!_menuPool.IsAnyMenuOpen)
                            {
                                _menu.Visible = !_menu.Visible;
                            }
                        }
                    }
                }
            }
        }
    }
}
