using System;

namespace Curiosity.Systems.Library.Models
{
    public class VehicleItem
    {
        public int CharacterVehicleId;
        public int NetworkId;
        public string Label;
        public string Hash;
        public DateTime DatePurchased;
        public VehicleInfo VehicleInfo = new VehicleInfo();
    }
}
