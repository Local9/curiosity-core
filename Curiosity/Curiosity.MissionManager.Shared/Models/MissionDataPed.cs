using System;
using System.Collections.Generic;

namespace Curiosity.Systems.Shared.Entity
{
    public class MissionDataPed : MissionDataEntity
    {
        public string Firstname { get; set; }
        public string Surname { get; set; }

        public string FullName
        {
            get
            {
                return $"{Firstname} {Surname}";
            }
        }

        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public int Gender { get; set; }

        public List<string> Wants = new List<string>();

        public bool IsDriver { get; set; }
        public bool IsSuspect { get; set; }
        public bool IsHandcuffed { get; set; }
        public bool IsUnderInfluence { get; internal set; }
        public bool IsUsingDrugs { get; internal set; }
        public bool IsCarryingDrugs { get; internal set; }
        public bool IsCarringWeapon { get; internal set; }
        public bool IsIdentified { get; set; }
        public bool StoleVehicle { get; set; }

        public override string ToString()
        {
            string dateOfBirth = DateOfBirth.ToString($"yyyy-MM-dd");
            string gender = Gender == 0 ? "Male" : "Female";

            string returnString = $"Ped:" +
                $"\n Name: {FullName}" +
                $"\n DOB: {dateOfBirth}" +
                $"\n Gender {gender}" +
                $"\n Suspect: {IsSuspect}" +
                $"\n IsHandcuffed: {IsHandcuffed}" +
                $"\n Blip: {AttachBlip}" +
                $"\n Identified: {IsIdentified}" +
                $"\n Stole Vehicle: {StoleVehicle}" +
                $"";


            return returnString;
        }
    }
}