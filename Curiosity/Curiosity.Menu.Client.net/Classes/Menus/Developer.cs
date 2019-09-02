using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Enums;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;

using static CitizenFX.Core.Native.API;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class Developer
    {
        static bool menuSetup = false;

        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Developer Menu", "Dev tools, what else did you expect?~n~~n~Why does this exist now, BECAUSE FUCK WEATHER!!");

        public static void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            SetupMenu();
        }

        static void OnPlayerSpawned(dynamic dynData)
        {
            SetupMenu();
        }

        static void SetupMenu()
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;

            if (menuSetup) return;

            menuSetup = true;

            MenuBase.AddSubMenu(menu, "WIP", false);
        }
    }
}
