using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Events;
using System;
using System.IO;

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
