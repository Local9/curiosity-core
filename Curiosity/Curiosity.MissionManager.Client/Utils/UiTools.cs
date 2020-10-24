using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.MissionManager.Client.Utils
{
    public class UiTools : BaseScript
    {
        static public void Impound(string subject, string message)
        {
            Advanced("CHAR_PROPERTY_TOWING_IMPOUND", 2, "San Andreas Impound", subject, message, 2);
        }
        static public void Dispatch(string subject, string message)
        {
            Advanced("CHAR_CALL911", 2, "Dispatch", subject, message, 2);
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
