using Curiosity.Shared.Client.net;

namespace Curiosity.GameWorld.Client.net
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

            // Classes.Environment.SeasonSync.Init();
            // Classes.Environment.TimeSync.Init(); // DEPRECATED
            // Classes.Environment.WeatherSystem.Init(); // DEPRECATED

            // Animals
            Classes.Environment.AquaticSpawner.Init();
            Classes.Environment.Birds.Init();


            // EE
            Classes.Environment.EasterEggs.DeveloperWall.Init();

            Log.Verbose("Leaving ClassLoader Init");
        }
    }
}