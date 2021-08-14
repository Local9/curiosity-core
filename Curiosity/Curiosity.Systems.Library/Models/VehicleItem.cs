﻿using Curiosity.Systems.Library.Enums;
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
        public SpawnType SpawnTypeId;
        public VehicleInfo VehicleInfo = new VehicleInfo();

        public int ServerHandle { get; set; }
    }
}
