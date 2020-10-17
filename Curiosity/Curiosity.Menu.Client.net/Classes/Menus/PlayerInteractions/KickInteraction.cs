﻿using MenuAPI;
using System;
using System.Collections.Generic;
using GlobalEntities = Curiosity.Global.Shared.Entity;
using GlobalEnums = Curiosity.Global.Shared.Enums;

namespace Curiosity.Menus.Client.net.Classes.Menus.PlayerInteractions
{
    class KickInteraction
    {
        static Client client = Client.GetInstance();
        static List<GlobalEntities.LogType> kickReasons = new List<GlobalEntities.LogType>();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Menu:Kick", new Action<string>(SetupKickReasons));
        }

        static void SetupKickReasons(string json)
        {
            kickReasons = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GlobalEntities.LogType>>(json);
        }

        public static Menu CreateMenu(string menuTitle, CitizenFX.Core.Player player)
        {
            Menu menu = new Menu(menuTitle, $"Kick: {player.Name}");

            menu.OnMenuOpen += async (_menu) =>
            {

                MenuBase.MenuOpen(true);

                if (kickReasons.Count == 0)
                {
                    Client.TriggerServerEvent("curiosity:Server:Menu:Reasons", (int)GlobalEnums.LogGroup.Kick);
                    menu.AddMenuItem(new MenuItem("Loading..."));

                    while (kickReasons.Count == 0)
                    {
                        await Client.Delay(0);
                    }

                    menu.ClearMenuItems();
                }

                foreach (GlobalEntities.LogType logType in kickReasons)
                {
                    menu.AddMenuItem(new MenuItem(logType.Description) { ItemData = logType, Description = "Select to kick the player" });
                }
            };

            menu.OnMenuClose += (_menu) =>
            {
                MenuBase.MenuOpen(false);
                _menu.ClearMenuItems();
            };

            menu.OnItemSelect += (_menu, _item, _index) =>
            {
                GlobalEntities.LogType lt = _item.ItemData;
                Client.TriggerServerEvent("curiosity:Server:Player:Kick", player.ServerId, $"{lt.LogTypeId}|{lt.Description}");
                _menu.CloseMenu();
            };

            return menu;
        }
    }
}
