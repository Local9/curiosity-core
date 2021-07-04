using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Enums;
using System;

namespace Curiosity.Core.Client.State
{
    public class VehicleState
    {
        // Creation
        public DateTime Created { get; private set; }
        // Vehicle
        public Vehicle Vehicle;
        public VehicleState AttachedVehicle;
        // Cruise Control
        public bool CruiseState;
        public bool CruiseReverse;
        public float LastSpeed;
        public float CruiseSpeed;

        public VehicleState(Vehicle vehicle)
        {
            this.Vehicle = vehicle;
            this.Created = DateTime.Now;

            Blip blip = vehicle.AttachBlip();

            int spawnType = vehicle.State.Get($"{StateBagKey.VEH_SPAWN_TYPE}") ?? (int)SpawnType.Vehicle;

            if (spawnType == (int)SpawnType.Vehicle)
            {
                blip.Name = "Personal Vehicle";
            }

            if (spawnType == (int)SpawnType.Trailer)
            {
                blip.Name = "Personal Trailer";
            }

            if (spawnType == (int)SpawnType.Boat)
            {
                blip.Name = "Personal Boat";
            }

            if (spawnType == (int)SpawnType.Plane)
            {
                blip.Name = "Personal Plane";
            }

            if (spawnType == (int)SpawnType.Helicopter)
            {
                blip.Name = "Personal Helicopter";
            }

            VehicleHash vehicleHash = (VehicleHash)vehicle.Model.Hash;

            if (ScreenInterface.VehicleBlips.ContainsKey(vehicleHash))
            {
                API.SetBlipSprite(blip.Handle, ScreenInterface.VehicleBlips[vehicleHash]);
            }
            else
            {
                if (ScreenInterface.VehicleClassBlips.ContainsKey(vehicle.ClassType))
                {
                    API.SetBlipSprite(blip.Handle, ScreenInterface.VehicleClassBlips[vehicle.ClassType]);
                }
            }

            blip.Scale = 0.85f;
            blip.Color = BlipColor.White;
            blip.Priority = 10;
        }

        public Prop AttachedProp { get; internal set; }
    }
}
