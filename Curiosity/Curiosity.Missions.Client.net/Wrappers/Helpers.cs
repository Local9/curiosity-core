using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net.Enums;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.net.Wrappers
{
    class Helpers
    {
        static public void ShowSimpleNotification(string message)
        {
            Screen.ShowNotification(message);
        }

        static public void ShowNotification(string title, string subtitle, string message, NotificationCharacter notificationCharacter = NotificationCharacter.CHAR_CALL911)
        {
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{notificationCharacter}", 2, title, subtitle, message, 2);
        }

        // internal methods
        static public void ShowOfficerSubtitle(string subtitle)
        {
            Screen.ShowSubtitle($"~o~Officer:~w~ {subtitle}");
        }

        static public void ShowSuspectSubtitle(string subtitle)
        {
            Screen.ShowSubtitle($"~b~Suspect:~w~ {subtitle}");
        }
    }
}
