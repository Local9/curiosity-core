using System;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;

namespace Curiosity.Player.Client.Net
{
    public class CuriosityPlayer : BaseScript
    {
        public CuriosityPlayer()
        {
            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
        }

        private void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            Debug.WriteLine("curiosity-server -> Started");

            Screen.ShowNotification("~b~Info:~w~ Curiosity Server Started");
        }
    }
}
