namespace Curiosity.Global.Shared.net.Entity
{
    public class ArrestedPedData
    {
        public bool IsAllowedToBeArrested;
        public bool IsDrunk;
        public bool IsDrugged;
        public bool IsCarryingIllegalItems;
        public bool IsDrivingStolenCar;
        public bool CaughtSpeeding;
        public bool IsWanted;
        public bool DispatchJail;
        public bool IsBike;
    }

    public class TrafficStopData
    {
        public bool Ticket;
        public int TicketValue;
    }
}
