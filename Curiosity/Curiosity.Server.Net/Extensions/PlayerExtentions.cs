using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Curiosity.Server.net.Helpers;

namespace Curiosity.Server.net.Extensions
{
    static class PlayerExtentions
    {
        public static void NotificationCuriosity(this Player player, string title, string message)
        {
            Helpers.Notifications.Advanced($"{title}", $"{message}", 63, player, NotificationType.CHAR_LIFEINVADER);
        }
    }
}
