using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Server
{
    public class PluginManager : BaseScript
    {
        internal static PluginManager Instance { get; private set; }

        public PluginManager()
        {
            Instance = this;
        }
    }
}
