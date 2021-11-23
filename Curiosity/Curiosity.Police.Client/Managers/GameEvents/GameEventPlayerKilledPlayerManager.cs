﻿using CitizenFX.Core;
using Curiosity.Police.Client.Diagnostics;
using Curiosity.Systems.Library.Enums;

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
            if (attacker != Game.Player) return; // I don't care about others, events would be too much
            StateBag victimStateBag = victim.State;
            int playerServerId = attacker.ServerId;
            int victimServerId = victim.ServerId;

            bool isVictimWanted = victimStateBag.Get(StateBagKey.PLAYER_IS_WANTED) ?? false;

            // if I'm an officer, I cannot get punished if the victim is flagged
            if (JobManager.IsOfficer && isVictimWanted)
            {
                Logger.Debug($"PK -> Officer killed Suspect");
                EventSystem.Send("police:suspect:killed", victimServerId, victim.Character.NetworkId);
                return;
            }

            if (JobManager.IsOfficer && !isVictimWanted)
            {
                // punish if a person who is not wanted has been shot and killed
                // check server side
                return;
            }

            if (!JobManager.IsOfficer)
            {
                // punish
                // check server side
                return;
            }
        }
    }
}
