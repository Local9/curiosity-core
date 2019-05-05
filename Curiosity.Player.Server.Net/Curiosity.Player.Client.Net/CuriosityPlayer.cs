using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.Net
{
    public class CuriosityPlayer : BaseScript
    {
        long userId = 0;

        public CuriosityPlayer()
        {
            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
            EventHandlers["curiosity:Client:Player:Setup"] += new Action<long>(OnPlayerSetup);

            Tick += OnGetUserId;
        }

        private void OnResourceStart(string resourceName)
        {
            Screen.ShowNotification("~b~Info:~w~ Curiosity Server Started");

            if (API.GetCurrentResourceName() != resourceName) return;

            Debug.WriteLine("curiosity-server -> Started");

            Screen.ShowNotification("~b~Info:~w~ Curiosity Server Started");
        }

        async Task OnGetUserId()
        {
            while (userId == 0)
            {
                await Delay(1000);
                TriggerServerEvent("curiosity:Server:Player:Setup");
            }
            await Delay(1000);
        }

        async void OnPlayerSetup(long userId)
        {
            Screen.ShowNotification($"~b~Info:~w~ User {userId} Created");
            await Delay(0);
        }
    }
}
