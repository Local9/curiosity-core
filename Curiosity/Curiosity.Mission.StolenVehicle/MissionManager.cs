using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.StolenVehicle
{
    public class MissionManager : BaseScript
    {
        internal static MissionManager Instance;
        internal static List<Blip> drawnBlips = new List<Blip>();
        internal static List<Vehicle> spawnedVehicles = new List<Vehicle>();

        public MissionManager()
        {
            Instance = this;

            EventHandlers["onClientResourceStop"] += new Action<string>(OnClientResourceStop);

            // Functions.RegisterMission(typeof(StolenVehicleChase));
            // Functions.RegisterMission(typeof(StolenVehicleChaseTwo));
        }

        private void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            foreach (var Blip in drawnBlips) Blip.Delete();
            foreach (var Vehicle in spawnedVehicles) Vehicle.MarkAsNoLongerNeeded();
        }

        internal void RegisterTickHandler(Func<Task> action)
        {
            try
            {
                Logger.Verbose("Register Tick Handler");

                Tick += action;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        internal void DeregisterTickHandler(Func<Task> action)
        {
            try
            {
                Logger.Verbose("Deregister Tick Handler");

                Tick -= action;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
    }

    internal enum MissionState
    {
        Started,
        ChaseActive,
        Escaped,
        End,
        SuspectDied,
        Error
    }
}
