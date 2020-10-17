using System.Collections.Generic;

namespace Curiosity.Global.Shared.Data
{
    public class CalloutMessage
    {
        public string Name { get; set; }
        public bool Success { get; set; }
        public bool IsCalloutFinished { get; set; }
        public int NumberRescued { get; set; }
        public CalloutType CalloutType { get; set; }
        public List<CalloutPed> calloutPed = new List<CalloutPed>();
        public List<CalloutVehicle> calloutVehicle = new List<CalloutVehicle>();
        public int NumberArrested { get; set; }
    }

    public class CalloutVehicle
    {
        public bool IsDestroyed { get; set; }
        public bool IsImpounded { get; set; }
        public bool IsStolen { get; set; }

        public List<CalloutItem> Items = new List<CalloutItem>();
    }

    public class CalloutPed
    {
        public string Name { get; set; }

        public bool IsAlive { get; set; }
        public bool IsDrunk { get; set; }
        public bool IsUnderInfluence { get; set; }

        public List<CalloutItem> Items = new List<CalloutItem>();
    }

    public class CalloutItem
    {
        public bool IsIllegal { get; set; }
        public string Label { get; set; }
    }

    public enum CalloutType
    {
        HOSTAGE_RESCUE,
        STOLEN_VEHICLE,
        PARKING_VIOLATION,
        CYCLIST_ON_FREEWAY
    }
}
