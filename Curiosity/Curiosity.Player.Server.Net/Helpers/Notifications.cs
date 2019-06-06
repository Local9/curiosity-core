using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Curiosity.Server.net.Helpers
{
    static class Notifications
    {
        public static void Advanced(string title, string message, int gtaColorId, Player player = null)
        {
            if (player == null)
            {
                Server.TriggerClientEvent("curiosity:Client:Notification:Advanced", "CHAR_LESTER", 1, "Curiosity", title, message, gtaColorId);
            }
            else
            {
                player.TriggerEvent("curiosity:Client:Notification:Advanced", "CHAR_LESTER", 1, "Curiosity", title, message, gtaColorId);
            }
        }
    }
}
