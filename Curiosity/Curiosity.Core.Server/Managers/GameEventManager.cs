using CitizenFX.Core;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers
{
    public class GameEventManager : Manager<GameEventManager>
    {
        DateTime lastRun = DateTime.Now;

        public override void Begin()
        {
            EventSystem.GetModule().Attach("gameEvent:playerKillPlayer", new EventCallback(metadata => {

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

                string msg = $"~o~{curiosityUserVictim.LatestName} ~s~killed by ~y~{curiosityUserKiller.LatestName} with a ~b~{weapon}";

                DiscordClient.GetModule().SendDiscordPlayerLogMessage($"[Player Kill] {msg.Replace("~o~", "").Replace("~s~", "").Replace("~y~", "").Replace("~b~", "")}");
                EventSystem.GetModule().Send("system:notification:basic", -1, msg);
                
                return null;
            }));
        }

        [Tick]
        private async Task OnPlayerKillDecreaseTick()
        {
            if (DateTime.Now.Subtract(lastRun).TotalMinutes >= 2)
            {
                lastRun = DateTime.Now;

                foreach(KeyValuePair<int, CuriosityUser> kvp in PluginManager.ActiveUsers)
                {
                    if (kvp.Value.TotalNumberOfPlayerKills > 0)
                    {
                        kvp.Value.LowerPlayerKills();
                    }
                }
            }

            await BaseScript.Delay(10000);
        }
    }
}
