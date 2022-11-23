namespace Curiosity.Core.Client.Managers.GameEvents
{
    public class GameEventPlayerKilledPedManager : Manager<GameEventPlayerKilledPedManager>
    {
        PlayerOptionsManager playerOptions = PlayerOptionsManager.GetModule();

        public override void Begin()
        {
            GameEventManager.OnPlayerKillPed += GameEventManager_OnPlayerKillPed;
        }

        private void GameEventManager_OnPlayerKillPed(Player attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            if (attacker != Game.Player) return; // I don't care for others

            victim.DropsWeaponsOnDeath = false;
            EventSystem.Send("police:playerKilledPed", attacker.ServerId, victim.NetworkId, isMeleeDamage, weaponInfoHash, damageTypeFlag);
            
            bool isPassive = playerOptions.IsPassive;

            if (!isPassive) return;
            if (attacker != Game.Player) return;

            if ((victim.IsInPoliceVehicle || IsPedPolice(victim)) && isPassive)
            {
                playerOptions.IsPassive = false;
                Cache.Character.IsPassive = false;

                Cache.PlayerPed.CanBeDraggedOutOfVehicle = true;
                API.SetPlayerVehicleDefenseModifier(Game.Player.Handle, 1f);
                API.NetworkSetFriendlyFireOption(true);
                SetPoliceIgnorePlayer(PlayerPedId(), false);
                SetPoliceRadarBlips(false);
                API.SetMaxWantedLevel(5);

                NetworkSetPlayerIsPassive(false);
                SetLocalPlayerAsGhost(false);
                
                Game.Player.WantedLevel = 3;

                Interface.Notify.Info($"Passive Mode Disabled<br />You killed a cop you fool.");
            }
        }

        public static bool IsPedPolice(Ped ped)
        {
            if (ped == null || !ped.IsHuman) return false;

            if (ped.Model == PedHash.Cop01SFY ||
                ped.Model == PedHash.Cop01SMY ||
                ped.Model == PedHash.Sheriff01SFY ||
                ped.Model == PedHash.Sheriff01SMY ||
                ped.Model == PedHash.Swat01SMY ||
                ped.Model == PedHash.Ranger01SFY ||
                ped.Model == PedHash.Ranger01SMY ||
                ped.Model == PedHash.RsRanger01AMO ||
                ped.Model == PedHash.Security01SMM)
                return true;

            return false;
        }
    }
}
