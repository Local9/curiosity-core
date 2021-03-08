using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Enums;

namespace Curiosity.Core.Server.Util
{
    public static class Notify
    {
        public static void Notification(Notification notification, string message, string position = "bottom-right", string theme = "toast")
        {
            EventSystem.GetModule().SendAll("ui:notification", (int)notification, message, position, theme);
        }
    }
}
