using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Entity
{
    class CalloutMessage
    {
        public string Name;
        public bool Success;
        public int NumberKilled;
        public int NumberRescued;
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
}
