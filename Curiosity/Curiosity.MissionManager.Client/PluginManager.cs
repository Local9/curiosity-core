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
            //if (Game.PlayerPed.IsDead && Mission.isOnMission)
            //{
            //    Mission.currentMission.Fail("The player died."); // need to think about how this should work given respawning
            //}


        }

        private void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            foreach (Blip blip in Blips) blip.Delete();
        }
    }
}
