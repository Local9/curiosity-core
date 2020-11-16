using CitizenFX.Core;

namespace Curiosity.ParkingMeters.Models
{
    public class ParkingMeter
    {
        public ParkingMeterVehicle ParkingMeterVehicle;
        public Vector3 Position;

        public ParkingMeter(Vector3 position)
        {
            this.Position = position;
        }
    }

    public class ParkingMeterVehicle
    {
        public VehicleHash Vehicle;
        public Vector3 Position;
        public float Heading;

        public ParkingMeterVehicle(VehicleHash vehicle, Vector3 position, float heading)
        {
            this.Vehicle = vehicle;
            this.Position = position;
            this.Heading = heading;
        }
    }
}
