using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Shared.Entity;
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

        private void OnGiveWeaponEvent(int source, int weaponType, bool unk1, int ammo, bool givenAsPickup)
        {
            API.CancelEvent();

            Player player = PluginManager.PlayersList[source];

            if (player == null)
            {
                return;
            }

            MissionData mission = Instance.ExportDictionary["curiosity-mission"].PlayerMission(source);

            string msg = $"[{source}] '{player.Name}' tried to give someone a weapon, or a script is badly written.";

            if (mission != null)
            {
                msg += " Player is a mission owner.";
            }

            Instance.ExportDictionary["curiosity-server"].DiscordServerEventLog(msg);
        }

        private void OnClearPedTasksEvent(int source, bool immediately)
        {
            API.CancelEvent();

            Player player = PluginManager.PlayersList[source];

            if (player == null)
            {
                return;
            }

            Instance.ExportDictionary["curiosity-server"].DiscordServerEventLog($"[{source}] '{player.Name}' tried to remove someone from their vehicle, or a script is badly written.");
        }
    }
}
