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
        public static void Advanced(string title, string message, int gtaColorId, CitizenFX.Core.Player player = null, NotificationType notificationType = NotificationType.CHAR_LESTER)
        {
            string messageTitle = notificationType == NotificationType.CHAR_LESTER ? "Curiosity" : "Life V";
            if (player == null)
            {
                Server.TriggerClientEvent("curiosity:Client:Notification:Advanced", $"{notificationType}", 1, messageTitle, title, message, gtaColorId);
            }
            else
            {
                player.TriggerEvent("curiosity:Client:Notification:Advanced", $"{notificationType}", 1, messageTitle, title, message, gtaColorId);
            }
        }
    }

    public enum NotificationType
    {
        CHAR_LESTER,
        CHAR_LIFEINVADER
    }
}
