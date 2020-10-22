using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.StolenVehicle.Missions;
using Curiosity.MissionManager.Client;
using System;
using System.Collections.Generic;

namespace Curiosity.StolenVehicle
{
    public class MissionManager : BaseScript
    {
        internal static List<Blip> drawnBlips = new List<Blip>();
        internal static List<Vehicle> spawnedVehicles = new List<Vehicle>();

        public MissionManager()
        {
            EventHandlers["onClientResourceStop"] += new Action<string>(OnClientResourceStop);

            Func.RegisterMission(typeof(Tezeract));
        }

        private void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            foreach (var Blip in drawnBlips) Blip.Delete();
            foreach (var Vehicle in spawnedVehicles) Vehicle.MarkAsNoLongerNeeded();
        }
    }
}
