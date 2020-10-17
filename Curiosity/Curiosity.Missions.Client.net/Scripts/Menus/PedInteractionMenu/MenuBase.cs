using CitizenFX.Core;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.Scripts.Interactions.PedInteractions;
using Curiosity.Shared.Client.net.Extensions;
using MenuAPI;
using System;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.Scripts.Menus.PedInteractionMenu
{
    class MenuBase
    {
        static Client client = Client.GetInstance();
        static public Menu MainMenu;
        static InteractivePed _interactivePed;

        // buttons
        static MenuItem mItemHello = new MenuItem("Hello");

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:interaction:closeMenu", new Action(OnCloseMenus));

        }

        static void OnCloseMenus()
        {
            if (MenuController.IsAnyMenuOpen())
                MenuController.CloseAllMenus();
        }

        static public void Open(InteractivePed interactivePed)
        {
            if (NpcHandler.IsPerformingCpr) return;

            MenuState(true);
            _interactivePed = interactivePed;

            if (MainMenu == null)
            {
                MainMenu = new Menu("", "Interaction Menu");

                // menu actions
                MainMenu.OnMenuClose += MainMenu_OnMenuClose;
                MainMenu.OnMenuOpen += MainMenu_OnMenuOpen;
                MainMenu.OnItemSelect += MainMenu_OnItemSelect;
                MainMenu.OnListIndexChange += MainMenu_OnListIndexChange;

                // menu config
                MainMenu.EnableInstructionalButtons = true;

                MenuController.AddMenu(MainMenu);
            }

            client.RegisterTickHandler(OnDistanceTask);
            MainMenu.OpenMenu();
        }

        static async Task OnDistanceTask()
        {
            if (_interactivePed.Position.Distance(Game.PlayerPed.Position) > 4)
                MenuController.CloseAllMenus();

            if (!MenuController.IsAnyMenuOpen())
                client.DeregisterTickHandler(OnDistanceTask);
        }

        private static void MainMenu_OnListIndexChange(Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex)
        {

        }

        private static void MainMenu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            // ALIVE
            if (menuItem == mItemHello)
                Social.InteractionHello(_interactivePed);
        }

        private static void MainMenu_OnMenuOpen(Menu menu)
        {
            MainMenu.ClearMenuItems();

            if (_interactivePed.IsAlive)
                SubMenus.MenuQuestions.SetupMenu(_interactivePed);

            SubMenus.MenuInteraction.SetupMenu(_interactivePed);

            if (_interactivePed.IsAlive)
                SubMenus.MenuDispatch.SetupMenu(_interactivePed);
        }

        private static void MainMenu_OnMenuClose(Menu menu)
        {
            MenuState(false);
        }

        public static void AddSubMenu(Menu menu, Menu submenu, string label = "→→→", bool buttonEnabled = true)
        {
            MenuController.AddSubmenu(menu, submenu);
            MenuItem submenuButton = new MenuItem(submenu.MenuTitle, submenu.MenuSubtitle) { Label = label, Enabled = buttonEnabled };
            menu.AddMenuItem(submenuButton);
            MenuController.BindMenuItem(menu, submenu, submenuButton);
        }

        public static void MenuState(bool IsOpen)
        {
            MenuController.DontOpenAnyMenu = !IsOpen;
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", IsOpen);
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", IsOpen);
        }

        public static bool AnyMenuVisible()
        {
            return MenuController.IsAnyMenuOpen();
        }
    }
}
