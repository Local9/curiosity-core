using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Enums;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class Developer
    {
        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Developer Menu", "Dev tools, what else did you expect?~n~~n~Why does this exist now, BECAUSE FUCK WEATHER!!");

        public static void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));
        }

        static void OnPlayerSpawned()
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;

            MenuBase.AddSubMenu(menu);
        }
    }
}
