using CitizenFX.Core;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Police.Client.Managers.GameEvents
{
    public class GameEventPlayerKilledPedManager : Manager<GameEventPlayerKilledPedManager>
    {
        public override void Begin()
        {
            GameEventManager.OnPlayerKillPed += GameEventManager_OnPlayerKillPed;
        }

        private void GameEventManager_OnPlayerKillPed(Player attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            if (attacker != Game.Player) return; // I don't care for others

            victim.DropsWeaponsOnDeath = false;

            if (JobManager.IsOfficer)
            {
                // punish cops
            }

            if (!JobManager.IsOfficer)
            {
                // punish players
            }
        }
    }
}
