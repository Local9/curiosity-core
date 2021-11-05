using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;

namespace Curiosity.Police.Client.Interface
{
    public static class Notify
    {
        public static void Success(string message, string position = "bottom-right", string theme = "snackbar")
        {
            API.PlaySoundFrontend(-1, "package_delivered_success", "DLC_GR_Generic_Mission_Sounds", true);
            UxNotification(eNotification.NOTIFICATION_SUCCESS, message, position, theme);
        }

        public static void Info(string message, string position = "bottom-right", string theme = "snackbar")
        {
            API.PlaySoundFrontend(-1, "INFO", "HUD_FRONTEND_DEFAULT_SOUNDSET", true);
            UxNotification(eNotification.NOTIFICATION_INFO, message, position, theme);
        }
        public static void Error(string message, string position = "bottom-right", string theme = "snackbar")
        {
            API.PlaySoundFrontend(-1, "ERROR", "HUD_FREEMODE_SOUNDSET", true);
            UxNotification(eNotification.NOTIFICATION_ERROR, message, position, theme);
        }

        public static void Warning(string message, string position = "bottom-right", string theme = "snackbar")
        {
            API.PlaySoundFrontend(-1, "tyre_health_warning", "DLC_sum20_Open_Wheel_Racing_Sounds", true);
            UxNotification(eNotification.NOTIFICATION_WARNING, message, position, theme);
        }

        public static void Show(string message, string position = "bottom-right", string theme = "snackbar")
        {
            API.PlaySoundFrontend(-1, "SELECT", "HUD_FREEMODE_SOUNDSET", true);
            UxNotification(eNotification.NOTIFICATION_SHOW, message, position, theme);
        }

        private static void UxNotification(eNotification notification, string message, string position = "bottom-right", string theme = "snackbar")
        {
            PluginManager.Instance.ExportRegistry["curiosity-client"].Notification((int)notification, message, position, theme, 10000, true, false);
        }
    }
}
