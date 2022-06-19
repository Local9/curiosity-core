using Curiosity.Systems.Library.Enums;
using System.Linq;

namespace Curiosity.Core.Client.Managers.GameWorld
{
    public class StungunManager : Manager<StungunManager>
    {
        PlayerOptionsManager playerOptions => PlayerOptionsManager.GetModule();
        public int StungunCount = 0;
        int lastTimer = 0;
        int cooldownTime = (1000 * 60);

        public override void Begin()
        {
            lastTimer = API.GetGameTimer();
        }

        [TickHandler(SessionWait = true)]
        private async Task OnStunCooldown()
        {
            if ((API.GetGameTimer() - lastTimer) > cooldownTime)
            {
                if (StungunCount > 0)
                    StungunCount--;
                lastTimer = API.GetGameTimer();
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task OnStunGunMonitor()
        {
            bool isPassive = Game.Player.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;
            if (isPassive)
            {
                await BaseScript.Delay(500);
                return;
            }

            bool isStungun = Game.PlayerPed.Weapons.Current.Hash == WeaponHash.StunGun;
            if (!isStungun)
            {
                await BaseScript.Delay(500);
                return;
            }

            if (Game.PlayerPed.IsShooting)
            {
                bool isOfficer = playerOptions.CurrentJob == ePlayerJobs.POLICE_OFFICER;

                Vector3 weaponImpact = Game.PlayerPed.GetLastWeaponImpactPosition();
                List<Player> players = PluginManager.Instance.PlayerList.Where(p => p.Character.IsInRangeOf(weaponImpact, 1f)).ToList();
                foreach (Player player in players)
                {
                    bool isWanted = player.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;
                    if (!isWanted && isOfficer)
                    {
                        EventSystem.Send("police:officerTazedPlayer", Game.Player.ServerId, player.ServerId);
                    }
                    else if (!isOfficer && StungunCount >= 3)
                    {
                        EventSystem.Send("police:playerTazedPlayer", Game.Player.ServerId, player.ServerId);
                    }
                }

                if (!isOfficer)
                    StungunCount++;
            }
        }
    }
}
