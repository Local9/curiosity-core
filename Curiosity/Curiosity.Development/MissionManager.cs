using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Development.Missions;
using Curiosity.MissionManager.Client;
using Curiosity.Shared.Client.net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Development
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

            Func.RegisterMission(typeof(SpawnPed));
            Func.RegisterMission(typeof(SpawnVehicleDamaged));
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
                Log.Verbose("Register Tick Handler");

                Tick += action;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        internal void DeregisterTickHandler(Func<Task> action)
        {
            try
            {
                Log.Verbose("Deregister Tick Handler");

                Tick -= action;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}
