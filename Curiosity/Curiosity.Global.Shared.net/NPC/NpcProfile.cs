using Curiosity.Global.Shared.net.Data;
using Newtonsoft.Json;
using System;

namespace Curiosity.Global.Shared.net.NPC
{
    public class NpcProfile
    {
        [JsonIgnore] public readonly static Random Random = new Random(DateTime.Now.Millisecond);

        [JsonIgnore] int _gender = 0;

        [JsonIgnore] public string FirstName;
        [JsonIgnore] public string LastName;
        [JsonIgnore] public DateTime DateOfBirth;

        [JsonIgnore] public int Gender { get; internal set; }

        public bool IsSearchable { get; internal set; }
        public bool IsUnderAlcaholInfluence { get; internal set; }
        public bool IsArrestable { get; internal set; }
        public bool IsWanted { get; internal set; }


        // incidentflags
        // DRUGS
        public bool IsUsingCannabis { get; internal set; }
        public bool IsUsingCocaine { get; internal set; }
        public bool IsUsingHerion { get; internal set; }
        public bool IsUsingMeth { get; internal set; }
        // Questionable Flags
        public bool HasLostId { get; internal set; }
        // Wanted Flags

        [JsonIgnore] public int BloodAlcaholLimit { get; internal set; } = 0;
        [JsonIgnore] public int RiskLevel { get; internal set; } = 0;
        [JsonIgnore] public int RiskOfFlee { get; internal set; } = 0;
        [JsonIgnore] public int RiskOfShootAndFlee { get; internal set; } = 0;

        [JsonIgnore]
        public string DOB
        {
            get
            {
                return DateOfBirth.ToString("yyyy-MM-dd");
            }
        }

        public NpcProfile(int gender)
        {

            Gender = gender > 1 ? 1 : gender;

            if (Gender == 1) // FEMALE
            {
                FirstName = PedNames.FirstNameFemale[Random.Next(PedNames.FirstNameFemale.Count)];
            }
            else
            {
                FirstName = PedNames.FirstNameMale[Random.Next(PedNames.FirstNameMale.Count)];
            }
            LastName = PedNames.Surname[Random.Next(PedNames.Surname.Count)];

            DateTime StartDateForDriverDoB = new DateTime(1949, 1, 1);
            double Range = (DateTime.Today - StartDateForDriverDoB).TotalDays;
            Range = Range - 6570; // MINUS 18 YEARS
            DateOfBirth = StartDateForDriverDoB.AddDays(Random.Next((int)Range));
        }
    }
}
