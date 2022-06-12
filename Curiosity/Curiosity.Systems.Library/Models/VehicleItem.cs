using Curiosity.Systems.Library.Enums;
using System;

namespace Curiosity.Systems.Library.Models
{
    public class VehicleItem
    {
        public string Message = string.Empty;
        public int CharacterVehicleId;
        public int NetworkId;
        public string Label;
        public string Hash;
        public DateTime DatePurchased;
        public DateTime? DateDeleted;
        public SpawnType SpawnTypeId;
        public VehicleInfo VehicleInfo = new VehicleInfo();

        public int ServerHandle;
        public float Heading;
        public float X;
        public float Y;
        public float Z;

        public long BuyBackValue;
        public bool TicketsOutstanding;
        public bool TicketsOverdue;
    }
}
