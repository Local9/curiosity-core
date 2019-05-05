using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;

namespace Curiosity.Player.Server.Net
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
        }
    }
}
