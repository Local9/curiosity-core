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
        public Dictionary<int, MissionDataPed> NetworkPeds { get; set; } = new Dictionary<int, MissionDataPed>();
        public Dictionary<int, MissionDataVehicle> NetworkVehicles { get; set; } = new Dictionary<int, MissionDataVehicle>();
        public DateTime Creation { get; set; } = DateTime.Now;
        public bool AssistanceRequested { get; set; }
        public bool IsCompleted { get; set; }

        public override string ToString()
        {
            return $"Mission: {ID}, OH: {OwnerHandleId}, P: {PartyMembers.Count}, NV: {NetworkVehicles.Count}, NP: {NetworkPeds.Count}, Assistance: {AssistanceRequested}";
        }

        public bool AddMember(int playerHandle)
        {
            if (PartyMembers.Contains(playerHandle)) return false;

            PartyMembers.Add(playerHandle);
            return true;
        }

        public bool AddNetworkVehicle(int networkId, bool isTowable)
        {
            if (NetworkVehicles.ContainsKey(networkId))
            {
                NetworkVehicles[networkId].IsTowable = isTowable;
            }
            else
            {
                MissionDataVehicle missionDataVeh = new MissionDataVehicle();
                missionDataVeh.IsTowable = isTowable;

                NetworkVehicles.Add(networkId, missionDataVeh);
            }

            return true;
        }

        public bool AddNetworkPed(int networkId, bool isSuspect)
        {
            if (NetworkPeds.ContainsKey(networkId))
            {
                NetworkPeds[networkId].IsSuspect = isSuspect;
            }
            else
            {
                MissionDataPed missionDataPed = new MissionDataPed();
                missionDataPed.IsSuspect = isSuspect;

                NetworkPeds.Add(networkId, missionDataPed);
            }
            
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
            if (!NetworkVehicles.ContainsKey(newtworkId)) return false;

            NetworkVehicles.Remove(newtworkId);
            return true;
        }

        public bool RemoveNetworkPed(int networkId)
        {
            if (!NetworkPeds.ContainsKey(networkId)) return false;

            NetworkPeds.Remove(networkId);
            return true;
        }
    }
}
