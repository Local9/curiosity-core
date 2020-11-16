using CitizenFX.Core;
using Curiosity.ParkingMeters.Models;
using System.Collections.Generic;

namespace Curiosity.ParkingMeters.Data
{
    public class ParkingMeterData : BaseScript
    {
        public static List<ParkingMeter> ParkingMetersCity = new List<ParkingMeter>();
        public static List<ParkingMeter> ParkingMetersCounty = new List<ParkingMeter>();

        public ParkingMeterData()
        {
            AddMeter(new Vector3(-732.473f, -209.3104f, 37.23155f), VehicleHash.Adder, new Vector3(-729.9196f, -206.8528f, 37.06701f), 221.0322f);
            AddMeter(new Vector3(-764.895f, -154.1835f, 37.46028f), VehicleHash.Adder, new Vector3(-762.1651f, -150.9136f, 37.26218f), 205.1401f);
            AddMeter(new Vector3(-733.3779f, -133.2461f, 37.4393f), VehicleHash.Adder, new Vector3(-735.2828f, -136.1845f, 37.21799f), 33.72361f);
        }

        static void AddMeter(Vector3 meterPosition, VehicleHash vehicleHash, Vector3 vehiclePosition, float vehicleHeading)
        {
            ParkingMeter parkingMeter = new ParkingMeter(meterPosition);
            parkingMeter.ParkingMeterVehicle = new ParkingMeterVehicle(vehicleHash, vehiclePosition, vehicleHeading);
            ParkingMetersCity.Add(parkingMeter);
        }
    }
}
