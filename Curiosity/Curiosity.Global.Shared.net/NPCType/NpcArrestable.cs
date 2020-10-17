using Curiosity.Global.Shared.NPC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Global.Shared.NPCType
{
    public class NpcArrestable : NpcProfile
    {
        [JsonIgnore] public string Offence { get; internal set; }
        [JsonIgnore] public string NumberOfCitations { get; internal set; }

        public bool HasBeenSearched;
        public bool HasFoundIllegalItems;
        public bool HasBeenTested;
        public bool HasBeenQuestioned;
        public bool HasBeenCuffed;
        public bool HasBeenCaughtSpeeding;

        public NpcArrestable(bool underInfluence, int gender) : base(gender)
        {
            IsUnderAlcaholInfluence = underInfluence;

            if (IsUnderAlcaholInfluence)
            {
                int limit = Random.Next(60, 100);
                if (limit >= 60)
                {
                    BloodAlcaholLimit = Random.Next(1, 7);
                    if (limit >= 88)
                    {
                        BloodAlcaholLimit = Random.Next(7, 10);
                        RiskOfFlee = Random.Next(25, 30);

                        if (limit >= 95)
                        {
                            BloodAlcaholLimit = Random.Next(10, 20);
                            RiskOfFlee = Random.Next(20, 30);
                            RiskOfShootAndFlee = Random.Next(1, 5);
                        }
                    }
                    IsArrestable = true;
                }
            }

            IsUsingCannabis = (Random.Next(100) >= 65);
            IsUsingCocaine = (Random.Next(100) >= 75);
            IsUsingHerion = (Random.Next(100) >= 85);
            IsUsingMeth = (Random.Next(100) >= 95);
            IsSearchable = (Random.Next(2) == 1);

            IsArrestable = IsUsingCannabis || IsUsingCocaine || IsUsingHerion || IsUsingMeth;

            if (Random.Next(100) >= 75)
            {
                List<string> Offense = new List<string>() { "WANTED BY LSPD", "WANTED FOR ASSAULT", "WANTED FOR UNPAID FINES", "WANTED FOR RUNNING FROM THE POLICE", "WANTED FOR EVADING LAW", "WANTED FOR HIT AND RUN", "WANTED FOR DUI" };
                Offence = $"~r~{Offense[Random.Next(Offense.Count)]}";
                IsArrestable = true;
            }
        }
    }
}
