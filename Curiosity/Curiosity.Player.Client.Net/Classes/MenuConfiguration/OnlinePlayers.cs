using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using MenuAPI;
using System.Drawing;
using Curiosity.Shared.Client.net.Helper;

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
