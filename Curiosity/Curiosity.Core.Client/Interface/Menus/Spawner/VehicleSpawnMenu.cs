﻿using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Events;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus.Spawner
{
    public class VehicleSpawnMenu : Manager<VehicleSpawnMenu>
    {
        private UIMenu jobMenu;

        public override void Begin()
        {
            EventSystem.Attach("vehicle:spawn:menu", new EventCallback(metadata =>
            {
                NotificationManger.GetModule().Success("MENU EVENT WORKING");
                return null;
            }));
        }

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.OnMenuOpen += Menu_OnMenuOpen;

            menu.MouseControlsEnabled = false;
            menu.MouseEdgeEnabled = false;

            return menu;
        }

        private void Menu_OnMenuOpen(UIMenu menu)
        {

        }
    }
}