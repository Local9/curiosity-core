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

                int attackerHandle = metadata.Find<int>(0);
                int victimHandle = metadata.Find<int>(1);

                if (!PluginManager.ActiveUsers.ContainsKey(attackerHandle)) return null;
                if (!PluginManager.ActiveUsers.ContainsKey(victimHandle)) return null;

                CuriosityUser curiosityUserKiller = PluginManager.ActiveUsers[attackerHandle];
                CuriosityUser curiosityUserVictim = PluginManager.ActiveUsers[victimHandle];

                curiosityUserKiller.LogPlayerKill();

                Player attacker = PluginManager.PlayersList[attackerHandle];
                Player victim = PluginManager.PlayersList[victimHandle];

                if (curiosityUserKiller.TotalNumberOfPlayerKills >= 3)
                {
                    attacker.Drop($"You've been kicked for killing other players.");
                    EventSystem.GetModule().Send("system:notification:basic", -1, $"{curiosityUserKiller.LatestName} has been kicked");
                    await BaseScript.Delay(100);
                }

                EventSystem.GetModule().Send("gameEvent:kill", int.Parse(attacker.Handle));
                victim.TriggerEvent("curiosity:Client:Player:Revive", "Server");

                string msg = $"{curiosityUserVictim.LatestName} killed by {curiosityUserKiller.LatestName}";

                BaseScript.TriggerEvent("curiosity:Server:Log:Message", msg);
                
                EventSystem.GetModule().Send("system:notification:basic", -1, msg);
                
                return null;
            }));
        }
    }
}
