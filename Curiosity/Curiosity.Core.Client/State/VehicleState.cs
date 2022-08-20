﻿using Curiosity.Core.Client.Managers;

namespace Curiosity.Core.Client.State
{
    public enum eVehicleStateType
    {
        Vehicle,
        Boat,
        Plane,
        Helicopter,
        Trailer
    }

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

        public eVehicleStateType eVehicleStateType;

        public async void ToggleLock(bool lockDoors)
        {
            if (Vehicle != null && Vehicle.Exists())
            {
                for (int i = 0; i < 2; i++)
                {
                    int timer = GetGameTimer();
                    while (GetGameTimer() - timer < 50)
                    {
                        SoundVehicleHornThisFrame(Vehicle.Handle);
                        await BaseScript.Delay(0);
                    }
                    await BaseScript.Delay(50);
                }

                if (lockDoors)
                {
                    NotificationManager.GetModule().Info($"Doors are now locked");
                    SetVehicleDoorsLockedForAllPlayers(Vehicle.Handle, true);
                }
                else
                {
                    NotificationManager.GetModule().Info($"Doors are now unlocked");
                    SetVehicleDoorsLockedForAllPlayers(Vehicle.Handle, false);
                }
            }
        }

        public VehicleState(Vehicle vehicle)
        {
            this.Vehicle = vehicle;
            this.Created = DateTime.Now;
        }

        public Prop AttachedProp { get; internal set; }
    }
}
