using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Curiosity.Client.net.Classes.Menus
{
    class PlayerActionsMenu
    {
        public static MenuObserver Observer;
        public static MenuModel PlayerActionsMenuModel;
        public static MenuModel PlayerSubActionsMenu;
        public static List<Tuple<int, MenuItem, Func<bool>>> ItemsAll = new List<Tuple<int, MenuItem, Func<bool>>>();
        internal static List<MenuItem> ItemsFiltered = new List<MenuItem>();
        public static bool IsDirty = false;

        class SubActionsMenu : MenuModel
        {
            public override void Refresh()
            {
                var _menuItems = new List<MenuItem>();

                if (Player.PlayerInformation.IsStaff())
                {
                    _menuItems.Add(new MenuItemStandard { Title = "Kick" });
                    _menuItems.Add(new MenuItemStandard { Title = "Ban" });
                }

                _menuItems.Add(new MenuItemStandard { Title = "Invite to Party" });

                menuItems = _menuItems;
            }
        }

        class PlayerMenu : MenuModel
        {
            public override void Refresh()
            {
                PlayerSubActionsMenu = new SubActionsMenu { numVisibleItems = 7 };
                PlayerSubActionsMenu.headerTitle = "Action";
                PlayerSubActionsMenu.statusTitle = "";
                PlayerSubActionsMenu.menuItems = new List<MenuItem>() { new MenuItemStandard { Title = "Loadiing..." } };

                var _menuItems = new List<MenuItem>();

                foreach(CitizenFX.Core.Player player in new PlayerList())
                {
                    _menuItems.Add(new MenuItemSubMenu
                    {
                        Title = $"{player.Name}"
                        , SubMenu = PlayerSubActionsMenu
                    });
                }

                menuItems = _menuItems;
            }
        }

        public static void Init()
        {
            try
            {
                PlayerActionsMenuModel = new PlayerMenu { numVisibleItems = 7 };
                PlayerActionsMenuModel.headerTitle = "Players";
                PlayerActionsMenuModel.statusTitle = "";
                PlayerActionsMenuModel.menuItems = new List<MenuItem>() { new MenuItemStandard { Title = "Loadiing..." } };

                InteractionListMenu.RegisterInteractionMenuItem(new MenuItemSubMenu
                {
                    Title = $"Player Actions",
                    SubMenu = PlayerActionsMenuModel
                }, () => true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
                if (ex.InnerException != null)
                    Debug.WriteLine($"{ex.InnerException}");

            }
        }
    }
}
