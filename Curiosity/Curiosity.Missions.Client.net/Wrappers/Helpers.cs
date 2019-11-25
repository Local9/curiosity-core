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

        static public async void AnimationRadio()
        {
            LoadAnimation("random@arrests");
            Game.PlayerPed.Task.PlayAnimation("random@arrests", "generic_radio_enter", 1.5f, 2.0f, -1, (AnimationFlags)50, 2.0f);
            await Client.Delay(6000);
            Game.PlayerPed.Task.ClearAll();
        }

        static public async Task<bool> LoadAnimation(string dict)
        {
            while (!HasAnimDictLoaded(dict))
            {
                await Client.Delay(0);
                RequestAnimDict(dict);
            }
            return true;
        }


        // internal methods
        static public void ShowOfficerSubtitle(string subtitle)
        {
            Screen.ShowSubtitle($"~o~Officer:~w~ {subtitle}");
        }

        static public void ShowDriverSubtitle(string subtitle)
        {
            Screen.ShowSubtitle($"~b~Driver:~w~ {subtitle}");
        }
    }
}
