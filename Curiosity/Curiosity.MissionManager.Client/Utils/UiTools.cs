using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.MissionManager.Client.Utils
{
    public class UiTools : BaseScript
    {
        static public void Impound(string subject, string message, bool flash = false, bool playsound = false)
        {
            Advanced("CHAR_PROPERTY_TOWING_IMPOUND", 2, "San Andreas Impound", subject, message, 2, flash, playsound);
        }
        static public void Dispatch(string subject, string message, bool flash = false, bool playsound = false)
        {
            Advanced("CHAR_CALL911", 2, "Dispatch", subject, message, 2, flash, playsound);
        }

        static public void Advanced(string icon, int iconType, string title, string subject, string message, int gtaColorId, bool flash = false, bool playsound = false)
        {
            if (playsound)
                API.PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);

            API.SetNotificationBackgroundColor(gtaColorId);
            API.SetNotificationTextEntry("STRING");
            API.AddTextComponentString(message);
            API.SetNotificationMessage(icon, icon, flash, iconType, title, subject);
            API.DrawNotification(true, true);
        }
    }
}
