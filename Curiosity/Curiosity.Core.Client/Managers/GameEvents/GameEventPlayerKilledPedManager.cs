namespace Curiosity.Core.Client.Managers.GameEvents
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

            EventSystem.Send("police:playerKilledPed", attacker.ServerId, victim.NetworkId, isMeleeDamage, weaponInfoHash, damageTypeFlag);
        }
    }
}
