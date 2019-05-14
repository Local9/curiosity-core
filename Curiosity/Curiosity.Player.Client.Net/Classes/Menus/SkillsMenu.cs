using CitizenFX.Core;
using Curiosity.Client.net.Classes.Player;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Curiosity.Client.net.Classes.Menus
{
    class SkillsMenu
    {
        public static MenuObserver Observer;
        public static MenuModel SkillsMenuModel;
        public static List<Tuple<int, MenuItem, Func<bool>>> ItemsAll = new List<Tuple<int, MenuItem, Func<bool>>>();
        internal static List<MenuItem> ItemsFiltered = new List<MenuItem>();
        public static bool IsDirty = false;

        class SkillMenu : MenuModel
        {
            public override void Refresh()
            {
                var _menuItems = new List<MenuItem>();

                if (Skills.playerSkills.Count == 0)
                {
                    _menuItems.Add(new MenuItemStandard { Title = $"Loading..." });
                }
                else
                {
                    foreach (KeyValuePair<string, int> v in Skills.playerSkills)
                    {
                        _menuItems.Add(new MenuItemStandard { Title = $"{v.Key.ToTitleCase()}", Detail = $"{v.Value:#,#00}" });
                    }
                }

                menuItems = _menuItems;
            }
        }

        public static void Init()
        {
            try
            {
                MenuOptions PedMenuOptions = new MenuOptions { Origin = new PointF(700, 200) };
                SkillsMenuModel = new SkillMenu { numVisibleItems = 7 };
                SkillsMenuModel.headerTitle = "Skills";
                SkillsMenuModel.statusTitle = "";
                SkillsMenuModel.menuItems = new List<MenuItem>() { new MenuItemStandard { Title = "Populating..." } };

                InteractionListMenu.RegisterInteractionMenuItem(new MenuItemSubMenu
                {
                    Title = $"Skills",
                    SubMenu = SkillsMenuModel
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
