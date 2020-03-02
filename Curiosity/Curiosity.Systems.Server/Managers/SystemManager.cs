using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Events;
using Curiosity.Systems.Server.MySQL;
using System;

namespace Curiosity.Systems.Server.Managers
{
    public class SystemManager : Manager<SystemManager>
    {
        public override void Begin()
        {
            Curiosity.EventRegistry["onResourceStop"] += new Action<string>(OnResourceStop);
        }

        private void OnResourceStop(string resourceName)
        {
            if (resourceName != API.GetCurrentResourceName()) return;

            Logger.Info($"Stopping Curiosity Systems");
        }
    }
}
