using MenuAPI;

namespace Curiosity.Missions.Client.net.Scripts.Police.MenuHandler.Submenu
{
    class TrafficStopInteractions
    {
        static Menu menu;

        static bool IsCharacterFollowing = false;

        static MenuItem mItemBreathalyzer = new MenuItem("Breathalyzer");
        static MenuItem mItemDrugTest = new MenuItem("Drug test");
        static MenuItem mItemSearch = new MenuItem("Search");

        static MenuItem mItemLeaveVehicle = new MenuItem("Leave Vehicle") { Description = "This will request the suspect to leave their own vehicle." };
        static MenuItem mItemReturnToVehicle = new MenuItem("Return to Vehicle") { Description = "This will request the suspect to get back in their own vehicle." };

        static MenuItem mItemPutInOwnCar = new MenuItem("Put in own Vehicle") { Description = "This will request the suspect to enter your own vehicle." };
        static MenuItem mItemRemoveFromOwnCar = new MenuItem("Remove from own Vehicle") { Description = "This will request the suspect to leave your own vehicle." };

        static MenuItem mItemGoBack = new MenuItem("Go Back");

        static public void SetupMenu()
        {
            if (menu == null)
            {
                menu = new Menu("Interactions", "Interactions");

                menu.OnMenuOpen += Menu_OnMenuOpen;
                menu.OnMenuClose += Menu_OnMenuClose;
                menu.OnItemSelect += Menu_OnItemSelect;
            }
            menu.MenuTitle = "Interactions";
            SuspectMenu.AddSubMenu(SuspectMenu.TrafficStopMenu, menu);
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuController.DontOpenAnyMenu = true;
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemBreathalyzer)
            {
                TrafficStop.InteractionBreathalyzer();
            }

            if (menuItem == mItemDrugTest)
            {
                TrafficStop.InteractionDrugTest();
            }

            if (menuItem == mItemSearch)
            {
                TrafficStop.InteractionSearching();
            }

            if (menuItem == mItemLeaveVehicle)
            {
                if (TrafficStop.StoppedDriver.IsInVehicle())
                {
                    TrafficStop.LeaveVehicle();
                }
            }

            if (menuItem == mItemReturnToVehicle)
            {
                if (!TrafficStop.StoppedDriver.IsInVehicle())
                {
                    TrafficStop.ReturnToVehicle();
                }
            }

            if (menuItem == mItemPutInOwnCar)
            {
                ArrestPed.InteractionPutInVehicle();
            }

            if (menuItem == mItemRemoveFromOwnCar)
            {
                ArrestPed.InteractionRemoveFromVehicle();
            }

            if (menuItem == mItemGoBack)
            {
                menu.CloseMenu();
                SuspectMenu.Open(Shared.Client.net.Enums.Patrol.MenuType.Normal);
            }
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", true);
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", true);
            menu.MenuTitle = "";
            MenuController.DontOpenAnyMenu = false;
            SuspectMenu.IsMenuOpen = true;

            menu.ClearMenuItems();

            menu.AddMenuItem(mItemBreathalyzer);
            menu.AddMenuItem(mItemDrugTest);
            menu.AddMenuItem(mItemSearch);

            if (TrafficStop.StoppedDriver.IsInVehicle() && TrafficStop.StoppedDriver.CurrentVehicle == TrafficStop.TargetVehicle)
                menu.AddMenuItem(mItemLeaveVehicle);

            if (!TrafficStop.StoppedDriver.IsInVehicle())
                menu.AddMenuItem(mItemReturnToVehicle);

            if (ArrestPed.IsPedCuffed)
            {
                if (!ArrestPed.PedInHandcuffs.IsInVehicle())
                    menu.AddMenuItem(mItemPutInOwnCar);

                if (ArrestPed.PedInHandcuffs.IsInVehicle())
                {
                    if (ArrestPed.PedInHandcuffs.CurrentVehicle == Client.CurrentVehicle)
                    {
                        menu.AddMenuItem(mItemRemoveFromOwnCar);
                    }
                }
                    
            }

            menu.AddMenuItem(mItemGoBack);

        }

        static void UpdateButtons()
        {
            mItemLeaveVehicle.Enabled = TrafficStop.StoppedDriver.IsInVehicle();
            mItemReturnToVehicle.Enabled = !TrafficStop.StoppedDriver.IsInVehicle();
        }
    }
}
