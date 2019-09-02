using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Enums;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Menus.Client.net.Classes.Menus.MissionCreator
{
    class MissionMenu
    {
        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Mission Maker", "WORK IN PROGRESS");

        // Buttons


        public static void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));

            
        }

        static void OnPlayerSpawned()
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;

            MenuBase.AddSubMenu(menu, "WIP");
        }
    }
}
