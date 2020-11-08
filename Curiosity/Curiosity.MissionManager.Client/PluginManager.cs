using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Diagnostics;
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
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
        }

        private void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            if (!API.HasAnimDictLoaded("veh@low@front_ps@enter_exit"))
                API.RequestAnimDict("veh@low@front_ps@enter_exit");

            if (!API.HasAnimDictLoaded("rcmnigel3_trunk"))
                API.RequestAnimDict("rcmnigel3_trunk");

            if (!API.HasAnimDictLoaded("rcmepsilonism8"))
                API.RequestAnimDict("rcmepsilonism8");
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
                Logger.Debug(ex.Message);
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
                Logger.Debug(ex.Message);
            }
        }
    }
}
