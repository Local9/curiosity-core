﻿using System;
using System.Collections.Generic;

namespace Curiosity.Systems.Library.Entity
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
        public bool IsUnderInfluence { get; set; }
        public bool IsCarryingIllegalItems { get; set; }
        public bool IsIdentified { get; set; }
        public bool StoleVehicle { get; set; }
        public bool HasCarryLicense { get; set; }
        public float BloodAlcoholLimit { get; set; }
        public bool FleeFromPlayer { get; internal set; }
        public bool HasBeenBreathalysed { get; set; }
        public bool IsWanted { get; internal set; }
        public bool HasBeenSearched { get; set; }
        public bool IsArrested { get; set; }

        public Dictionary<string, bool> Items = new Dictionary<string, bool>();

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
                $"\n IsDriver: {IsDriver}" +
                $"\n Stole Vehicle: {StoleVehicle}" +
                $"\n Illegal Item(s): {IsCarryingIllegalItems}" +
                $"\n Has Carry License: {HasCarryLicense}" +
                $"\n IsWanted: {IsWanted}" +
                $"\n HasBeenBreathalysed: {HasBeenBreathalysed}" +
                $"\n BloodAlcoholLimit: 0.{BloodAlcoholLimit:00}" +
                $"";


            return returnString;
        }
    }
}