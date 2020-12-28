using Curiosity.MissionManager.Server.Managers;
using Curiosity.Systems.Shared.Entity;
using Newtonsoft.Json;
using System;

namespace Curiosity.MissionManager.Server.ServerExports
{
    public class StatusExport : Manager<StatusExport>
    {
        public override void Begin()
        {
            Instance.ExportDictionary.Add("Status", new Func<string>(
                () =>
                {
                    var returnObject = new { status = PluginManager.ServerReady };
                    return JsonConvert.SerializeObject(returnObject);
                }
            ));

            Instance.ExportDictionary.Add("PlayerMission", new Func<int, MissionData>(
                (playerHandle) =>
                {
                    return Managers.MissionManager.ActiveMissions[playerHandle];
                }
            ));
        }
    }
}
