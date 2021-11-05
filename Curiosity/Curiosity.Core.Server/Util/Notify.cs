using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Enums;

namespace Curiosity.Core.Server.Util
{
    public static class Notify
    {
        public static void Send(int handle = -1, eNotification notification = eNotification.NOTIFICATION_INFO, string message = "", string position = "bottom-right", string theme = "toast")
        {
            if (handle == -1)
            {
                EventSystem.GetModule().SendAll("ui:notification", (int)notification, message, position, theme);
            }
            else
            {
                EventSystem.GetModule().Send("ui:notification", handle, (int)notification, message, position, theme);
            }
        }
    }
}
