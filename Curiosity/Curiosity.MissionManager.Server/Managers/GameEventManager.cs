using CitizenFX.Core;
using Curiosity.MissionManager.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;

namespace Curiosity.MissionManager.Server.Managers
{
    public class GameEventManager : Manager<GameEventManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("gameEvent:playerKillPlayer", new AsyncEventCallback(async metadata => {

                int killerHandle = metadata.Find<int>(0);
                int victimHandle = metadata.Find<int>(1);

                if (!PluginManager.ActiveUsers.ContainsKey(killerHandle)) return null;
                if (!PluginManager.ActiveUsers.ContainsKey(victimHandle)) return null;

                CuriosityUser curiosityUserKiller = PluginManager.ActiveUsers[killerHandle];
                CuriosityUser curiosityUserVictim = PluginManager.ActiveUsers[victimHandle];

                curiosityUserKiller.LogPlayerKill();

                if (curiosityUserKiller.TotalNumberOfPlayerKills >= 3)
                {
                    Player player = PluginManager.PlayersList[killerHandle];
                    player.Drop($"You've been kicked for killing other players.");
                    EventSystem.GetModule().Send("system:notification:basic", -1, $"{curiosityUserKiller.LatestName} has been kicked");
                    await BaseScript.Delay(100);
                }

                EventSystem.GetModule().Send("system:notification:basic", -1, $"{curiosityUserVictim.LatestName} killed by {curiosityUserKiller.LatestName}");
                
                return null;
            }));
        }
    }
}
