using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Web;
using System;

namespace Curiosity.Core.Server.Managers
{
    public class ServerEventManager : Manager<ServerEventManager>
    {
        public override void Begin()
        {
            Instance.EventRegistry["clearPedTasksEvent"] += new Action<int, dynamic>(OnClearPedTasksEvent);
            Instance.EventRegistry["giveWeaponEvent"] += new Action<int, dynamic>(OnGiveWeaponEvent);
            Instance.EventRegistry["removeWeaponEvent"] += new Action<int, dynamic>(OnCancelEvent);
            Instance.EventRegistry["removeAllWeaponsEvent"] += new Action<int, dynamic>(OnCancelEvent);
        }

        private void OnCancelEvent(int source, dynamic data)
        {
            API.CancelEvent();
        }

        private void OnGiveWeaponEvent(int source, dynamic data)
        {
            API.CancelEvent();

            Player player = PluginManager.PlayersList[source];

            if (player == null)
            {
                return;
            }

            // MissionData mission = Instance.ExportDictionary["curiosity-mission"].PlayerMission(source);

            string msg = $"[{source}] '{player.Name}' tried to give another ped a weapon, or a script is badly written.";

            //if (mission != null)
            //{
            //    msg += " Player is a mission owner.";
            //}

            DiscordClient.GetModule().SendDiscordServerEventLogMessage(msg);
        }

        private void OnClearPedTasksEvent(int source, dynamic data)
        {
            API.CancelEvent();

            Player player = PluginManager.PlayersList[source];

            if (player == null)
            {
                return;
            }

            DiscordClient.GetModule().SendDiscordServerEventLogMessage($"[{source}] '{player.Name}' tried to remove someone from their vehicle, or a script is badly written.\nDiscordID: {player.Identifiers["discord"]}");
            API.DropPlayer($"{source}", "An error occurred while processing the previous error.");
        }
    }
}
