using CitizenFX.Core;
using NativeUI;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    public class CustomRaceMenu
    {
        private UIMenu menu;

        UIMenuListItem miCreateRace;
        List<dynamic> raceList = new List<dynamic>()
        {
            new { label = "Short", minimalBet = 500 },
            new { label = "Normal", minimalBet = 1500 },
            new { label = "Long", minimalBet = 4500 },
        };

        public UIMenu CreateMenu(UIMenu m)
        {
            menu = m;

            miCreateRace = new UIMenuListItem("Create Normal Race", raceList.Select(x => x.label).ToList(), 1);
            menu.AddItem(miCreateRace);

            menu.OnListSelect += Menu_OnListSelect;

            return menu;
        }

        private void Menu_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            dynamic item = raceList[newIndex];
            string raceType = $"{item.label}";
            ExecuteCommand($"createrace {raceType}");
        }
    }
}
