using System;
using System.Collections.Generic;
using GlobalEntity = Curiosity.Global.Shared.net.Entity;

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

                if (Skills.playerSkills == null)
                {
                    _menuItems.Add(new MenuItemStandard { Title = $"Loading..." });
                }
                else
                {
                    if (Skills.playerSkills.Count == 0)
                    {
                        _menuItems.Add(new MenuItemStandard { Title = $"Loading..." });
                    }
                    else
                    {
                        foreach (KeyValuePair<string, GlobalEntity.Skills> v in Skills.playerSkills)
                        {
                            string labelDes = v.Value.LabelDescription;
                            if (string.IsNullOrEmpty(labelDes))
                            {
                                _menuItems.Add(new MenuItemStandard { Title = $"{v.Value.Label.ToTitleCase()}", Detail = $"{v.Value.Value:#,#00}" });
                            }
                            else
                            {
                                _menuItems.Add(new MenuItemStandard { Title = $"{v.Value.Label.ToTitleCase()}", Detail = $"{v.Value.Value:#,#00}", SubDetail = $"{labelDes}" });
                            }
                        }
                    }
                }

                _menuItems.Add(new MenuItemStandard { Title = $"Last Update", Detail = $"{Skills.LastUpdate.ToString("HH:mm")}" });

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
