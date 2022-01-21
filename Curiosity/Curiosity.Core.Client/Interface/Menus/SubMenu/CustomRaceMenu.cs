using CitizenFX.Core;
using NativeUI;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    public class CustomRaceMenu
    {
        private UIMenu menu;

        UIMenuListItem miCreateRace;
        List<dynamic> raceList = new List<dynamic>()
        {
            new { label = "Short" },
            new { label = "Normal" },
            new { label = "Long" },
        };

        public UIMenu CreateMenu(UIMenu m)
        {
            menu = m;

            miCreateRace = new UIMenuListItem("Create Normal Race", raceList, 1);
            menu.AddItem(miCreateRace);

            menu.OnListSelect += Menu_OnListSelect;

            return menu;
        }

        private void Menu_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            string raceType = $"{listItem.Items[newIndex]}";
            ExecuteCommand($"createrace {raceType}");
        }
    }
}
