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
            AddMeter(new Vector3(-764.895f, -154.1835f, 37.46028f), VehicleHash.Baller, new Vector3(-762.1651f, -150.9136f, 37.26218f), 205.1401f);
            AddMeter(new Vector3(-733.3779f, -133.2461f, 37.4393f), VehicleHash.BestiaGTS, new Vector3(-735.2828f, -136.1845f, 37.21799f), 33.72361f);
            AddMeter(new Vector3(351.6434f, -946.4038f, 29.43203f), VehicleHash.BType, new Vector3(346.4091f, -950.9089f, 29.42352f), 312.8323f);
            AddMeter(new Vector3(374.8358f, -946.3287f, 29.43777f), VehicleHash.Buffalo, new Vector3(371.1099f, -951.7784f, 29.3634f), 317.3116f);
            AddMeter(new Vector3(235.9043f, -861.3858f, 29.84077f), VehicleHash.CarbonRS, new Vector3(236.3051f, -857.7656f, 29.71272f), 247.9213f);
            AddMeter(new Vector3(201.173f, -848.5595f, 30.71731f), VehicleHash.Elegy, new Vector3(202.1539f, -845.5259f, 30.57161f), 249.8012f);
            AddMeter(new Vector3(178.7899f, -1008.817f, 29.32871f), VehicleHash.Infernus, new Vector3(179.976f, -1013.822f, 29.29109f), 24.77328f);
            AddMeter(new Vector3(189.473f, -1012.637f, 29.31478f), VehicleHash.Kuruma, new Vector3(190.7037f, -1017.019f, 29.25858f), 17.55031f);
        }

        static void AddMeter(Vector3 meterPosition, VehicleHash vehicleHash, Vector3 vehiclePosition, float vehicleHeading)
        {
            ParkingMeter parkingMeter = new ParkingMeter(meterPosition);
            parkingMeter.ParkingMeterVehicle = new ParkingMeterVehicle(vehicleHash, vehiclePosition, vehicleHeading);
            ParkingMetersCity.Add(parkingMeter);
        }
    }
}
