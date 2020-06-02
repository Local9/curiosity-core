using System.Collections.Generic;

namespace Curiosity.Server.net.Entity
{
    class CalloutMessage
    {
        public string Name;
        public bool Success;
        public bool IsCalloutFinished;
        public int NumberRescued;
        public CalloutType CalloutType;
        public List<CalloutPed> calloutPed = new List<CalloutPed>();
        public List<CalloutVehicle> calloutVehicle = new List<CalloutVehicle>();
    }

    public class CalloutVehicle
    {
        public bool IsDestroyed;
        public bool IsImpounded;
        public bool IsStolen;

        public List<CalloutItem> Items = new List<CalloutItem>();
    }

    public class CalloutPed
    {
        public string Name;

        public bool IsAlive;
        public bool IsDrunk;
        public bool IsUnderInfluence;

        public List<CalloutItem> Items = new List<CalloutItem>();
    }

    public class CalloutItem
    {
        public bool IsIllegal;
        public string Label;
    }

    public enum CalloutType
    {
        HOSTAGE_RESCUE,
        STOLEN_VEHICLE,
        PARKING_VIOLATION
    }
}
