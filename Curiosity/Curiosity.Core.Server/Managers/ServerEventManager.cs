using CitizenFX.Core;
using CitizenFX.Core.Native;
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

            Instance.ExportDictionary["curiosity-server"].ServerEventLog($"{player.Name} tried to give someone a weapon, or a script is badly written.");
        }

        private void OnClearPedTasksEvent(int source, bool immediately)
        {
            API.CancelEvent();

            Player player = PluginManager.PlayersList[source];

            if (player == null)
            {
                return;
            }

            Instance.ExportDictionary["curiosity-server"].ServerEventLog($"{player.Name} tried to remove someone from their vehicle, or a script is badly written.");
        }
    }
}
