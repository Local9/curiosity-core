using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;

namespace Curiosity.Core.Server.Extensions
{
    static class UserExtensions
    {
        public static void Send(this CuriosityUser user, string target, params object[] payloads)
        {
            EventSystem.GetModule().Send(target, user.Handle, payloads);
        }

        public static void NotificationSuccess(this CuriosityUser user, string message)
        {

            EventSystem.GetModule().Send("ui:notification", user.Handle, eNotification.NOTIFICATION_SUCCESS, message, "bottom-right", "snackbar", true);
        }
    }
}
