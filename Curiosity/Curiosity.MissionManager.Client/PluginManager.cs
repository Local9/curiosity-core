using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client
{
    public class PluginManager : BaseScript
    {
        internal static List<Blip> Blips = new List<Blip>();

        public PluginManager()
        {

        }

        [Tick]
        private async Task OnMissionHandlerTick()
        {

        }
    }
}
