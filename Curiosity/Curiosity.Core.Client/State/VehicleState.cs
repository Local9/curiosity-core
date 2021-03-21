using CitizenFX.Core;

namespace Curiosity.Core.Client.State
{
    public class VehicleState
    {
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
        }
    }
}
