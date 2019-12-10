using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;

namespace Curiosity.Missions.Client.net.Scripts.Police.MenuHandler.Submenu
{
    class TrafficStopQuestions
    {
        static Menu menu;

        static MenuItem mItemQuestionDrunk = new MenuItem("Have you had anything to drink today?");
        static MenuItem mItemQuestionDrugs = new MenuItem("Have you took any drugs recently?");
        static MenuItem mItemQuestionIllegal = new MenuItem("Anything illegal in the vehicle?");
        static MenuItem mItemQuestionSearchVehicle = new MenuItem("Can I search your vehicle?");
        static MenuItem mItemGoBack = new MenuItem("Go Back");

        static public void SetupMenu()
        {
            if (menu == null)
            {
                menu = new Menu("Question the driver", "Question the driver");

                menu.OnMenuOpen += Menu_OnMenuOpen;
                menu.OnMenuClose += Menu_OnMenuClose;
                menu.OnItemSelect += Menu_OnItemSelect;

                menu.AddMenuItem(mItemQuestionDrunk);
                menu.AddMenuItem(mItemQuestionDrugs);
                menu.AddMenuItem(mItemQuestionIllegal);
                menu.AddMenuItem(mItemQuestionSearchVehicle);
                menu.AddMenuItem(mItemGoBack);
            }
            menu.MenuTitle = "Question the driver";
            //SuspectMenu.AddSubMenu(SuspectMenu.TrafficStopMenu, menu);
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuController.DontOpenAnyMenu = true;
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemQuestionDrunk)
            {
                // TrafficStop.InteractionDrunk();
            }
            if (menuItem == mItemQuestionDrugs)
            {
                // TrafficStop.InteractionDrug();
            }
            if (menuItem == mItemQuestionIllegal)
            {
                // TrafficStop.InteractionIllegal();
            }
            if (menuItem == mItemQuestionSearchVehicle)
            {
                // TrafficStop.InteractionSearch();
            }
            if (menuItem == mItemGoBack)
            {
                menu.CloseMenu();
                //SuspectMenu.Open(Shared.Client.net.Enums.Patrol.MenuType.Normal);
            }
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", true);
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", true);
            menu.MenuTitle = "";
            MenuController.DontOpenAnyMenu = false;
            //SuspectMenu.IsMenuOpen = true;
        }
    }
}
