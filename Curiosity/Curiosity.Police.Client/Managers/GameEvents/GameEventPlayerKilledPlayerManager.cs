using CitizenFX.Core;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Police.Client.Managers.GameEvents
{
    public class GameEventPlayerKilledPlayerManager : Manager<GameEventPlayerKilledPlayerManager>
    {
        public override void Begin()
        {
            GameEventManager.OnPlayerKillPlayer += GameEventManager_OnPlayerKillPlayer;
        }

        private void GameEventManager_OnPlayerKillPlayer(Player attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            if (attacker != Game.Player) return; // I don't care about others
            StateBag victimStateBag = victim.State;

            bool isVictimWanted = victimStateBag.Get(StateBagKey.PLAYER_WANTED) ?? false;

            // if I'm an officer, I cannot get punished if the victim is flagged
            if (JobManager.IsOfficer && isVictimWanted)
            {

            }

            if (JobManager.IsOfficer && !isVictimWanted)
            {
                // punish
            }
        }
    }
}
