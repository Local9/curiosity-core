using Curiosity.Missions.Client.MissionPeds;
using MenuAPI;

namespace Curiosity.Missions.Client.Scripts.Menus.PedInteractionMenu.SubMenus
{
    class MenuDispatch
    {
        private const string MENU_TITLE = "Dispatch";
        static Menu menu;
        static InteractivePed _interactivePed;

        static MenuItem mItemRunIdentifcation = new MenuItem("Run Name Check");
        static MenuItem mItemRunVehiclePlate = new MenuItem("Run Plate");
        static MenuItem mItemArrestPed = new MenuItem("Arrest Ped");

        static public void SetupMenu(InteractivePed interactivePed)
        {
            if (menu == null)
            {
                menu = new Menu(MENU_TITLE, MENU_TITLE);
                menu.OnMenuClose += Menu_OnMenuClose;
                menu.OnMenuOpen += Menu_OnMenuOpen;
                menu.OnItemSelect += Menu_OnItemSelect;
                menu.EnableInstructionalButtons = true;
            }
            menu.MenuTitle = MENU_TITLE;
            _interactivePed = interactivePed;
            MenuBase.AddSubMenu(MenuBase.MainMenu, menu);
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemRunIdentifcation)
                Interactions.DispatchInteractions.DispatchCenter.InteractionRunPedIdentification(_interactivePed);

            if (menuItem == mItemRunVehiclePlate)
                Interactions.DispatchInteractions.DispatchCenter.InteractionRunPedVehicle(_interactivePed);

            if (menuItem == mItemArrestPed)
                Interactions.DispatchInteractions.DispatchCenter.InteractionArrestPed(_interactivePed);
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            menu.MenuTitle = "";
            menu.MenuSubtitle = MENU_TITLE;
            MenuBase.MenuState(true);

            menu.ClearMenuItems();

            mItemRunVehiclePlate.Enabled = _interactivePed.GetInteger(Client.DECOR_NPC_VEHICLE_HANDLE) > 0;
            menu.AddMenuItem(mItemRunVehiclePlate);

            mItemRunIdentifcation.Enabled = _interactivePed.HasProvidedId && !_interactivePed.HasLostId;
            mItemRunIdentifcation.Description = _interactivePed.HasProvidedId && !_interactivePed.HasLostId ? "" : "Must have the suspects ID";
            menu.AddMenuItem(mItemRunIdentifcation);

            // mItemArrestPed.Enabled = _interactivePed.CanBeArrested;
            menu.AddMenuItem(mItemArrestPed);
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuController.MainMenu.OpenMenu();
        }
    }
}
