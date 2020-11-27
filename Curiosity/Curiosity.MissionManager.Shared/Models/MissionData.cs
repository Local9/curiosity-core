using System;
using System.Collections.Generic;

namespace Curiosity.Systems.Shared.Entity
{
    public class MissionData
    {
        public string ID { get; private set; }
        public bool IsMissionUnique { get; private set; }
        public int OwnerHandleId { get; private set; }
        public List<int> PartyMembers { get; private set; }
        public List<int> NetworkVehicles { get; private set; }
        public List<int> NetworkPeds { get; private set; }
        public DateTime Creation { get; private set; }
        public bool AssistanceRequested { get; set; }

        public MissionData(string missionName, int owner, bool unique = false)
        {
            PartyMembers = new List<int>();
            NetworkVehicles = new List<int>();
            NetworkPeds = new List<int>();

            OwnerHandleId = owner;
            ID = missionName;
            IsMissionUnique = unique;

            Creation = DateTime.Now;
        }

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
