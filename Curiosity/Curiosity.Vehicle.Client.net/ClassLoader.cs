﻿using Curiosity.Shared.Client.net;
using Curiosity.Vehicle.Client.net.Classes;
using Curiosity.Vehicles.Client.net.Classes.CurPlayer;

namespace Curiosity.Vehicles.Client.net
{
    /// <summary>
    /// For initialization of all these static classes
    /// May want to split this into multiple more specific files later
    /// </summary>
    static class ClassLoader
    {
        static Plugin client = Plugin.GetInstance();

        public static void Init()
        {
            Log.Verbose("Entering ClassLoader Init");

            PlayerInformation.Init();

            Classes.CuriosityVehicle.BrakeSignals.Init();
            Classes.CuriosityVehicle.CruiseControl.Init();
            // Classes.Vehicle.DisableAirControls.Init();
            Classes.CuriosityVehicle.EngineManager.Init();
            Classes.CuriosityVehicle.FuelManager.Init();
            Classes.CuriosityVehicle.VehicleDamage.Init();
            Classes.CuriosityVehicle.DeleteVehicle.Init();
            Classes.CuriosityVehicle.VehicleBlip.Init();
            VehicleCreator.Init();

            // Environment
            Classes.Environment.VehicleSpawnMarkerHandler.Init();
            Classes.Environment.BlipHandler.Init();
            Classes.Environment.SafeZone.Init();
            Classes.Environment.ChatCommands.Init();
            Classes.Environment.VehicleTicks.Init();

            // Instances
            Classes.CuriosityVehicle.Spawn.Init();

            // MENU
            Classes.Menus.VehicleSpawn.Init();
            Classes.Menus.DonatorVehicles.Init();
            Classes.Menus.Staff.Init();

            Log.Verbose("Leaving ClassLoader Init");
        }
    }
}