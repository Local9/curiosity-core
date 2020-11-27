using System;
using System.Collections.Generic;

namespace Curiosity.Systems.Shared.Entity
{
    public class MissionData
    {
        public string ID { get; set; }
        public bool IsMissionUnique { get; set; }
        public int OwnerHandleId { get; set; }
        public List<int> PartyMembers { get; set; } = new List<int>();
        public List<int> NetworkVehicles { get; set; } = new List<int>();
        public List<int> NetworkPeds { get; set; } = new List<int>();
        public DateTime Creation { get; set; } = DateTime.Now;
        public bool AssistanceRequested { get; set; }

        public bool AddMember(int playerHandle)
        {
            if (PartyMembers.Contains(playerHandle)) return false;

            PartyMembers.Add(playerHandle);
            return true;
        }

        public bool AddNetworkVehicle(int newtworkId)
        {
            if (NetworkVehicles.Contains(newtworkId)) return false;

            NetworkVehicles.Add(newtworkId);
            return true;
        }

        public bool AddNetworkPed(int newtworkId)
        {
            if (NetworkPeds.Contains(newtworkId)) return false;

            NetworkPeds.Add(newtworkId);
            return true;
        }

        public bool RemoveMember(int playerHandle)
        {
            if (!PartyMembers.Contains(playerHandle)) return false;

            PartyMembers.Remove(playerHandle);
            return true;
        }

        public bool RemoveNetworkVehicle(int newtworkId)
        {
            if (!NetworkVehicles.Contains(newtworkId)) return false;

            NetworkVehicles.Remove(newtworkId);
            return true;
        }

        public bool RemoveNetworkPed(int newtworkId)
        {
            if (!NetworkPeds.Contains(newtworkId)) return false;

            NetworkPeds.Remove(newtworkId);
            return true;
        }
    }
}
