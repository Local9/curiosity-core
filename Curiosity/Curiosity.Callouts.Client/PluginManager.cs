using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.EventWrapper;
using System;

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
    }
}
