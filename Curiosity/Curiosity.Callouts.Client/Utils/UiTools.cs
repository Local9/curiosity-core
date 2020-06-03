using CitizenFX.Core.Native;

namespace Curiosity.Callouts.Client.Utils
{
    class UiTools
    {
        static public void Dispatch(string subject, string message)
        {
            Advanced("CHAR_911", 2, "Dispatch", subject, message, 2);
        }

        static public void Advanced(string icon, int iconType, string title, string subject, string message, int gtaColorId)
        {
            API.SetNotificationBackgroundColor(gtaColorId);
            API.SetNotificationTextEntry("STRING");
            API.AddTextComponentString(message);
            API.SetNotificationMessage(icon, icon, false, iconType, title, subject);
            API.DrawNotification(false, true);
        }
    }
}
