using MenuAPI;

namespace Curiosity.Client.net.Classes.MenuConfiguration
{
    class OnlinePlayers
    {
        static Menu menu = new Menu("Online Players", "Online Players");
        static Client client = Client.GetInstance();

        public static void Init()
        {
            PlayerMenu.AddSubMenu(menu);

            menu.OnMenuOpen += (_menu) => {
                foreach (CitizenFX.Core.Player player in Client.players)
                {
                    Menu playerMenu = new Menu(player.Name, "Player Interactions");

                    Menu reportingOptions = PlayerInteractions.ReportInteraction.CreateMenu("Report", player);
                    AddSubMenu(playerMenu, reportingOptions);

                    if (Player.PlayerInformation.IsStaff())
                    {
                        Menu kickOptions = PlayerInteractions.KickInteraction.CreateMenu("Kick", player);
                        AddSubMenu(playerMenu, kickOptions);

                        Menu banOptions = PlayerInteractions.BanInteraction.CreateMenu("Ban", player);
                        AddSubMenu(playerMenu, banOptions);
                    }
                    AddSubMenu(menu, playerMenu);
                }
            };

            menu.OnMenuClose += (_menu) =>
            {
                _menu.ClearMenuItems();
            };            
        }

        public static void AddSubMenu(Menu menu, Menu submenu)
        {
            MenuController.AddSubmenu(menu, submenu);
            MenuItem submenuButton = new MenuItem(submenu.MenuTitle, submenu.MenuSubtitle) { Label = "→→→" };
            menu.AddMenuItem(submenuButton);
            MenuController.BindMenuItem(menu, submenu, submenuButton);
        }
    }
}
