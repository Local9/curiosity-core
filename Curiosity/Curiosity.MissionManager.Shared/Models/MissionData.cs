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
            string missionInformation = $"Mission: {DisplayName}\nOwner ID: {OwnerHandleId}\nNo. Party Members: {PartyMembers.Count}\nNo. NPC Vehicles: {NetworkVehicles.Count}\nNo. NPCs: {NetworkPeds.Count}\nAssistance: {AssistanceRequested}";

            if (NetworkPeds.Count > 0)
            {
                missionInformation += "\n---- NPCs ----";

                foreach(KeyValuePair<int, MissionDataPed> ped in NetworkPeds)
                {
                    missionInformation += $"\n{ped.Value}";
                }
            }
            if (NetworkVehicles.Count > 0)
            {
                missionInformation += "\n---- VEHs ----";

                foreach (KeyValuePair<int, MissionDataVehicle> veh in NetworkVehicles)
                {
                    missionInformation += $"\n{veh.Value}";
                }
            }

            return missionInformation;
        }

        public bool AddMember(int playerHandle)
        {
            if (PartyMembers.Contains(playerHandle)) return false;

            PartyMembers.Add(playerHandle);
            return true;
        }

        public MissionDataVehicle AddNetworkVehicle(int networkId)
        {
            if (NetworkVehicles.ContainsKey(networkId)) return NetworkVehicles[networkId];

            MissionDataVehicle mdv = new MissionDataVehicle();

            NetworkVehicles.Add(networkId, mdv);

            return mdv;
        }

        public MissionDataPed AddNetworkPed(int networkId, int gender, bool isDriver)
        {
            if (NetworkPeds.ContainsKey(networkId)) return NetworkPeds[networkId];

            MissionDataPed mpd = new MissionDataPed();

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
            mpd.IsDriver = isDriver;

            NetworkPeds.Add(networkId, mpd);

            return mpd;
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
