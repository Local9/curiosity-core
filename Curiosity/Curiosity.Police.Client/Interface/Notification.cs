using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;

namespace Curiosity.Police.Client.Interface
{
    public static class Notify
    {
        public static void Success(string message, string position = "bottom-right", string theme = "snackbar", bool playSound = false)
        {
            UxNotification(eNotification.NOTIFICATION_SUCCESS, message, position, theme, playSound);
        }

        public static void Info(string message, string position = "bottom-right", string theme = "snackbar", bool playSound = false)
        {
            UxNotification(eNotification.NOTIFICATION_INFO, message, position, theme, playSound);
        }
        public static void Error(string message, string position = "bottom-right", string theme = "snackbar", bool playSound = false)
        {
            UxNotification(eNotification.NOTIFICATION_ERROR, message, position, theme, playSound);
        }

        public static void Warning(string message, string position = "bottom-right", string theme = "snackbar", bool playSound = false)
        {
            UxNotification(eNotification.NOTIFICATION_WARNING, message, position, theme, playSound);
        }

        public static void Show(string message, string position = "bottom-right", string theme = "snackbar", bool playSound = false)
        {
            UxNotification(eNotification.NOTIFICATION_SHOW, message, position, theme, playSound);
        }

        private static void UxNotification(eNotification notification, string message, string position = "bottom-right", string theme = "snackbar", bool playSound = false)
        {
            PluginManager.Instance.ExportRegistry["curiosity-client"].Notification((int)notification, message, position, theme, 10000, true, false, playSound);
        }
    }
}
