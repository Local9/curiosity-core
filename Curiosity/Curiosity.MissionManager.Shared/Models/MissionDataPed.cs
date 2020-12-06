using System;

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

        public bool IsSuspect { get; internal set; }
        public bool IsHandcuffed { get; internal set; }
        public override string ToString()
        {
            return $"Ped: Suspect: {IsSuspect}, IsHandcuffed: {IsHandcuffed}, Blip: {AttachBlip}";
        }
    }
}