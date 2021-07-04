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

            bool personalVehicle = vehicle.State.Get($"{StateBagKey.VEH_PERSONAL}") ?? false;
            if (personalVehicle == true)
            {
                Blip b = Cache.PersonalVehicle.Vehicle.AttachBlip();

                b.Sprite = BlipSprite.PersonalVehicleCar;

                VehicleHash vehicleHash = (VehicleHash)Cache.PersonalVehicle.Vehicle.Model.Hash;

                if (ScreenInterface.VehicleBlips.ContainsKey(vehicleHash))
                {
                    API.SetBlipSprite(b.Handle, ScreenInterface.VehicleBlips[vehicleHash]);
                }
                else
                {
                    if (ScreenInterface.VehicleClassBlips.ContainsKey(Cache.PersonalVehicle.Vehicle.ClassType))
                    {
                        API.SetBlipSprite(b.Handle, ScreenInterface.VehicleClassBlips[Cache.PersonalVehicle.Vehicle.ClassType]);
                    }
                }

                b.Scale = 0.85f;
                b.Color = BlipColor.White;
                b.Priority = 10;
                b.Name = "Personal Vehicle";
                b.IsShortRange = true;
            }

            bool trailer = vehicle.State.Get($"{StateBagKey.VEH_PERSONAL_TRAILER}") ?? false;
            if (trailer == true)
            {
                Blip b = Cache.PersonalTrailer.Vehicle.AttachBlip();
                API.SetBlipSprite(b.Handle, 479);
                b.Scale = 0.85f;
                b.Color = BlipColor.White;
                b.Priority = 10;
                b.Name = "Personal Trailer";
                b.IsShortRange = true;
            }
        }

        public Prop AttachedProp { get; internal set; }
    }
}
