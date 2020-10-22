using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client
{
    public class PluginManager : BaseScript
    {
        internal static List<Blip> Blips = new List<Blip>();

        public PluginManager()
        {
            EventHandlers["onClientResourceStop"] += new Action<string>(OnClientResourceStop);
        }

        [Tick]
        private async Task OnMissionHandlerTick()
        {

        }

        private void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            foreach (Blip blip in Blips) blip.Delete();
        }
    }
}
