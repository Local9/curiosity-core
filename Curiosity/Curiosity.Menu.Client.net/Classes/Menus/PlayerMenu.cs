using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Enums;
using MenuAPI;
using Curiosity.Menus.Client.net.Classes.Data;
using System.Collections.Generic;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class PlayerMenu
    {
        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Player", "Various player options and selections.");

        static MenuListItem playerScenarios = new MenuListItem("Player Scenarios", PedScenarios.Scenarios, 0, "Select a scenario and hit enter to start it. Selecting another scenario will override the current scenario. If you're already playing the selected scenario, selecting it again will stop the scenario.");
        static MenuItem stopScenario = new MenuItem("Force Stop Scenario", "This will force a playing scenario to stop immediately, without waiting for it to finish it's 'stopping' animation.");

        static List<string> chatPositions = new List<string>() { "Right", "Left" };

        static MenuListItem chatSide = new MenuListItem("Chat Position", chatPositions, API.GetResourceKvpInt("curiosity:menu:chat:listIndex"), "Select chat position and press enter.");

        public static void Init()
        {
            MenuBase.AddSubMenu(menu);

            menu.OnMenuOpen += (_menu) => {

                MenuBase.MenuOpen(true);
                menu.AddMenuItem(playerScenarios);
                menu.AddMenuItem(stopScenario);

                //menu.AddMenuItem(new MenuItem("Open Skills", "View Skills") { ItemData = SkillType.Experience });
                //menu.AddMenuItem(new MenuItem("Open Stats", "View Stats") { ItemData = SkillType.Statistic });
                menu.AddMenuItem(chatSide);
                menu.AddMenuItem(new MenuCheckboxItem("Cinematic Mode", "Enable Cinematic Mode") { ItemData = "CINEMATIC", Checked = false });
                menu.AddMenuItem(new MenuItem("Cinematic Bars", "Select to adjust Cinematic Bars") { ItemData = "CINEMATICBARS" });

            };

            menu.OnCheckboxChange += (Menu _menu, MenuCheckboxItem _menuItem, int _itemIndex, bool _newCheckedState) =>
            {
                if (_menuItem.ItemData == "CINEMATIC")
                    Client.TriggerEvent("curiosity:Player:UI:CinematicMode");
            };

            menu.OnItemSelect += (_menu, _item, _index) =>
            {
                // Code in here would get executed whenever an item is pressed.
                if ($"{_item.ItemData}" == $"{SkillType.Experience}" || $"{_item.ItemData}" == $"{SkillType.Statistic}")
                {
                    Client.TriggerServerEvent("curiosity:Server:Skills:GetListData", (int)_item.ItemData);
                    menu.CloseMenu();
                }

                if ($"{_item.ItemData}" == "CINEMATICBARS")
                    Client.TriggerEvent("curiosity:Player:UI:BlackBarHeight");

                if (_item == stopScenario)
                {
                    CommonFunctions.PlayScenario("forcestop");
                }
            };

            // List selections
            menu.OnListItemSelect += (sender, listItem, listIndex, itemIndex) =>
            {
                if (listItem == chatSide)
                {
                    string cssClass = "chat-right";

                    if (chatPositions[listIndex] == chatPositions[1])
                        cssClass = "chat-left";

                    API.SetResourceKvp("curiosity:menu:chat:position", cssClass);
                    API.SetResourceKvpInt("curiosity:menu:chat:listIndex", listIndex);

                    Client.TriggerEvent("curiosity:Client:Chat:Side", cssClass);
                }

                if (listItem == playerScenarios)
                {
                    CommonFunctions.PlayScenario(PedScenarios.ScenarioNames[PedScenarios.Scenarios[listIndex]]);
                }
            };

            menu.OnMenuClose += (_menu) =>
            {
                MenuBase.MenuOpen(false);
                _menu.ClearMenuItems();
            };

            menu.OnListIndexChange += (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                // Code in here would get executed whenever the selected value of a list item changes (when left/right key is pressed).
            };
        }
    }
}
