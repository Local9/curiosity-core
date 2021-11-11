using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Entity;
using System;

namespace Curiosity.Core.Server.Managers
{
    public class ServerEventManager : Manager<ServerEventManager>
    {
        public override void Begin()
        {
            Instance.EventRegistry["clearPedTasksEvent"] += new Action<int, bool>(OnClearPedTasksEvent);
            Instance.EventRegistry["giveWeaponEvent"] += new Action<int, int, bool, int, bool>(OnGiveWeaponEvent);
        }

        private async void OnGiveWeaponEvent(int source, int weaponType, bool unk1, int ammo, bool givenAsPickup)
        {
            API.CancelEvent();

            Player player = PluginManager.PlayersList[source];

            if (player == null)
            {
                return;
            }

            MissionData mission = Instance.ExportDictionary["curiosity-mission"].PlayerMission(source);

            string msg = $"[{source}] '{player.Name}' tried to give another ped a weapon, or a script is badly written.";

            if (mission != null)
            {
                msg += " Player is a mission owner.";
            }

            DiscordClient.GetModule().SendDiscordServerEventLogMessage(msg);
            await BaseScript.Delay(0);
        }

        private async void OnClearPedTasksEvent(int source, bool immediately)
        {
            API.CancelEvent();

            Player player = PluginManager.PlayersList[source];

            if (player == null)
            {
                return;
            }

            DiscordClient.GetModule().SendDiscordServerEventLogMessage($"[{source}] '{player.Name}' tried to remove someone from their vehicle, or a script is badly written.");
            await BaseScript.Delay(0);
        }
    }
}
