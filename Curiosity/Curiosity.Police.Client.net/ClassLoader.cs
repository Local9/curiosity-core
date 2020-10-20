using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using System;
using System.Threading;

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

            Classes.Menus.MenuLoadout.Init(); // exists to make the instance exist
            Classes.Menus.MenuPoliceOptions.Init();
            Classes.Menus.PoliceDispatchMenu.Init();

            Environment.Job.BackupMessages.Init();

            // Environment.Vehicle.Sirens.Init();
            Curiosity.Global.Shared.Data.BlipHandler.Init();
            Environment.Job.DutyMarkers.Init();

            Environment.Vehicle.PolmavScripts.Init();

            // Developer
            Classes.Developer.Init();
            Environment.WeaponScripts.SharkLauncher.Init();

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

            // Spin();

            Log.Verbose("Leaving ClassLoader Init");
        }

        
    }
}