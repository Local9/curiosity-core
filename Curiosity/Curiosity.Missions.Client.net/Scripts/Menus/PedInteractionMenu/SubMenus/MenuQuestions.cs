using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.Scripts.Interactions.PedInteractions;
using MenuAPI;

namespace Curiosity.Missions.Client.Scripts.Menus.PedInteractionMenu.SubMenus
{
    class MenuQuestions
    {
        private const string MENU_TITLE = "Questions";
        static Menu menu;
        static InteractivePed _interactivePed;

        static MenuItem mItemIdentification = new MenuItem("Personal Identification please");
        static MenuItem mItemQuestionDrunk = new MenuItem("Have you had anything to drink today?");
        static MenuItem mItemQuestionDrugs = new MenuItem("Have you took any drugs recently?");
        static MenuItem mItemQuestionIllegal = new MenuItem("Anything illegal you might be carrying?");
        static MenuItem mItemQuestionSearchVehicle = new MenuItem("Can I search you?");

        static public void SetupMenu(InteractivePed interactivePed)
        {
            if (menu == null)
            {
                menu = new Menu(MENU_TITLE, MENU_TITLE);
                menu.OnMenuClose += Menu_OnMenuClose;
                menu.OnMenuOpen += Menu_OnMenuOpen;
                menu.OnItemSelect += Menu_OnItemSelect;
                menu.OnListIndexChange += Menu_OnListIndexChange;
                menu.EnableInstructionalButtons = true;
            }
            menu.MenuTitle = MENU_TITLE;
            _interactivePed = interactivePed;
            MenuBase.AddSubMenu(MenuBase.MainMenu, menu);
        }

        private static void Menu_OnListIndexChange(Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex)
        {

        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemIdentification)
                Social.InteractionPresentIdentification(_interactivePed);

            if (menuItem == mItemQuestionDrunk)
                Social.InteractionDrunk(_interactivePed);

            if (menuItem == mItemQuestionDrugs)
                Social.InteractionDrug(_interactivePed);

            if (menuItem == mItemQuestionIllegal)
                Social.InteractionIllegal(_interactivePed);

            if (menuItem == mItemQuestionSearchVehicle)
                Social.InteractionSearch(_interactivePed);
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            menu.MenuTitle = "";
            menu.MenuSubtitle = MENU_TITLE;

            MenuBase.MenuState(true);
            menu.ClearMenuItems();

            menu.AddMenuItem(mItemIdentification);
            menu.AddMenuItem(mItemQuestionDrunk);
            menu.AddMenuItem(mItemQuestionDrugs);
            menu.AddMenuItem(mItemQuestionIllegal);
            menu.AddMenuItem(mItemQuestionSearchVehicle);
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuController.MainMenu.OpenMenu();
        }
    }
}
