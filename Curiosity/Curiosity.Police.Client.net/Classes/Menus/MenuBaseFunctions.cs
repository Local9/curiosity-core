using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Police.Client.net.Classes.Menus
{
    class MenuBaseFunctions
    {
        public static void MenuOpen()
        {
            try
            {
                Client.TriggerEvent("curiosity:Client:Menu:IsOpened", true);
                Client.TriggerEvent("curiosity:Client:UI:LocationHide", true);
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
                Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
                Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
            }
            catch (Exception ex)
            {
                // 
            }
        }
    }
}
