using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Enums;
using MenuAPI;
using Curiosity.Shared.Client.net.Classes.Data;
using Curiosity.Shared.Client.net.Classes;
using System.Collections.Generic;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class PlayerMenu
    {
        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Player", "Various player options and selections.");

        static MenuListItem playerScenarios = new MenuListItem("Player Scenarios", PedScenarios.Scenarios, 0, "Select a scenario and hit enter to start it. Selecting another scenario will override the current scenario. If you're already playing the selected scenario, selecting it again will stop the scenario.");
        static MenuItem stopScenario = new MenuItem("Force Stop Scenario", "This will force a playing scenario to stop immediately, without waiting for it to finish it's 'stopping' animation.");

        static MenuCheckboxItem setMinimapScale = new MenuCheckboxItem("Large Minimap", API.GetResourceKvpString("curiosity:minimap:scale") == "large");

        static bool _cinematic = false;

        public static void Init()
        {
            bool scale = API.GetResourceKvpString("curiosity:minimap:scale") == "large";
            API.SetBigmapActive(scale, scale);

            MenuBase.AddSubMenu(menu, leftIcon: MenuItem.Icon.INV_PERSON);

            menu.OnMenuOpen += (_menu) => {

                MenuBase.MenuOpen(true);
                menu.AddMenuItem(playerScenarios);
                menu.AddMenuItem(stopScenario);

                //menu.AddMenuItem(new MenuItem("Open Skills", "View Skills") { ItemData = SkillType.Experience });
                //menu.AddMenuItem(new MenuItem("Open Stats", "View Stats") { ItemData = SkillType.Statistic });
                menu.AddMenuItem(setMinimapScale);
                menu.AddMenuItem(new MenuCheckboxItem("Cinematic Mode", "Enable Cinematic Mode") { ItemData = "CINEMATIC", Checked = _cinematic });
                menu.AddMenuItem(new MenuItem("Cinematic Bars", "Select to adjust Cinematic Bars") { ItemData = "CINEMATICBARS" });

            };

            menu.OnCheckboxChange += (Menu _menu, MenuCheckboxItem _menuItem, int _itemIndex, bool _newCheckedState) =>
            {
                if (_menuItem.ItemData == "CINEMATIC")
                {
                    _cinematic = _newCheckedState;
                    Client.TriggerEvent("curiosity:Player:UI:CinematicMode", _cinematic);
                }

                if (_menuItem == setMinimapScale)
                {
                    SetMapScale();
                }
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

        static void SetMapScale()
        {
            bool isScaleSmall = API.GetResourceKvpString("curiosity:minimap:scale") == "small";

            if (isScaleSmall)
            {
                API.SetResourceKvp("curiosity:minimap:scale", "large");
            }
            else
            {
                API.SetResourceKvp("curiosity:minimap:scale", "small");
            }

            Client.TriggerEvent("curiosity:Client:UI:SetMapScale", isScaleSmall);
        }
    }
}
