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

            // Game Code
            Static.Relationships.Init();

            // DATA
            DataClasses.Mission.PoliceStores.Init();

            Classes.Environment.ChatCommands.Init();

            // extras
            Scripts.Extras.Coroner.Init();
            Scripts.Extras.VehicleTow.Init();

            // MISSION HANDLER
            Scripts.Mission.RandomMissionHandler.Init();
            Scripts.MissionEvents.Init();

            Log.Verbose("Leaving ClassLoader Init");
        }
    }
}