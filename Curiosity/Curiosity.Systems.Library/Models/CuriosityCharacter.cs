using Newtonsoft.Json;
using System.Collections.Generic;

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
        public int Health { get; set; }
        public int Armor { get; set; }
        public bool IsDead { get; set; }
        public long Cash { get; set; }
        public bool MarkedAsRegistered { get; set; }
        public int Gender { get; set; } = 0; // Default Gender is Male
        public Position LastPosition { get; set; }
        public CharacterHeritage Heritage { get; set; } = new CharacterHeritage();
        public CharacterAppearance Appearance { get; set; } = new CharacterAppearance();
        public Dictionary<int, float> Features { get; set; } = new Dictionary<int, float>();

        [JsonIgnore] public List<CharacterSkill> Skills = new List<CharacterSkill>();

        [JsonIgnore] public List<CharacterStat> Stats = new List<CharacterStat>();

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