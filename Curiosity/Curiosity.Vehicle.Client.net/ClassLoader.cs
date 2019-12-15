using Curiosity.Shared.Client.net;
using System;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Vehicle.Client.net
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

            Classes.Player.PlayerInformation.Init();

            Classes.Vehicle.BrakeSignals.Init();
            Classes.Vehicle.CruiseControl.Init();
            // Classes.Vehicle.DisableAirControls.Init();
            Classes.Vehicle.EngineManager.Init();
            Classes.Vehicle.FuelManager.Init();
            Classes.Vehicle.VehicleDamage.Init();
            Classes.Vehicle.DeleteVehicle.Init();
            Classes.Vehicle.VehicleBlip.Init();

            // Environment
            Classes.Environment.VehicleSpawnMarkerHandler.Init();
            Classes.Environment.BlipHandler.Init();
            Classes.Environment.SafeZone.Init();
            Classes.Environment.ChatCommands.Init();

            // Instances
            Classes.Vehicle.Spawn.Init();

            // MENU
            Classes.Menus.VehicleSpawn.Init();
            Classes.Menus.DonatorVehicles.Init();

            Log.Verbose("Leaving ClassLoader Init");
        }
    }
}