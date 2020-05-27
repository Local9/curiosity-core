using Curiosity.CasinoSystems.Client.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.CasinoSystems.Client
{
    class PluginLoader
    {
        public static void Init()
        {
            InteriorChecker.Init();
            Teleporters.Init();
        }
    }
}
