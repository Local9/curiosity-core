using Curiosity.CasinoSystems.Client.Scripts;

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
