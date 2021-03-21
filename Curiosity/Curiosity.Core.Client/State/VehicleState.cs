using CitizenFX.Core;

namespace Curiosity.Core.Client.State
{
    public class VehicleState
    {
        // Vehicle
        public Vehicle Vehicle;
        public VehicleState AttachedVehicle;

        // Direct Access
        public int Handle => this.Vehicle.Handle;
        public StateBag State => this.Vehicle.State;
        public Model Model => this.Vehicle.Model;
        public Vector3 Position => this.Vehicle.Position;
        public bool IsEngineRunning
        {
            get
            {
                return this.Vehicle.IsEngineRunning;
            }
            set
            {
                this.Vehicle.IsEngineRunning = value;
            }
        }

        public float FuelLevel
        {
            get
            {
                return this.Vehicle.FuelLevel;
            }
            set
            {
                this.Vehicle.FuelLevel = value;
            }
        }

        public float Speed
        {
            get
            {
                return this.Vehicle.Speed;
            }
            set
            {
                this.Vehicle.Speed = value;
            }
        }

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
