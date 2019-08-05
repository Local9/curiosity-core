using System;
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
            Environment.VehicleSpawnMarkerHandler.IsMenuOpen = true;
        }

        public static void MenuClose()
        {
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
            Environment.VehicleSpawnMarkerHandler.IsMenuOpen = false;
        }
    }
}
