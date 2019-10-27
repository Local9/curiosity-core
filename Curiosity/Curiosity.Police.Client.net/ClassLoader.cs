using Curiosity.Shared.Client.net;
using System.Collections.Generic;
using System;

namespace Curiosity.Police.Client.net
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

            Classes.ChatCommands.Init();

            Classes.Player.PlayerInformation.Init();
            Environment.Job.DutyManager.Init();
            Environment.Tasks.CalloutHandler.Init();

            Classes.Menus.MenuLoadout.Init(); // exists to make the instance exist
            Classes.Menus.MenuPoliceOptions.Init();

            Environment.Vehicle.Sirens.Init();
            Curiosity.Global.Shared.net.Data.BlipHandler.Init();
            Environment.Job.DutyMarkers.Init();

            // PULLOVER
            Classes.Pullover.Init();

            // Developer
            Classes.Developer.Init();

            /// INSPIRATION!
            // Police
            //Sirens.Init();
            //WeaponStash.Init();
            //Arrest.Init(); 
            //CivilianCarSirenLights.Init();
            //CustomizationCommands.Init();
            //Helicopter.Init();
            //SkinLoadoutPresets.Init();
            //Slimjim.Init();
            //SpikeStrip.Init();
            //CellDoors.Init();
            //Tackle.Init();
            //PoliceCharacterMenu.Init();
            //PoliceVehicleMenu.Init();
            //DutyManager.Init();

            Log.Verbose("Leaving ClassLoader Init");
        }
    }
}