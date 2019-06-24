using MenuAPI;
using System;
using System.Collections.Generic;
using GlobalEntities = Curiosity.Global.Shared.net.Entity;
using GlobalEnums = Curiosity.Global.Shared.net.Enums;

namespace Curiosity.Client.net.Classes.Menus.PlayerInteractions
{
    class BanInteraction
    {
        static Client client = Client.GetInstance();
        static List<GlobalEntities.LogType> banReasons = new List<GlobalEntities.LogType>();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Menu:Ban", new Action<string>(SetupBanReasons));
        }

        static void SetupBanReasons(string json)
        {
            banReasons = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GlobalEntities.LogType>>(json);
        }

        public static Menu CreatePeriodMenu(string menuTitle, GlobalEntities.LogType logType, CitizenFX.Core.Player player)
        {
            string title = (menuTitle.Length > 15) ? $"{menuTitle.Substring(0, 15)}..." : menuTitle;

            Menu periodMenu = new Menu(title, $"Select to set Ban Period");
            periodMenu.AddMenuItem(new MenuItem("3 Days") { ItemData = 3 });
            periodMenu.AddMenuItem(new MenuItem("7 Days") { ItemData = 7 });
            periodMenu.AddMenuItem(new MenuItem("14 Days") { ItemData = 14 });
            periodMenu.AddMenuItem(new MenuItem("28 Days") { ItemData = 28 });
            periodMenu.AddMenuItem(new MenuItem("Permanent Ban") { ItemData = 0 });

            periodMenu.OnMenuOpen += (_menu) =>
            {
                Environment.UI.Location.HideLocation = true;
            };

            periodMenu.OnMenuClose += (_menu) =>
            {
                Environment.UI.Location.HideLocation = false;
                _menu.ClearMenuItems();
            };

            periodMenu.OnItemSelect += (_menu, _item, _index) =>
            {
                bool perm = _item.ItemData == 0;

                Client.TriggerServerEvent("curiosity:Server:Player:Ban", player.ServerId, $"{logType.LogTypeId}|{logType.Description}", perm, _item.ItemData);
                _menu.CloseMenu();
            };

            return periodMenu;
        }

        public static Menu CreateMenu(string menuTitle, CitizenFX.Core.Player player)
        {
            Menu menu = new Menu(menuTitle, $"Ban: {player.Name}");

            menu.OnMenuOpen += async (_menu) => {

                Environment.UI.Location.HideLocation = true;

                if (banReasons.Count == 0)
                {
                    Client.TriggerServerEvent("curiosity:Server:Menu:Reasons", (int)GlobalEnums.LogGroup.Ban);
                    menu.AddMenuItem(new MenuItem("Loading..."));
                    
                    while (banReasons.Count == 0)
                    {
                        await Client.Delay(0);
                    }

                    menu.ClearMenuItems();
                }

                foreach (GlobalEntities.LogType logType in banReasons)
                {
                    if (logType.Description.Contains("Permanent") && Player.PlayerInformation.IsTrustedAdmin())
                    {
                        Menu periodMenu = CreatePeriodMenu(logType.Description, logType, player);
                        AddSubMenu(menu, periodMenu, logType.Description);
                    }
                    else if (!logType.Description.Contains("Permanent"))
                    {
                        Menu periodMenu = CreatePeriodMenu(logType.Description, logType, player);
                        AddSubMenu(menu, periodMenu, logType.Description);
                    }
                }
            };

            menu.OnMenuClose += (_menu) =>
            {
                Environment.UI.Location.HideLocation = false;
                _menu.ClearMenuItems();
            };

            return menu;
        }

        public static void AddSubMenu(Menu menu, Menu submenu, string buttonText)
        {
            MenuController.AddSubmenu(menu, submenu);
            MenuItem submenuButton = new MenuItem(buttonText, submenu.MenuSubtitle) { Label = "→→→" };
            menu.AddMenuItem(submenuButton);
            MenuController.BindMenuItem(menu, submenu, submenuButton);
        }
    }
}
