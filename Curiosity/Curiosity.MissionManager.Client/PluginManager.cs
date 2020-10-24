using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client
{
    public class PluginManager : BaseScript
    {
        internal static PluginManager Instance;
        internal static List<Blip> Blips = new List<Blip>();

        public PluginManager()
        {
            Instance = this;

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

        /// <summary>
        /// Registers a tick function
        /// </summary>
        /// <param name="action"></param>
        public void RegisterTickHandler(Func<Task> action)
        {
            try
            {
                Tick += action;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        /// <summary>
        /// Removes a tick function from the registry
        /// </summary>
        /// <param name="action"></param>
        public void DeregisterTickHandler(Func<Task> action)
        {
            try
            {
                Tick -= action;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }
    }
}
