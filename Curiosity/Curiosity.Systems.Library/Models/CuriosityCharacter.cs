using System;
using System.Collections.Generic;
using System.Linq;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;

namespace Curiosity.Systems.Library.Models
{
    public class CuriosityCharacter
    {
        [JsonIgnore] public string MP0_STAMINA { get { return "MP0_STAMINA"; } }
        [JsonIgnore] public string MP0_SHOOTING_ABILITY { get { return "MP0_SHOOTING_ABILITY"; } }
        [JsonIgnore] public string MP0_STRENGTH { get { return "MP0_STRENGTH"; } }
        [JsonIgnore] public string MP0_STEALTH_ABILITY { get { return "MP0_STEALTH_ABILITY"; } }
        [JsonIgnore] public string MP0_FLYING_ABILITY { get { return "MP0_FLYING_ABILITY"; } }
        [JsonIgnore] public string MP0_WHEELIE_ABILITY { get { return "MP0_WHEELIE_ABILITY"; } }
        [JsonIgnore] public string MP0_LUNG_CAPACITY { get { return "MP0_LUNG_CAPACITY"; } }

        public long CharacterId { get; set; }
        public long UserId { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }
        public long Cash { get; set; }
        public bool MarkedAsRegistered { get; set; }
        public long LocationId { get; set; }
        public int Gender { get; set; } = 0; // Default Gender is Male
        public Position LastPosition { get; set; }
        public CharacterHeritage Style { get; set; } = new CharacterHeritage();
        public Dictionary<string, int> Stats { get; set; } = new Dictionary<string, int>();
        public CharacterAppearance Appearance { get; set; } = new CharacterAppearance();
        public Dictionary<int, float> Features { get; set; } = new Dictionary<int, float>();

        public void ChangeStat(string stat, int value)
        {
            if (Stats.ContainsKey(stat))
            {
                Stats[stat] = value;
            }
            else
            {
                Stats.Add(stat, value);
            }
        }

        public void ChangeFeature(int feature, float value)
        {
            if (Features.ContainsKey(feature))
            {
                Features[feature] = value;
            }
            else
            {
                Features.Add(feature, value);
            }
        }
    }
}