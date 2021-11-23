using CitizenFX.Core;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Core.Server.Managers
{
    public class GameEventManager : Manager<GameEventManager>
    {
        DateTime lastRun = DateTime.Now;

        public override void Begin()
        {
            EventSystem.Attach("gameEvent:playerKillPlayer", new AsyncEventCallback(async metadata => {

                int attackerHandle = metadata.Find<int>(0);
                int victimHandle = metadata.Find<int>(1);
                string weapon = metadata.Find<string>(2);

                if (!PluginManager.ActiveUsers.ContainsKey(attackerHandle)) return null;
                if (!PluginManager.ActiveUsers.ContainsKey(victimHandle)) return null;

                CuriosityUser curiosityUserKiller = PluginManager.ActiveUsers[attackerHandle];
                CuriosityUser curiosityUserVictim = PluginManager.ActiveUsers[victimHandle];

                curiosityUserKiller.IncreasePlayerKills();

                Player attacker = PluginManager.PlayersList[attackerHandle];
                Player victim = PluginManager.PlayersList[victimHandle];

                int currentAttackerJob = attacker.State.Get(StateBagKey.PLAYER_JOB) ?? (int)ePlayerJobs.UNEMPLOYED;
                ePlayerJobs ePlayerJobCurrentAttacker = (ePlayerJobs)currentAttackerJob;
                bool isAttackerOfficer = ePlayerJobCurrentAttacker == ePlayerJobs.POLICE_OFFICER;

                int currentVictimJob = victim.State.Get(StateBagKey.PLAYER_JOB) ?? (int)ePlayerJobs.UNEMPLOYED;
                ePlayerJobs ePlayerJobCurrentVictim = (ePlayerJobs)currentVictimJob;
                bool isVictimOfficer = ePlayerJobCurrentVictim == ePlayerJobs.POLICE_OFFICER;

                bool isAttackerSuspect = attacker.State.Get(StateBagKey.PLAYER_IS_WANTED) ?? false;
                bool isVictimSuspect = victim.State.Get(StateBagKey.PLAYER_IS_WANTED) ?? false;

                string atkPrefix = string.Empty;
                string vctPrefix = string.Empty;
                string atkSuffix = string.Empty;
                string vctSuffix = string.Empty;

                if (isAttackerOfficer)
                    atkPrefix = "~b~Officer~s~ ";
                else if (isVictimOfficer)
                    vctPrefix = "~b~Officer~s~ ";

                if (isAttackerSuspect)
                    atkSuffix = " ~s~[~o~Criminal~s~]";
                else if (isVictimSuspect)
                    vctSuffix = " ~s~[~o~Criminal~s~]";

                string msg = $"~o~{vctPrefix}{curiosityUserVictim.LatestName}{vctSuffix} ~s~killed by ~y~{atkPrefix}{curiosityUserKiller.LatestName}{atkSuffix} ~s~(~b~{weapon}~s~)";

                string cleanMessage = msg.Replace("~o~", "").Replace("~s~", "").Replace("~y~", "").Replace("~b~", "");
                DiscordClient.GetModule().SendDiscordPlayerDeathLogMessage($"[Player Kill] {cleanMessage}");
                await BaseScript.Delay(0);
                EventSystem.SendAll("system:notification:basic", msg);
                
                return null;
            }));
        }
    }
}
