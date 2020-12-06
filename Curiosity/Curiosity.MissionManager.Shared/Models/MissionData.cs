using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;

namespace Curiosity.Systems.Shared.Entity
{
    public class MissionData
    {
        private const int EIGHTEEN_YEARS_IN_DAYS = 6570;

        public string ID { get; set; }
        public string DisplayName { get; set; }
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

        public bool AddNetworkVehicle(int networkId, bool isTowable, bool attachBlip)
        {
            if (NetworkVehicles.ContainsKey(networkId))
            {
                MissionDataVehicle mdv = NetworkVehicles[networkId];
                mdv.IsTowable = isTowable;
                mdv.AttachBlip = attachBlip;
            }
            else
            {
                MissionDataVehicle mdv = new MissionDataVehicle();
                mdv.IsTowable = isTowable;
                mdv.AttachBlip = attachBlip;

                NetworkVehicles.Add(networkId, mdv);
            }

            return true;
        }

        public bool AddNetworkPed(int networkId, bool isSuspect, bool isHandcuffed, bool attachBlip, int gender)
        {
            if (NetworkPeds.ContainsKey(networkId))
            {
                MissionDataPed mpd = NetworkPeds[networkId];
                mpd.IsHandcuffed = isHandcuffed;
                mpd.IsSuspect = isSuspect;
                mpd.AttachBlip = attachBlip;
            }
            else
            {
                MissionDataPed mpd = new MissionDataPed();
                mpd.IsSuspect = isSuspect;
                mpd.IsHandcuffed = isHandcuffed;
                mpd.AttachBlip = attachBlip;

                mpd.Gender = gender; // 0 = M, 1 = F
                if (mpd.Gender == 0)
                {
                    mpd.Firstname = PedIdentifcationData.FirstNameMale[Utility.RANDOM.Next(PedIdentifcationData.FirstNameMale.Count)];
                }
                else
                {
                    mpd.Firstname = PedIdentifcationData.FirstNameFemale[Utility.RANDOM.Next(PedIdentifcationData.FirstNameFemale.Count)];
                }

                mpd.Surname = PedIdentifcationData.Surname[Utility.RANDOM.Next(PedIdentifcationData.Surname.Count)];

                DateTime StartDateForDriverDoB = new DateTime(1949, 1, 1);
                double Range = (DateTime.Today - StartDateForDriverDoB).TotalDays;
                Range -= EIGHTEEN_YEARS_IN_DAYS; // MINUS 18 YEARS
                mpd.DateOfBirth = StartDateForDriverDoB.AddDays(Utility.RANDOM.Next((int)Range));

                NetworkPeds.Add(networkId, mpd);
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
