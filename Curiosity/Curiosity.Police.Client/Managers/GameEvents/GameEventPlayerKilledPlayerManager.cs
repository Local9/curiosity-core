using CitizenFX.Core;

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
            EventSystem.Send("police:playerKilledPlayer", attacker.ServerId, victim.ServerId, isMeleeDamage, weaponInfoHash, damageTypeFlag);
        }
    }
}
