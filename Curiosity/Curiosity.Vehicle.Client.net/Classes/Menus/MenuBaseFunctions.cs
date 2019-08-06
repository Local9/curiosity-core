﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Vehicle.Client.net.Classes.Menus
{
    class MenuBaseFunctions
    {
        public static void MenuOpen()
        {
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", true);
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", true);
            try
            {
                Environment.VehicleSpawnMarkerHandler.IsMenuOpen = true;
            }
            catch (Exception ex)
            {
                // 
            }
        }

        public static void MenuClose()
        {
            try
            {
                Environment.VehicleSpawnMarkerHandler.IsMenuOpen = false;
            }
            catch (Exception ex)
            {
                // 
            }
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
        }
    }
}
