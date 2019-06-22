using CitizenFX.Core;
using Curiosity.Global.Shared.net.Enums;
using MenuAPI;

namespace Curiosity.Client.net.Classes.Menus
{
    class PlayerMenu
    {
        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Player", "Various player options and selections.");

        public static void Init()
        {
            MenuBase.AddSubMenu(menu);

            menu.OnMenuOpen += (_menu) => {
                menu.AddMenuItem(new MenuItem("Open Skills", "View Skills") { ItemData = SkillType.Experience });
                menu.AddMenuItem(new MenuItem("Open Stats", "View Stats") { ItemData = SkillType.Statistic });
            };

            menu.OnItemSelect += (_menu, _item, _index) =>
            {
                // Code in here would get executed whenever an item is pressed.
                if (_item.ItemData == SkillType.Experience || _item.ItemData == SkillType.Statistic)
                {
                    Client.TriggerServerEvent("curiosity:Server:Skills:GetListData", (int)_item.ItemData);
                }

                menu.CloseMenu();
            };

            menu.OnMenuClose += (_menu) =>
            {
                _menu.ClearMenuItems();
            };

            menu.OnListIndexChange += (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                // Code in here would get executed whenever the selected value of a list item changes (when left/right key is pressed).
                Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");
            };
        }
    }
}
