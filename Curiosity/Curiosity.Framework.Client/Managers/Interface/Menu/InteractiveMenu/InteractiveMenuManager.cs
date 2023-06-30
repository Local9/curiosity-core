namespace Curiosity.Framework.Client.Managers.Interface.Menu.InteractiveMenu
{
    public class InteractiveMenuManager : Manager<InteractiveMenuManager>
    {
        UIMenu menu;
        MenuPool menuPool => GameInterface.Hud.MenuPool;

        // Menu GPS
        static List<dynamic> gpsLocations = new List<dynamic>() { "Loading..." };
        int gpsIndex = 0;
        UIMenuListItem menuListGpsLocations = new UIMenuListItem("Navigation", gpsLocations, 0);
        // Menu Player
        UIMenu submenuPlayer;
        SubMenu.PlayerSubmenu playerSubmenu = new();
        // -> Inventory
        // -> Supporter
        // -> Settings (Language)
        // Menu Vehicles
        // Menu Settings
        // Item Passive (checkbox)
        // Item Kill Yourself (button)

        public override void Begin()
        {
            menu = new UIMenu(Game.Player.Name, "Interactive Menu", GameInterface.Hud.MenuOffset)
            {
                EnableAnimation = false,
                MouseControlsEnabled = false,
                ControlDisablingEnabled = false,
                MouseWheelControlEnabled = true,
                BuildingAnimation = MenuBuildingAnimation.NONE,
                Glare = true
            };

            menu.AddItem(menuListGpsLocations);
            menuListGpsLocations.SetLeftBadge(BadgeIcon.GLOBE_WHITE);

            submenuPlayer = menuPool.AddSubMenu(menu, "Player Options", "Various player options and settings.");
            playerSubmenu.CreateMenu(submenuPlayer);
            BadgeIcon badgeIcon = Game.PlayerPed.Gender == Gender.Male ? BadgeIcon.MALE : BadgeIcon.FEMALE;
            submenuPlayer.ParentItem.SetLeftBadge(badgeIcon);

            menu.OnListChange += OnListChange;

            menu.RefreshIndex();
            menuPool.Add(menu);
        }

        private void OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == menuListGpsLocations)
            {
                gpsIndex = newIndex;
                return;
            }
        }

        // [TickHandler]
        private async Task OnInteractiveMenuAsync()
        {
            if (!Game.IsPaused && !IsPauseMenuRestarting() && IsScreenFadedIn() && !IsPlayerSwitchInProgress())
            {
                if (menuPool.IsAnyMenuOpen)
                {
                    if (Game.CurrentInputMode == InputMode.MouseAndKeyboard)
                    {
                        if ((Game.IsControlJustPressed(0, Control.InteractionMenu) || Game.IsDisabledControlJustPressed(0, Control.InteractionMenu)))
                        {
                            menu.Visible = false;

                            if (menuPool.IsAnyMenuOpen)
                                menuPool.CloseAllMenus();
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
                                if (!menuPool.IsAnyMenuOpen)
                                {
                                    menu.Visible = !menu.Visible;
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
                            if (!menuPool.IsAnyMenuOpen)
                            {
                                menu.Visible = !menu.Visible;
                            }
                        }
                    }
                }
            }
        }
    }
}
