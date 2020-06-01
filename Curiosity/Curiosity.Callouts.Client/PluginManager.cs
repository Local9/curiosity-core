using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.EventWrapper;
using System;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Client
{
    public class PluginManager : BaseScript
    {
        internal static PluginManager Instance { get; private set; }

        public PluginManager()
        {
            Instance = this;

            EventHandlers[Events.Native.Client.PlayerSpawned] += new Action<dynamic>(OnPlayerSpawned);

            EventHandlers[Events.Client.Callout.EnableCalloutManager.Path] += Events.Client.Callout.EnableCalloutManager.Action += enableCallouts =>
            {
                UpdateCalloutStatus(enableCallouts);
            };

            EventHandlers[Events.Native.Client.OnClientResourceStart.Path] += Events.Native.Client.OnClientResourceStart.Action +=
                resourceName =>
                {
                    if (resourceName != API.GetCurrentResourceName()) return;

                    BaseScript.TriggerEvent(Events.Client.RequestPlayerInformation);

                    Logger.Log("---------------------------------");
                    Logger.Log("------ > Callouts loaded < ------");
                    Logger.Log("---------------------------------");
                };
        }

        private void UpdateCalloutStatus(bool enableCallouts)
        {
            if (enableCallouts)
            {
                Screen.ShowNotification($"~b~Callouts~s~: ~g~Enabled");
                Game.PlayerPed.RelationshipGroup = (uint)Collections.RelationshipHash.Cop;
            }
            else
            {
                Screen.ShowNotification($"~b~Callouts~s~: ~r~Disabled");
                Game.PlayerPed.RelationshipGroup = (uint)Collections.RelationshipHash.Player;
            }
        }

        private void OnPlayerSpawned(dynamic spawnInfo)
        {
            BaseScript.TriggerEvent("curiosity:Client:Player:Information");
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
