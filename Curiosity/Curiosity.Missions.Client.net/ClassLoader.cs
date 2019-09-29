using Curiosity.Shared.Client.net;
using System;

namespace Curiosity.Missions.Client.net
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

            Classes.PlayerClient.ClientInformation.Init();

            Classes.Environment.ChatCommands.Init();

            // Game Code
            Static.Relationships.Init();

            Log.Verbose("Leaving ClassLoader Init");
        }
    }
}