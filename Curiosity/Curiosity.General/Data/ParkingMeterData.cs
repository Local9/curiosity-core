using CitizenFX.Core;
using Curiosity.ParkingMeters.Models;
using System.Collections.Generic;

namespace Curiosity.ParkingMeters.Data
{
    public class ParkingMeterData : BaseScript
    {
        static List<ParkingMeter> parkingMeters = new List<ParkingMeter>();

        public ParkingMeterData()
        {
            ParkingMeter parkingMeter1 = new ParkingMeter();
            parkingMeter1.Position = new Vector3();
            parkingMeter1.ParkingMeterVehicle = new ParkingMeterVehicle(VehicleHash.Adder, new Vector3(), 0f);
        }
    }
}
