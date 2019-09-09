using Curiosity.Shared.Client.net;
using CitizenFX.Core;

namespace Curiosity.World.Client.net
{
    /// <summary>
    /// For initialization of all these static classes
    /// May want to split this into multiple more specific files later
    /// </summary>
    static class ClassLoader
    {
        public static void Init()
        {
            Log.Verbose("Entering ClassLoader Init");

            Classes.Player.PlayerInformation.Init();
            Classes.Player.LoadoutManagement.Init();

            Classes.Environment.TimeSync.Init();
            Classes.Environment.WeatherSystem.Init();
            Classes.Environment.WorldLimits.Init();

            // Animals
            Classes.Environment.AquaticSpawner.Init();
            Classes.Environment.Birds.Init();


            // EE
            Classes.Environment.EasterEggs.DeveloperWall.Init();

            // Scenarios
            Classes.Environment.WorldScenarios.Init();

            Log.Verbose("Leaving ClassLoader Init");
        }
    }
}