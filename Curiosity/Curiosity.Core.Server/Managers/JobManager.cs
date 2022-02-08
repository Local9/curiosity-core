using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Core.Server.Managers
{
    public class JobManager : Manager<JobManager>
    {
        public override void Begin()
        {
            Instance.ExportDictionary.Add("SetUserJob", new Func<string, string, bool>(
                (playerHandle, job) =>
                {

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        return false;

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        return false;

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];
                    user.CurrentJob = job;

                    return true;
                }));
        }
    }
}
