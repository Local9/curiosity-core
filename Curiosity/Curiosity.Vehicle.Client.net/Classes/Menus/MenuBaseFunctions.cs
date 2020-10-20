using System;

namespace Curiosity.Vehicles.Client.net.Classes.Menus
{
    class MenuBaseFunctions
    {
        public static void MenuOpen()
        {
            Plugin.TriggerEvent("curiosity:Client:Menu:IsOpened", true);
            Plugin.TriggerEvent("curiosity:Client:UI:LocationHide", true);
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
            Plugin.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
            Plugin.TriggerEvent("curiosity:Client:UI:LocationHide", false);
        }
    }
}
