using Curiosity.Shared.Client.net;

namespace Curiosity.Player.Client.net
{
    /// <summary>
    /// For initialization of all these static classes
    /// May want to split this into multiple more specific files later
    /// </summary>
    static class ClassLoader
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            Log.Verbose("Entering ClassLoader Init");

            // Player Spawn
            // Player Respawn
            // Player Roles
            // Player Wallet
            // Player Skills
            // Player Shit in General

            Log.Verbose("Leaving ClassLoader Init");
        }
    }
}